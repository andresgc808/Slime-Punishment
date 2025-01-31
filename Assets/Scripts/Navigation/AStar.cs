using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStar {
    public static List<Vector2Int> FindPath(NavigationCell[,] grid, Vector2Int start, Vector2Int target) {
        if (grid == null) {
            Debug.LogError("FindPath: Navigation grid is null.");
            return null;
        }
        if (start.x < 0 || start.x >= grid.GetLength(0) || start.y < 0 || start.y >= grid.GetLength(1) ||
            target.x < 0 || target.x >= grid.GetLength(0) || target.y < 0 || target.y >= grid.GetLength(1)) {
            Debug.LogWarning($"FindPath: Start or Target position is out of bounds, start: {start}, target: {target}");
            return null;
        }
        if (!grid[start.x, start.y].IsWalkable || !grid[target.x, target.y].IsWalkable) {
            Debug.LogWarning($"FindPath: Start or Target position is not walkable, start: {start}, target: {target}");
            return null;
        }


        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        Node startNode = new Node(start, null, 0, CalculateHeuristic(start, target));
        openSet.Add(startNode);

        while (openSet.Count > 0) {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].FCost < currentNode.FCost || (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.Position == target) {
                return RetracePath(currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode, grid, target)) {
                if (closedSet.Contains(neighbor)) {
                    continue;
                }

                int newCostToNeighbor = currentNode.GCost + 1;

                if (newCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor)) {
                    neighbor.GCost = newCostToNeighbor;
                    neighbor.HCost = CalculateHeuristic(neighbor.Position, target);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor)) {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        return null; //No path found
    }

    private static List<Vector2Int> RetracePath(Node endNode) {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != null) {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }

    private static List<Node> GetNeighbors(Node node, NavigationCell[,] grid, Vector2Int target) {
        List<Node> neighbors = new List<Node>();

        int x = node.Position.x;
        int y = node.Position.y;
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        int[,] neighborDirections = {
             { 0, 1 },  // Up
             { 0, -1 }, // Down
             { -1, 0 }, // Left
             { 1, 0 }   // Right
          };

        Direction[] directions = {
             Direction.Up,
             Direction.Down,
             Direction.Left,
             Direction.Right
         };


        for (int i = 0; i < neighborDirections.GetLength(0); i++) {
            int newX = x + neighborDirections[i, 0];
            int newY = y + neighborDirections[i, 1];

            if (newX >= 0 && newX < width && newY >= 0 && newY < height && grid[x, y].passableConnections[directions[i]]) {

                neighbors.Add(new Node(new Vector2Int(newX, newY), node, node.GCost + 1, CalculateHeuristic(new Vector2Int(newX, newY), target)));
            }
        }
        return neighbors;
    }

    private static int CalculateHeuristic(Vector2Int start, Vector2Int target) {
        //Manhattan distance
        return Math.Abs(start.x - target.x) + Math.Abs(start.y - target.y);
    }
    private class Node {
        public Vector2Int Position { get; private set; }
        public Node Parent { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;

        public Node(Vector2Int position, Node parent, int gCost, int hCost) {
            Position = position;
            Parent = parent;
            GCost = gCost;
            HCost = hCost;
        }
        public override bool Equals(object obj) {
            return obj is Node node && Position == node.Position;
        }

        public override int GetHashCode() {
            return Position.GetHashCode();
        }
    }
}