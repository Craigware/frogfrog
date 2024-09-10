using Godot;

public partial class Connections : Node {
    public override void _Ready()
    { 
    }

    public void UpdateScene() {
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }

    public void CreateServer() {
        var multiplayer = GetTree().GetMultiplayer();
        var peer = new ENetMultiplayerPeer();
        var err = peer.CreateServer((int)ProjectSettings.GetSetting("connections/server/port"), (int)ProjectSettings.GetSetting("connections/server/max_clients"));
        if (err == Error.Ok) {
            multiplayer.MultiplayerPeer = peer;
            GD.Print("Server successfully created.");
            CallDeferred(nameof(UpdateScene));
        } else {
            GD.Print("Failed to create server. " + err);
        }
    }

    public void CreateClient(string ip, int port) {
        var multiplayer = GetTree().GetMultiplayer();
        var peer = new ENetMultiplayerPeer();
        var err = peer.CreateClient(ip, port);
        if (err == Error.Ok) {
            UpdateScene();
            multiplayer.MultiplayerPeer = peer;
            GD.Print("Client successfully created.");
            return;
        } else {
            GD.Print("Failed to create client. " + err);
        }
    }

    public void AttemptToConnect() {
    }

    public void ConnectionFailed() {
    }

    public void ConnectionSuccess() {
    }
}