using System.Collections.Generic;

[System.Serializable]
public struct NavigationCell {
    public bool IsWalkable;
    public Dictionary<Direction, bool> passableConnections;
    public NavigationCell(bool isWalkable) {
        IsWalkable = isWalkable;
        passableConnections = new Dictionary<Direction, bool>
      {
           { Direction.Up, false},
           {Direction.Down, false},
           {Direction.Left, false},
           {Direction.Right, false},
       };
    }
}