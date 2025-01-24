using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC : MonoBehaviour {

    [System.Serializable]
    public class TileData {
        public string name;
        public GameObject prefab;
        public string[] allowedNeighbors;  // Array of the names of tiles that can be adjacent to this one.
    }
    public int width = 30;
    public int height = 30;
    public TileData[] tiles;
    private Dictionary<string, int> tileIndexMap;
    private int[,] grid; // Stores the final tile index
    private Dictionary<(int, int), List<int>> possibilitiesMap; // Stores the possiblities of each grid cell.


    void Start() {
        Initialize();
        GenerateMap();
    }

    void Initialize() {
        tileIndexMap = new Dictionary<string, int>();
        for (int i = 0; i < tiles.Length; i++) {
            tileIndexMap.Add(tiles[i].name, i);
        }
        grid = new int[width, height];
        possibilitiesMap = new Dictionary<(int, int), List<int>>();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                possibilitiesMap.Add((x, y), Enumerable.Range(0, tiles.Length).ToList()); // every cell can start as any tile
                grid[x, y] = -1; // start the grid as empty cells
            }
        }
    }
    void GenerateMap() {
        bool success = WaveFunctionCollapse();
        if (success) {
            CreateVisuals();
        } else {
            Debug.LogError("WFC failed to generate");
        }
    }

    bool WaveFunctionCollapse() {
        while (true) {
            // 1. Find the lowest entropy cell
            (int, int) lowestEntropyCell = FindLowestEntropyCell();
            if (lowestEntropyCell.Item1 == -1) {
                // If there are no more cells left to collapse. Generation is complete!
                return true;
            }
            // 2. Collapse Cell
            if (!CollapseCell(lowestEntropyCell)) {
                return false; // Failed to generate
            }
            // 3. Propogate Constraints
            PropagateConstraints(lowestEntropyCell);
        }
    }

    (int, int) FindLowestEntropyCell() {
        int minEntropy = int.MaxValue;
        (int, int) lowestEntropyCell = (-1, -1);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (grid[x, y] == -1) {
                    int entropy = possibilitiesMap[(x, y)].Count;
                    if (entropy < minEntropy) {
                        minEntropy = entropy;
                        lowestEntropyCell = (x, y);
                    }
                }

            }
        }
        if (lowestEntropyCell.Item1 == -1) {
            // if no more uncollapsed cells return (-1, -1)
            return lowestEntropyCell;
        }
        return lowestEntropyCell;

    }

    bool CollapseCell((int, int) pos) {
        int x = pos.Item1;
        int y = pos.Item2;
        var possibilities = possibilitiesMap[(x, y)];
        if (possibilities.Count == 0) {
            // Could not collapse the cell and the generation is therefore invalid
            return false;
        }
        int collapsedTileIndex = possibilities[Random.Range(0, possibilities.Count)];

        grid[x, y] = collapsedTileIndex;
        possibilitiesMap[(x, y)].Clear();
        possibilitiesMap[(x, y)].Add(collapsedTileIndex);
        return true;
    }


    void PropagateConstraints((int, int) pos) {

        int x = pos.Item1;
        int y = pos.Item2;
        int collapsedTile = grid[x, y];
        string collapsedTileName = tiles[collapsedTile].name;
        // Propagate to all 8 surrounding neighbors
        for (int nx = x - 1; nx <= x + 1; nx++) {
            for (int ny = y - 1; ny <= y + 1; ny++) {
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && !(nx == x && ny == y)) {
                    PropagateToNeighbor(nx, ny, collapsedTileName);
                }
            }
        }


    }
    void PropagateToNeighbor(int x, int y, string collapsedTileName) {
        // for each neighbor in the possibilities map
        for (int index = possibilitiesMap[(x, y)].Count - 1; index >= 0; index--) {
            int neighborTileIndex = possibilitiesMap[(x, y)][index];
            string neighborTileName = tiles[neighborTileIndex].name;
            bool neighborIsAllowed = false;
            foreach (string allowedNeighbor in tiles[neighborTileIndex].allowedNeighbors) {
                if (allowedNeighbor == collapsedTileName) {
                    neighborIsAllowed = true;
                    break;
                }
            }
            if (!neighborIsAllowed) {
                possibilitiesMap[(x, y)].RemoveAt(index);
            }
        }
    }


    void CreateVisuals() {
        // Clear existing tiles
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                int tileIndex = grid[x, y];
                if (tileIndex > -1) {
                    GameObject tile = Instantiate(tiles[tileIndex].prefab, new Vector3(x, y, 0), Quaternion.identity);
                    tile.transform.SetParent(transform);

                }
            }
        }
    }
}