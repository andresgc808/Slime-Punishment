using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour {
    public float moveSpeed = 5f;
    [SerializeField] public Transform playerTransform;
    private List<Vector2Int> _currentPath;
    [SerializeField] private MapManager _mapManager;
    private int _pathIndex;
    private bool _isMoving = false;

    private void Start() {

        if (_mapManager == null) {
            Debug.LogError("EnemyMovement: Map manager not found on this object, attempting to find from scene.");
            _mapManager = FindObjectOfType<MapManager>();
        }

        if (_mapManager == null) {
            Debug.LogError("EnemyMovement: Map manager could not be found.");
            return;
        }

        if (playerTransform == null) {
            Debug.LogError("EnemyMovement: Player transform not set");
            // get reference
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (playerTransform == null) {
            Debug.LogError("EnemyMovement: Player transform could not be found");
            return;
        }

        if (_mapManager.NavigationGridGenerated) {
            _mapManager.PrintNavigationGrid();
        } else {
            Debug.LogWarning("EnemyMovement: Navigation grid is null, or not yet generated.");
        }
    }

    private void Update() {
        if (_mapManager == null || playerTransform == null) return;
        if (Vector3.Distance(transform.position, playerTransform.position) > 100f) return;

        if (!_isMoving) {
            CalculatePath();
        }
        MoveAlongPath();
    }

    private void CalculatePath() {
        if (playerTransform == null) return;
        Vector2Int startGrid = _mapManager.WorldToGridPosition(transform.position);
        Vector2Int targetGrid = _mapManager.WorldToGridPosition(playerTransform.position);

        if (_mapManager.navigationGrid == null) {
            Debug.LogWarning("CalculatePath: Navigation grid is null");
            return;
        }

        if (!_mapManager.navigationGrid[startGrid.x, startGrid.y].IsWalkable || !_mapManager.navigationGrid[targetGrid.x, targetGrid.y].IsWalkable) {
            Debug.LogWarning($"CalculatePath: Start or Target position is not walkable, start: {startGrid}, target: {targetGrid}");
            return;
        }

        _currentPath = AStar.FindPath(_mapManager.navigationGrid, startGrid, targetGrid);

        if (_currentPath != null) {
            _pathIndex = 0;
            _isMoving = true;
        } else {
            _isMoving = false;
            Debug.LogWarning("CalculatePath: No path found.");
        }
    }


    private void MoveAlongPath() {
        if (_currentPath == null || _currentPath.Count == 0) {
            _isMoving = false;
            return;
        }
        if (_pathIndex >= _currentPath.Count) {
            _isMoving = false;
            return;
        }

        Vector3 targetPosition = _mapManager.GridToWorldPosition(_currentPath[_pathIndex].x, _currentPath[_pathIndex].y);
        targetPosition.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f) {
            _pathIndex++;
        }
    }
}