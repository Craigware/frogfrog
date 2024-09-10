using System;
using Godot;
public partial class Player : CharacterBody2D
{
    public long PlayerID;
    public bool Connected;
    public bool Dead = false;

    public bool ReadyCheck = false;
    [Signal] public delegate void ReadyCheckUpdatedEventHandler(bool ready);

    bool inputVectorHeld;
    Vector2 lastInputDirection;
    Vector2 StoredForce;

    float ToungeStoredForce;
    int ToungeSize = 32*10;

    public override void _Input(InputEvent @event) {
        if (!IsActivePlayer()) return;
        if (Input.IsActionJustReleased("Click")) {
            var mousePos = GetViewport().GetMousePosition();
            var direction = Position.DirectionTo(mousePos);
            var target = ToungeStoredForce * direction * ToungeSize;

            var targetWorldPos = Position + target;
            // Do tounge attack thing with this like PEW PEW BOOM BLOAW BOOM CRRRRSSAAAHSHH
        }
    }

    public override void _PhysicsProcess(double delta) {
        GetNode<RichTextLabel>("PositionDebug").Text = "X:" + Position.X.ToString() + "\nY:" + Position.Y.ToString();
        if (IsActivePlayer()) GetNode<RichTextLabel>("PositionDebug").Text += "\n THIS IS ME";
        MoveAndSlide();
        Velocity = Velocity.MoveToward(Vector2.Zero, 256 * (float)delta);
        var bounds = GetNode<GameLoop>("/root/Main").WorldBounds;
        
        // Bounce
        if (Position.X > bounds.X || Position.Y > bounds.Y || Position.Y < 0 || Position.X < 0) {
            Vector2 stuckProtection = Position;
            if (Position.X > bounds.X) {
                stuckProtection.X = bounds.X;
            } else if (Position.X < 0) {
                stuckProtection.X = 0;
            }

            if (Position.Y > bounds.Y) {
                stuckProtection.Y = bounds.Y;
            } else if (Position.Y < 0) {
                stuckProtection.Y = 0;
            }

            Position = stuckProtection;
            Velocity = -Velocity * 0.9f;
        }

        if (!IsActivePlayer()) return;
        // -------------- IF THE PLAYERID IS THE CLIENTID ------------------ //
        if (Input.IsActionPressed("Click")) {
             ToungeStoredForce += 1 * (float)delta;
        } else {
            ToungeStoredForce = 0;
        }

        if (Velocity != Vector2.Zero) return;

        var inputVector = Input.GetVector("Left", "Right", "Up", "Down");
        if (inputVector != Vector2.Zero && Velocity == Vector2.Zero) {
            inputVectorHeld = true;
            lastInputDirection = inputVector;
            StoredForce += inputVector * 3 * (float)delta;
            StoredForce.X = Math.Clamp(StoredForce.X, -1, 1);
            StoredForce.Y = Math.Clamp(StoredForce.Y, -1, 1);
        } else {
            if (inputVectorHeld) {
                Rpc(nameof(ApplyVelocity), StoredForce * 400);
            }
            inputVectorHeld = false;
            StoredForce = Vector2.Zero;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ToungeAttack(Vector2 targetPosition) {
    }

    public void AnimateTounge() {
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ApplyVelocity(Vector2 velocity) {
        Velocity = velocity;
    }

    public bool IsActivePlayer() {
        if (PlayerID == GetTree().GetMultiplayer().GetUniqueId()) {
            return true;
        }
        return false;
    }

    public Godot.Collections.Dictionary<string, Variant> GetRPCDict() {
        return new() {
            {"Position", Position},
            {"PlayerID", PlayerID},
            {"Connected", Connected}
        };
    }

    public void ReadyButtonPressed() {
        Rpc(nameof(UpdateReadyCheck));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void UpdateReadyCheck() {
        ReadyCheck = !ReadyCheck;
        EmitSignal(SignalName.ReadyCheckUpdated, ReadyCheck);
    }
}