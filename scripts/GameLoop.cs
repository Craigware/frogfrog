using System;
using System.Security.Cryptography.X509Certificates;
using Godot;

public partial class GameLoop : Node
{
    public enum Phases {
        LOBBY,
        LOOP,
        END
    }
    public Phases Phase;

    MultiplayerApi multiplayerApi;
    Node PlayerContainer;
    Node FlyContainer;
    
    PackedScene PlayerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
    PackedScene FlyScene = GD.Load<PackedScene>("res://scenes/Fly.tscn");

    public Godot.Collections.Array<Player> Players = new();
    public Godot.Collections.Array<Fly> Flies = new();
    public Godot.Collections.Dictionary<long, int> Scores;

    
    public Vector2 WorldBounds = new(640,640);
    int ScoreToWin;    

    double lastPingTime;
    public double Ping;

    public override void _Ready() {
        multiplayerApi = GetTree().GetMultiplayer();
        PlayerContainer = GetNode("Players");
        FlyContainer = GetNode("Flies");
        Phase = Phases.LOBBY;
        if (multiplayerApi.IsServer()) {
            SetupServer();
        } else {
            Timer pingTimer = new() {
                Autostart = true,
                WaitTime = 1
            };
            pingTimer.Timeout += () => { SetPing(1); };
            AddChild(pingTimer);
        }
    }

    public void SetupServer() {
        multiplayerApi.PeerConnected += PlayerConnected;
        multiplayerApi.PeerDisconnected += PlayerDisconnected;
        GD.Print("Server peer connection signals connected.");
    }

    public void PlayerConnected(long id) {
        if (Players.Count >= (int)ProjectSettings.GetSetting("connections/server/max_clients") || Phase == Phases.LOOP) {
            foreach (Player p in Players) {
                if (p.Connected == false) {
                    p.Connected = true;
                    p.PlayerID = id;
                    return;
                }
            }
            return;
        }
        var random = new Random().Next(0,640);
        var args = new Variant[2] {
            id,
            new Vector2(random,random)
        };
        Rpc(nameof(SpawnPlayer), args);
        RpcId(id, nameof(SyncState), GetState());
    }

    public void PlayerDisconnected(long id) {
        if (Phase == Phases.LOBBY) {
            Rpc(nameof(RemovePlayer), id);
        }

        foreach (Player player in Players) {
            if (player.PlayerID == id) {
                player.Connected = false;
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void RemovePlayer(long id) {
        Player p = GetPlayerByID(id);
        if (p != null) {
            p.QueueFree();
            Players.Remove(p);
        }
    }

    public Player GetPlayerByID(long id) {
        foreach (Player player in Players) {
            if (player.PlayerID == id) {
                return player;
            }
        }
        return null;
    }

    public Godot.Collections.Dictionary<string, Variant> GetState() {
        Godot.Collections.Dictionary<string, Variant> state = new();
        Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> playerState = new();
        foreach (Player p in Players) {
            playerState.Add(p.GetRPCDict());
        }
        state.Add("Players", playerState);
        return state;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public void SyncState(Godot.Collections.Dictionary<string, Variant> state) {
        GD.Print("Sync State: ", multiplayerApi.IsServer());
        Players = new();

        for (int i = 0; i < PlayerContainer.GetChildCount(); i++) {
            PlayerContainer.RemoveChild(PlayerContainer.GetChild(0));
        }
        
        foreach (Godot.Collections.Dictionary playerState in (Godot.Collections.Array)state["Players"]) {
            var player = PlayerScene.Instantiate<Player>();
            player.PlayerID = (long)playerState["PlayerID"];
            player.Position = (Vector2)playerState["Position"];
            player.Connected = (bool)playerState["Connected"];
            player.Name = player.PlayerID.ToString();

            PlayerContainer.AddChild(player);
            Players.Add(player);
            GD.Print("Player Synced: ", playerState);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void SpawnPlayer(long id, Vector2 position) {
        var player = PlayerScene.Instantiate<Player>();
        player.PlayerID = id;
        player.Connected = true;
        player.Name = id.ToString();
        player.Position = position;
        if (multiplayerApi.IsServer()) {
            player.ReadyCheckUpdated += CheckCombatStart;
        }
        PlayerContainer.AddChild(player);
        Players.Add(player);
    }

    public void CheckCombatStart(bool something) {
        int readyCount = 0;
        foreach (Player p in Players) {
            if (p.ReadyCheck) readyCount++;
        }

        if (readyCount == Players.Count) {
            if (multiplayerApi.IsServer()) {
                BeginCombat();
            }
        }
    }

    public void BeginCombat() {
        Rpc(nameof(UpdateGamePhase), (int)Phases.LOOP);
        Rpc(nameof(SpawnFly), new Vector2(320,320));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void SpawnFly(Vector2 spawnLocation) {
        Fly fly = FlyScene.Instantiate<Fly>();
        fly.Position = spawnLocation;
    
        if (multiplayerApi.IsServer()) {
            fly.FlyWasCaught += (int flyId, long playerId) => {
                Rpc(nameof(PlayerCaughtFly), new Variant[]{ flyId, playerId });
            };
        }

        Flies.Add(fly);
        fly.FlyId = Flies.IndexOf(fly);
        FlyContainer.AddChild(fly);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void PlayerCaughtFly(int flyId, long playerId) {
        Fly caughtFly = Flies[flyId];
        caughtFly.QueueFree();
        Scores[playerId]++;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void UpdateGamePhase(Phases newPhase) {
        Phase = newPhase;
        if (Phase == Phases.LOOP) {
            Scores = new();
            foreach (Player p in Players) {
                Scores.Add(p.PlayerID, 0);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void PlayerVictory(long playerId) {
        GD.Print("Player ", playerId, " won.");
        Rpc(nameof(UpdateGamePhase), (int)Phases.LOBBY);
    }

    public void SetPing(long id) {
        lastPingTime = Time.GetUnixTimeFromSystem();
        RpcId(id, nameof(Pong));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void Pong() {
        var sender = multiplayerApi.GetRemoteSenderId();
        RpcId(sender, nameof(UpdatePing));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void UpdatePing() {
        Ping = Time.GetUnixTimeFromSystem() - lastPingTime;
    }
}

