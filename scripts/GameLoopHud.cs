using Godot;
using System;

public partial class GameLoopHud : Control
{
    GameLoop gl;
    RichTextLabel label;
    public override void _Ready() {
        label = GetNode<RichTextLabel>("Debug");
        gl = GetNode<GameLoop>("/root/Main");
    }

    public override void _PhysicsProcess(double delta) {
        UpdateDebug();
    }

    public void UpdateDebug() {
        label.Text = "FPS: " + Engine.GetFramesPerSecond().ToString() + "\n";
        label.Text += gl.Phase.ToString() + "\n";
        label.Text += gl.Players.Count + "/" + ProjectSettings.GetSetting("connections/server/max_clients").ToString() + "\n";
        int readyCount = 0;
        foreach (var player in gl.Players) {
            if (player.ReadyCheck) readyCount++;
            label.Text += player.PlayerID.ToString() + "\n";
        }
        label.Text += readyCount.ToString() + "/" + gl.Players.Count + "\n";

        var isServ = GetNode<RichTextLabel>("IsServer");
        if (GetTree().GetMultiplayer().IsServer()) isServ.Text = "THIS IS THE SERVER";
        GetNode<RichTextLabel>("Ping").Text = "Ping: " +  Math.Round(gl.Ping, 2).ToString();
    }

    public void ReadyButton() {
        foreach (Player p in gl.Players) {
            if (p.IsActivePlayer()) {
                p.ReadyButtonPressed();
            }    
        }
    }
}
