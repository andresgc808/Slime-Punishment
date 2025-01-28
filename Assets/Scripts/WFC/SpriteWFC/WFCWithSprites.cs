using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using Random = UnityEngine.Random;

public class WFCWithSprites : MonoBehaviour {
    public WFCDataOld wfcDataOld;
    public Transform spriteParent;
    public Vector3 offset;
    public bool autoRun = false;
    public int seed = -1; // -1 means no specific seed.

    private int _totalNodesExpanded;
    private int _totalRetries;
    private Stack<(WFCGridCellSprite, List<TileSprite>)> _backtrackStack = new Stack<(WFCGridCellSprite, List<TileSprite>)>();
    private Stack<WFCGridCellSprite[,]> _backtrackGridStateStack = new Stack<WFCGridCellSprite[,]>();

    private class WFCTestResult {
        public int Seed { get; set; }
        public bool Success { get; set; }
        public int NodesExpanded { get; set; }
        public int Retries { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public override string ToString() {
            return $"Seed: {Seed} Success: {Success}, Nodes: {NodesExpanded}, Retries: {Retries}, Time: {ElapsedMilliseconds} ms";
        }
    }

    [ContextMenu("Generate with Arc Consistency")]
    public void GenerateWithArcConsistency() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCArcConsistency();
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            VisualizeMap();
            UnityEngine.Debug.Log($"Map Generation Complete (Arc Consistency). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (Arc Consistency). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Generate")]
    public void Generate() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(false, false); // No cache, no forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            VisualizeMap();
            UnityEngine.Debug.Log($"Map Generation Complete (No Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (No Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Generate With Cache")]
    public void GenerateWithCache() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(true, false); // With cache, no forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            VisualizeMap();
            UnityEngine.Debug.Log($"Map Generation Complete (Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Generate With Forward Checking")]
    public void GenerateWithForwardChecking() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(false, true); // No cache, With forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            VisualizeMap();
            UnityEngine.Debug.Log($"Map Generation Complete (No Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (No Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Generate With Cache and Forward Checking")]
    public void GenerateWithCacheAndForwardChecking() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(true, true); // With cache and forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            VisualizeMap();
            UnityEngine.Debug.Log($"Map Generation Complete (Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }


    [ContextMenu("Test Performance")]
    public void TestPerformance() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(false, false); // No cache, no forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            UnityEngine.Debug.Log($"Map Generation Complete (No Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (No Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Test Performance With Cache")]
    public void TestPerformanceWithCache() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(true, false); // With cache, no forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            UnityEngine.Debug.Log($"Map Generation Complete (Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (Cache, No FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Test Performance With Forward Checking")]
    public void TestPerformanceWithForwardChecking() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(false, true); // No cache, With forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            UnityEngine.Debug.Log($"Map Generation Complete (No Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (No Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }

    [ContextMenu("Test Performance With Cache and Forward Checking")]
    public void TestPerformanceWithCacheAndForwardChecking() {
        ClearSprites();
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool generationSuccess = WFCBasic(true, true); // With cache and forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (generationSuccess) {
            UnityEngine.Debug.Log($"Map Generation Complete (Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        } else {
            UnityEngine.Debug.LogError($"Map Generation Failed (Cache, FC). Nodes Expanded: {_totalNodesExpanded}. Retries: {_totalRetries}. Time: {elapsedMilliseconds}ms");
        }
        _totalNodesExpanded = 0;
        _totalRetries = 0;
    }


    [ContextMenu("Test Performance All Seeds")]
    public void TestPerformanceAllSeeds() {
        UnityEngine.Debug.Log("Starting performance tests for all WFC variants");
        ClearSprites();
        int numberOfSeeds = 5;

        List<WFCTestResult> noCacheNoFCResults = new List<WFCTestResult>();
        List<WFCTestResult> cacheNoFCResults = new List<WFCTestResult>();
        List<WFCTestResult> noCacheFCResults = new List<WFCTestResult>();
        List<WFCTestResult> cacheFCResults = new List<WFCTestResult>();

        for (int i = 0; i < numberOfSeeds; i++) {
            int currentSeed = i + 1;
            UnityEngine.Debug.Log($"\nSeed: {currentSeed}");
            seed = currentSeed;

            WFCTestResult noCacheNoFCResult = RunTest(false, false, currentSeed);
            noCacheNoFCResults.Add(noCacheNoFCResult);

            WFCTestResult cacheNoFCResult = RunTest(true, false, currentSeed);
            cacheNoFCResults.Add(cacheNoFCResult);

            WFCTestResult noCacheFCResult = RunTest(false, true, currentSeed);
            noCacheFCResults.Add(noCacheFCResult);

            WFCTestResult cacheFCResult = RunTest(true, true, currentSeed);
            cacheFCResults.Add(cacheFCResult);
        }
        ReportResults("No Cache, No FC", noCacheNoFCResults);
        ReportResults("Cache, No FC", cacheNoFCResults);
        ReportResults("No Cache, FC", noCacheFCResults);
        ReportResults("Cache, FC", cacheFCResults);
    }

    private WFCTestResult RunTest(bool useCache, bool useForwardChecking, int currentSeed) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool success = WFCBasic(useCache, useForwardChecking); // No cache, no forward checking
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        WFCTestResult testResult = new WFCTestResult {
            Seed = currentSeed,
            Success = success,
            NodesExpanded = _totalNodesExpanded,
            Retries = _totalRetries,
            ElapsedMilliseconds = elapsedMilliseconds
        };
        _totalNodesExpanded = 0;
        _totalRetries = 0;

        return testResult;
    }

    private void ReportResults(string wfcType, List<WFCTestResult> testResults) {
        UnityEngine.Debug.Log($"\n--- Results for: {wfcType} ---");
        int totalSuccesses = testResults.Count(r => r.Success);
        float successPercentage = (float)totalSuccesses / testResults.Count * 100f;
        long totalNodesExpanded = 0;
        long totalRetries = 0;
        long totalElapsedMilliseconds = 0;

        foreach (var result in testResults) {
            totalNodesExpanded += result.NodesExpanded;
            totalRetries += result.Retries;
            totalElapsedMilliseconds += result.ElapsedMilliseconds;
        }


        float averageNodesExpanded = (float)totalNodesExpanded / testResults.Count;
        float averageRetries = (float)totalRetries / testResults.Count;
        float averageElapsedMilliseconds = (float)totalElapsedMilliseconds / testResults.Count;


        UnityEngine.Debug.Log($"Success Rate: {successPercentage:F2}%, Average Nodes Expanded: {averageNodesExpanded:F2}, Average Retries: {averageRetries:F2}, Average Time: {averageElapsedMilliseconds:F2} ms");
    }

    private void ClearSprites() {
        if (spriteParent == null) return;
        // Use a for loop for safety while removing
        for (int i = spriteParent.childCount - 1; i >= 0; i--) {
            Transform child = spriteParent.GetChild(i);
            if (child != null) {
                DestroyImmediate(child.gameObject); // Destroy immediately for editor
            }
        }
    }


    private bool WFCBasic(bool useCache, bool useForwardChecking) {
        UnityEngine.Debug.Log($"WFCBasic: Starting WFC process, Caching = {useCache}, Forward Checking = {useForwardChecking}");
        if (wfcDataOld.wfcObject.tiles.Count == 0) {
            UnityEngine.Debug.LogError("WFCBasic: No tiles in wfcDataOld!");
            return false;
        }
        if (wfcDataOld.wfcObject.gridSize.x == 0 || wfcDataOld.wfcObject.gridSize.y == 0) {
            UnityEngine.Debug.LogError("WFCBasic: WFC Grid size cannot be zero");
            return false;
        }

        int maxRetries = 500;
        bool finished = false;

        if (seed != -1) {
            Random.InitState(seed);
        }

        for (int currentRetry = 0; currentRetry < maxRetries; currentRetry++) {
            _totalRetries++;
            _backtrackStack.Clear(); // Clear the backtrack stack at the beginning of each retry
            _backtrackGridStateStack.Clear(); // Clear the grid state stack at the beginning of each retry

            // Explicitly initialize the grid
            wfcDataOld.wfcObject.InitializeGrid(wfcDataOld.wfcObject.tiles);

            PrePopulateEdgesWithWater(useCache, useForwardChecking);

            while (!finished) {
                if (Collapse(out WFCGridCellSprite cell, useForwardChecking)) {
                    _totalNodesExpanded++;
                    Propagate(cell, useCache, useForwardChecking);

                    if (CheckIfDone()) {
                        finished = true;
                    } else if (HasContradiction()) {
                        // Backtrack if contradiction is found
                        if (!Backtrack()) {
                            break; // If backtracking fails, break to the outer loop for retry
                        }
                    }
                } else {
                    break; // If cannot collapse, break out of inner loop, and retry
                }
            }
            if (finished) {
                UnityEngine.Debug.Log("WFCBasic: WFC process finished");
                UnityEngine.Debug.Log(wfcDataOld.wfcObject.ToString());
                return true; // Exit the method if generation is successful
            }
        }


        UnityEngine.Debug.LogError("WFCBasic: WFC process failed after multiple retries");
        UnityEngine.Debug.Log(wfcDataOld.wfcObject.ToString());
        return false; // Return false if max retries are exhausted
    }


    private bool HasContradiction() {
        for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcDataOld.wfcObject.gridSize.y; y++) {
                if (!wfcDataOld.wfcObject.grid[x, y].collapsed && wfcDataOld.wfcObject.grid[x, y].possibleTiles.Count == 0) {
                    return true;
                }

            }
        }

        return false;
    }

    private void PrePopulateEdgesWithWater(bool useCache, bool useForwardChecking) {
        // UnityEngine.Debug.Log("PrePopulateEdgesWithWater: Starting pre-population of edges with water");
        // Find the water tile
        TileSprite waterTile = wfcDataOld.wfcObject.tiles.Find(tile => tile.tileType == TileType.Water);
        if (waterTile.Equals(default(Tile))) {
            UnityEngine.Debug.LogError("PrePopulateEdgesWithWater: No Water tile found!");
            return;
        }

        // Top row
        for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
            SetCellToTile(x, wfcDataOld.wfcObject.gridSize.y - 1, waterTile);
            Propagate(wfcDataOld.wfcObject.grid[x, wfcDataOld.wfcObject.gridSize.y - 1], useCache, useForwardChecking); // we don't want forward checking at the edges
        }

        // Bottom row
        for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
            SetCellToTile(x, 0, waterTile);
            Propagate(wfcDataOld.wfcObject.grid[x, 0], useCache, useForwardChecking);
        }

        // Left Column
        for (int y = 1; y < wfcDataOld.wfcObject.gridSize.y - 1; y++) {
            SetCellToTile(0, y, waterTile);
            Propagate(wfcDataOld.wfcObject.grid[0, y], useCache, useForwardChecking);
        }

        // Right Column
        for (int y = 1; y < wfcDataOld.wfcObject.gridSize.y - 1; y++) {
            SetCellToTile(wfcDataOld.wfcObject.gridSize.x - 1, y, waterTile);
            Propagate(wfcDataOld.wfcObject.grid[wfcDataOld.wfcObject.gridSize.x - 1, y], useCache, useForwardChecking);
        }

        // UnityEngine.Debug.Log("PrePopulateEdgesWithWater: Finished pre-population of edges with water");
    }

    private void SetCellToTile(int x, int y, TileSprite tile) {
        if (x < 0 || x >= wfcDataOld.wfcObject.gridSize.x || y < 0 || y >= wfcDataOld.wfcObject.gridSize.y) {
            UnityEngine.Debug.LogError($"SetCellToTile: Invalid cell coordinates x:{x}, y:{y}");
            return;
        }
        WFCGridCellSprite cell = wfcDataOld.wfcObject.grid[x, y];
        cell.possibleTiles = new List<TileSprite>() { tile };
        cell.collapsed = true;
        wfcDataOld.wfcObject.grid[x, y] = cell;
        UnityEngine.Debug.Log($"SetCellToTile: Set cell at x: {x}, y: {y} to tile {tile.tileType}");
    }

    private bool CheckIfDone() {
        // UnityEngine.Debug.Log("CheckIfDone: Checking if map is fully collapsed");
        for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcDataOld.wfcObject.gridSize.y; y++) {
                if (!wfcDataOld.wfcObject.grid[x, y].collapsed) {
                    //  UnityEngine.Debug.Log($"CheckIfDone: Cell not collapsed at x:{x} y:{y}");
                    return false;
                }
            }
        }

        UnityEngine.Debug.Log("CheckIfDone: Map is fully collapsed");
        return true;
    }

    private bool Collapse(out WFCGridCellSprite lowestEntropyCell, bool useForwardChecking) {
        lowestEntropyCell = new WFCGridCellSprite();
        List<WFCGridCellSprite> possibleCells = new List<WFCGridCellSprite>();

        for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcDataOld.wfcObject.gridSize.y; y++) {
                WFCGridCellSprite cell = wfcDataOld.wfcObject.grid[x, y];
                if (!cell.collapsed) {
                    possibleCells.Add(cell);
                }
            }
        }

        if (possibleCells.Count == 0) {
            return false;
        }

        if (useForwardChecking) {
            possibleCells = possibleCells.OrderBy(cell => cell.possibleTiles.Count).ToList();
        } else {
            possibleCells = possibleCells.OrderBy(cell => cell.possibleTiles.Count).ToList();
        }

        lowestEntropyCell = possibleCells[0];


        if (lowestEntropyCell.possibleTiles == null) {
            UnityEngine.Debug.LogError("Collapse: possibleTiles is null!");
            return false;
        }

        if (lowestEntropyCell.possibleTiles.Count == 0) {
            return false;
        }

        TileSprite chosenTile;
        if (lowestEntropyCell.possibleTiles.Count > 1) {
            int index = Random.Range(0, lowestEntropyCell.possibleTiles.Count);
            chosenTile = lowestEntropyCell.possibleTiles[index];
            _backtrackStack.Push((lowestEntropyCell, new List<TileSprite>(lowestEntropyCell.possibleTiles)));
            _backtrackGridStateStack.Push(CopyGrid(wfcDataOld.wfcObject.grid)); //Save our grid state for this decision.
            lowestEntropyCell.possibleTiles = new List<TileSprite>() { chosenTile };

        } else {
            chosenTile = lowestEntropyCell.possibleTiles[0];
            _backtrackStack.Push((lowestEntropyCell, new List<TileSprite>(lowestEntropyCell.possibleTiles)));
            _backtrackGridStateStack.Push(CopyGrid(wfcDataOld.wfcObject.grid)); //Save our grid state for this decision.
            lowestEntropyCell.possibleTiles = new List<TileSprite>() { chosenTile };
        }



        lowestEntropyCell.collapsed = true;
        wfcDataOld.wfcObject.grid[lowestEntropyCell.x, lowestEntropyCell.y] = lowestEntropyCell;
        return true;
    }



    private bool Backtrack() {
        if (_backtrackStack.Count == 0) {
            //If no more previous decisions, cannot backtrack.
            return false;
        }

        (WFCGridCellSprite prevCell, List<TileSprite> prevPossibleTiles) = _backtrackStack.Pop();
        wfcDataOld.wfcObject.grid = _backtrackGridStateStack.Pop(); //Reset Grid to previous state.

        //Reset previous cell state
        prevCell = wfcDataOld.wfcObject.grid[prevCell.x, prevCell.y];
        prevCell.collapsed = false;
        prevCell.possibleTiles = prevPossibleTiles;
        wfcDataOld.wfcObject.grid[prevCell.x, prevCell.y] = prevCell;

        // If there is more than one possibility in the cell.
        if (prevPossibleTiles.Count > 1) {
            // Remove the last tile that was tried from possible tiles, and add it back into the previous stack.
            TileSprite lastTriedTile = prevCell.possibleTiles[0];
            prevCell.possibleTiles.Remove(lastTriedTile);

            _backtrackStack.Push((prevCell, new List<TileSprite>(prevCell.possibleTiles)));
            _backtrackGridStateStack.Push(CopyGrid(wfcDataOld.wfcObject.grid)); //Save our grid state for this decision.

            // Try a different one next time.
            return true;
        }
        //If no more tiles in this cell, backtrack further
        return Backtrack();
    }

    private WFCGridCellSprite[,] CopyGrid(WFCGridCellSprite[,] sourceGrid) {
        int width = sourceGrid.GetLength(0);
        int height = sourceGrid.GetLength(1);
        WFCGridCellSprite[,] newGrid = new WFCGridCellSprite[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                newGrid[x, y] = new WFCGridCellSprite(sourceGrid[x, y].x, sourceGrid[x, y].y) {
                    collapsed = sourceGrid[x, y].collapsed,
                    possibleTiles = new List<TileSprite>(sourceGrid[x, y].possibleTiles)

                };
            }
        }
        return newGrid;
    }


    private void Propagate(WFCGridCellSprite collapsedCell, bool useCache, bool useForwardChecking) {
        // UnityEngine.Debug.Log($"Propagate: Starting propagation for cell at x: {collapsedCell.x} y: {collapsedCell.y}");
        int x = collapsedCell.x;
        int y = collapsedCell.y;
        TileSprite collapsedTile = collapsedCell.possibleTiles[0];
        // UnityEngine.Debug.Log($"Propagate: Collapsed tile is {collapsedTile.tileType}");

        PropagateToCell(x, y + 1, collapsedTile, Direction.Up, useCache, useForwardChecking);
        PropagateToCell(x, y - 1, collapsedTile, Direction.Down, useCache, useForwardChecking);
        PropagateToCell(x - 1, y, collapsedTile, Direction.Left, useCache, useForwardChecking);
        PropagateToCell(x + 1, y, collapsedTile, Direction.Right, useCache, useForwardChecking);
        // UnityEngine.Debug.Log("Propagate: Finished propagation");
    }
    private Dictionary<(Direction, TileType), List<TileSprite>> _propagationCache = new Dictionary<(Direction, TileType), List<TileSprite>>();

    private void PropagateToCell(int x, int y, TileSprite collapsedTile, Direction direction, bool useCache, bool useForwardChecking) {
        // UnityEngine.Debug.Log($"PropagateToCell: Starting propagate to x:{x} y:{y} from tile: {collapsedTile.tileType}, direction: {direction}");
        if (x < 0 || x >= wfcDataOld.wfcObject.gridSize.x) {
            // UnityEngine.Debug.Log($"PropagateToCell: Invalid x:{x}, skipping");
            return;
        }

        if (y < 0 || y >= wfcDataOld.wfcObject.gridSize.y) {
            // UnityEngine.Debug.Log($"PropagateToCell: Invalid y:{y}, skipping");
            return;
        }

        WFCGridCellSprite cell = wfcDataOld.wfcObject.grid[x, y];

        if (cell.collapsed) {
            // UnityEngine.Debug.Log($"PropagateToCell: Cell x:{x} y:{y} already collapsed, skipping");
            return;
        }
        //  UnityEngine.Debug.Log($"PropagateToCell: Before filtering possible tiles: {string.Join(", ", cell.possibleTiles.Select(tile => tile.tileType))} for cell at x:{x}, y:{y}");

        HashSet<TileType> validConnections = new HashSet<TileType>();
        if (useCache) {
            var cacheKey = (direction, collapsedTile.tileType);

            if (_propagationCache.TryGetValue(cacheKey, out List<TileSprite> cachedTiles)) {
                validConnections.UnionWith(cachedTiles.Select(tile => tile.tileType));
                //UnityEngine.Debug.Log($"PropagateToCell: Using cached valid connections for cell at x:{x}, y:{y}, tiles: {string.Join(", ", validConnections)}");
            } else {

                switch (direction) {
                    case Direction.Up:
                        if (collapsedTile.upConnections != null) {
                            validConnections.UnionWith(collapsedTile.upConnections);
                        }
                        break;
                    case Direction.Down:
                        if (collapsedTile.downConnections != null) {
                            validConnections.UnionWith(collapsedTile.downConnections);
                        }
                        break;
                    case Direction.Left:
                        if (collapsedTile.leftConnections != null) {
                            validConnections.UnionWith(collapsedTile.leftConnections);
                        }
                        break;
                    case Direction.Right:
                        if (collapsedTile.rightConnections != null) {
                            validConnections.UnionWith(collapsedTile.rightConnections);
                        }
                        break;
                }
                _propagationCache[cacheKey] = validConnections.Select(connection => wfcDataOld.wfcObject.tiles.Find(t => t.tileType == connection)).ToList();
                //UnityEngine.Debug.Log($"PropagateToCell: Adding valid connections to cache x:{x}, y:{y}, tiles {string.Join(", ", validConnections)}");
            }

        } else {
            switch (direction) {
                case Direction.Up:
                    if (collapsedTile.upConnections != null) {
                        validConnections.UnionWith(collapsedTile.upConnections);
                    }
                    break;
                case Direction.Down:
                    if (collapsedTile.downConnections != null) {
                        validConnections.UnionWith(collapsedTile.downConnections);
                    }
                    break;
                case Direction.Left:
                    if (collapsedTile.leftConnections != null) {
                        validConnections.UnionWith(collapsedTile.leftConnections);
                    }
                    break;
                case Direction.Right:
                    if (collapsedTile.rightConnections != null) {
                        validConnections.UnionWith(collapsedTile.rightConnections);
                    }
                    break;
            }
        }

        cell.possibleTiles.RemoveAll(tile => !validConnections.Contains(tile.tileType));
        wfcDataOld.wfcObject.grid[x, y] = cell;
    }

    private void VisualizeMap() {
        // UnityEngine.Debug.Log("VisualizeMap: Starting map visualization");
        for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcDataOld.wfcObject.gridSize.y; y++) {
                var cell = wfcDataOld.wfcObject.grid[x, y];
                if (cell.possibleTiles == null || cell.possibleTiles.Count == 0) {
                    UnityEngine.Debug.LogError($"VisualizeMap: No valid tiles in cell at x:{x} y:{y}, skipping");
                    continue; // Skip if no tile could be placed
                }

                var tile = cell.possibleTiles[0];
                if (tile.possibleSprites != null && tile.possibleSprites.Count > 0) {
                    int randomIndex = Random.Range(0, tile.possibleSprites.Count);
                    Sprite chosenSprite = tile.possibleSprites[randomIndex];

                    // UnityEngine.Debug.Log($"VisualizeMap: instantiating sprite: {chosenSprite}, at x: {x}, y: {y}");
                    GameObject spriteObject = new GameObject(tile.tileType.ToString());
                    SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = chosenSprite;

                    // Get the bounds of the sprite
                    var bounds = renderer.sprite.bounds;
                    // Calculate the offset to center the sprite
                    var offset = new Vector3(bounds.center.x, bounds.center.y, 0);

                    spriteObject.transform.position = new Vector3(x, y, 0) + offset + this.offset;
                    spriteObject.transform.SetParent(spriteParent, true);
                } else {
                    UnityEngine.Debug.LogError($"VisualizeMap: No valid sprites in cell at x:{x} y:{y}, skipping");
                }
            }
        }
        // UnityEngine.Debug.Log("VisualizeMap: Finished map visualization");
    }
    private bool WFCArcConsistency() {
        UnityEngine.Debug.Log($"WFCArcConsistency: Starting WFC process with Arc Consistency");
        if (wfcDataOld.wfcObject.tiles.Count == 0) {
            UnityEngine.Debug.LogError("WFCArcConsistency: No tiles in wfcDataOld!");
            return false;
        }
        if (wfcDataOld.wfcObject.gridSize.x == 0 || wfcDataOld.wfcObject.gridSize.y == 0) {
            UnityEngine.Debug.LogError("WFCArcConsistency: WFC Grid size cannot be zero");
            return false;
        }

        int maxRetries = 500;
        bool finished = false;
        if (seed != -1) {
            Random.InitState(seed);
        }

        for (int currentRetry = 0; currentRetry < maxRetries; currentRetry++) {
            _totalRetries++;
            _backtrackStack.Clear();
            _backtrackGridStateStack.Clear(); // Clear the grid state stack at the beginning of each retry
            wfcDataOld.wfcObject.InitializeGrid(wfcDataOld.wfcObject.tiles);

            PrePopulateEdgesWithWater(false, false);


            if (!EstablishArcConsistency()) {
                if (!Backtrack()) {
                    continue;
                } else {
                    continue;
                }
            }


            while (!finished) {
                if (Collapse(out WFCGridCellSprite cell, false)) {
                    _totalNodesExpanded++;

                    if (!EstablishArcConsistency()) {
                        // Backtrack if contradiction is found
                        if (!Backtrack()) {
                            break; // If backtracking fails, break to the outer loop for retry
                        } else {
                            continue;
                        }
                    }

                    if (CheckIfDone()) {
                        finished = true;
                    } else if (HasContradiction()) {
                        if (!Backtrack()) {
                            break; // If backtracking fails, break to the outer loop for retry
                        } else {
                            continue;
                        }
                    }

                } else {
                    break;
                }
            }
            if (finished) {
                UnityEngine.Debug.Log("WFCArcConsistency: WFC process finished");
                UnityEngine.Debug.Log(wfcDataOld.wfcObject.ToString());
                return true;
            }

        }
        UnityEngine.Debug.LogError("WFCArcConsistency: WFC process failed after multiple retries");
        UnityEngine.Debug.Log(wfcDataOld.wfcObject.ToString());
        return false;

    }
    private bool EstablishArcConsistency() {
        bool changed = true;
        while (changed) {
            changed = false;
            for (int x = 0; x < wfcDataOld.wfcObject.gridSize.x; x++) {
                for (int y = 0; y < wfcDataOld.wfcObject.gridSize.y; y++) {
                    WFCGridCellSprite currentCell = wfcDataOld.wfcObject.grid[x, y];
                    if (currentCell.collapsed) continue; // Skip collapsed cells

                    // Check and reduce domains of neighbors
                    if (ReduceDomain(x, y + 1, currentCell, Direction.Up)) changed = true;
                    if (ReduceDomain(x, y - 1, currentCell, Direction.Down)) changed = true;
                    if (ReduceDomain(x - 1, y, currentCell, Direction.Left)) changed = true;
                    if (ReduceDomain(x + 1, y, currentCell, Direction.Right)) changed = true;
                }
            }
            if (HasContradiction()) return false;
        }
        return true; // returns true when no more constraints can be removed.
    }

    private bool ReduceDomain(int neighborX, int neighborY, WFCGridCellSprite currentCell, Direction direction) {
        if (neighborX < 0 || neighborX >= wfcDataOld.wfcObject.gridSize.x ||
            neighborY < 0 || neighborY >= wfcDataOld.wfcObject.gridSize.y) {
            return false; // Neighbor is out of bounds
        }

        WFCGridCellSprite neighborCell = wfcDataOld.wfcObject.grid[neighborX, neighborY];
        if (neighborCell.collapsed) return false;


        List<TileSprite> validTilesForNeighbor = new List<TileSprite>();

        foreach (var neighborTile in neighborCell.possibleTiles) {
            bool isValid = false;
            foreach (var currentTile in currentCell.possibleTiles) {
                if (HasValidConnection(currentTile, neighborTile, direction)) {
                    isValid = true;
                    break;
                }
            }
            if (isValid) {
                validTilesForNeighbor.Add(neighborTile);
            }

        }

        var originalCount = neighborCell.possibleTiles.Count;
        neighborCell.possibleTiles = validTilesForNeighbor;
        wfcDataOld.wfcObject.grid[neighborX, neighborY] = neighborCell;

        return neighborCell.possibleTiles.Count < originalCount;
    }

    private bool HasValidConnection(TileSprite currentTile, TileSprite neighborTile, Direction direction) {
        switch (direction) {
            case Direction.Up:
                if (currentTile.upConnections != null && currentTile.upConnections.Contains(neighborTile.tileType)
                && neighborTile.downConnections != null && neighborTile.downConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
            case Direction.Down:
                if (currentTile.downConnections != null && currentTile.downConnections.Contains(neighborTile.tileType)
                    && neighborTile.upConnections != null && neighborTile.upConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
            case Direction.Left:
                if (currentTile.leftConnections != null && currentTile.leftConnections.Contains(neighborTile.tileType)
                  && neighborTile.rightConnections != null && neighborTile.rightConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
            case Direction.Right:
                if (currentTile.rightConnections != null && currentTile.rightConnections.Contains(neighborTile.tileType)
                   && neighborTile.leftConnections != null && neighborTile.leftConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
        }
        return false;
    }
}
