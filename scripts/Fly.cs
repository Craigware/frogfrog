using Godot;

public partial class Fly : CharacterBody2D 
{
    public int FlyId;
    [Signal] public delegate void FlyWasCaughtEventHandler(int flyId, long playerId);

    public override void _Ready()
    {
        if (IsServer()) {
            Rpc(nameof(ApplyVelocity), new Vector2(2100,2000));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        GetNode<RichTextLabel>("PositionDebug").Text = "X:" + Position.X.ToString() + "\nY:" + Position.Y.ToString();
        var col = MoveAndCollide(Velocity * (float)delta);
        if (col != null && IsServer()) {
            if (col.GetCollider() is Player p) {
                EmitSignal(SignalName.FlyWasCaught, new Variant[]{ FlyId, p.PlayerID });
            }
        }
        Velocity = Velocity.MoveToward(Vector2.Zero, 256 * (float)delta);
        var bounds = GetNode<GameLoop>("/root/Main").WorldBounds;
        
        // Bounce Wall
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
    }
 
    public bool IsServer() {
        if (GetTree().GetMultiplayer().IsServer()) return true;
        return false;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ApplyVelocity(Vector2 velocity) {
        Velocity = velocity;
    }

    public Godot.Collections.Dictionary<string, Variant> GetRPCDict() {
        return new() {
            {"Position", Position},
        };
    }
}