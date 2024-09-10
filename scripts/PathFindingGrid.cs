using System;
using Godot;

public partial class PathFindingGrid : Node {
    Vector2 GridSize;
    Vector2 Origin;
    PathNode[] Grid;

    public PathFindingGrid() {

    }

    void SetupGrid() {

    }

    void AssignValues(Vector2 start, Vector2 target) {
        foreach (var node in Grid) {
            node.g = (int)Math.Round(node.position.DistanceTo(start) * 10);
            node.h = (int)Math.Round(node.position.DistanceTo(target) * 10);
        }
    }

    void PathFindToPoint(Vector2 start, Vector2 target) {
    
    }
}