using Godot;
using System;

public partial class MainMenu : Control
{
    public void Play() {
        GetNode<Connections>("/root/Connections").CreateServer();
    }

    public void Settings() {
         GetNode<Connections>("/root/Connections").CreateClient("127.0.0.1", 4207);
    }

    public void Quit() { GetTree().Quit(); }
}
