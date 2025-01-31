using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour {
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float aggroRange = 10f;
    [SerializeField] public Transform playerTransform;
    private List<Vector2Int> _currentPath;
    [SerializeField] private MapManager _mapManager;
    private int _pathIndex;
    private bool _isMoving = false;
    private Vector3 _currentVelocity;
    private float _smoothTime = 0.1f;

    private float timeSinceLastAttack = 0f;

    private Animator animator;

    private void Awake() {
        // get animator
        animator = GetComponent<Animator>();
    }


    private void Start() {
        if (_mapManager == null) {
            Debug.LogWarning("EnemyMovement: Map manager not found on this object, attempting to find from scene.");
            _mapManager = FindObjectOfType<MapManager>();
        }

        if (_mapManager == null) {
            Debug.LogWarning("EnemyMovement: Map manager could not be found.");
            return;
        }

        if (playerTransform == null) {
            Debug.LogWarning("EnemyMovement: Player transform not set");
            // get reference
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (playerTransform == null) {
            Debug.LogWarning("EnemyMovement: Player transform could not be found");
            return;
        }

        if (_mapManager.NavigationGridGenerated) {
            _mapManager.PrintNavigationGrid();
        } else {
            Debug.LogWarning("EnemyMovement: Navigation grid is null, or not yet generated.");
            return;
        }
    }

    private void Update() {
        if (_mapManager == null || playerTransform == null) return;


        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= aggroRange) {
            if (!_isMoving) {
                CalculatePath();
            }

            MoveAlongPath();
        } else {
            _isMoving = false; // Stop moving if player is out of aggro range
        }

        timeSinceLastAttack += Time.deltaTime;

        if (distanceToPlayer <= 0.5f && timeSinceLastAttack > 2f) {
            // Attack player
            Debug.Log("Attacking player");
            animator.SetBool("inAttackRange", true);
            // directly deal damage to player
            playerTransform.GetComponent<PlayerHealth>().TakeDamage(15);
            timeSinceLastAttack = 0f;
        } else if (timeSinceLastAttack > 1f && timeSinceLastAttack < 2f) {
            animator.SetBool("inAttackRange", false);
        }
    }


    private void CalculatePath() {
        if (playerTransform == null) return;
        Vector2Int startGrid = _mapManager.WorldToGridPosition(transform.position);
        Vector2Int targetGrid = _mapManager.WorldToGridPosition(playerTransform.position);


        if (_mapManager.navigationGrid == null) {
            Debug.LogWarning("CalculatePath: Navigation grid is null");
            return;
        }
        if (startGrid.x < 0 || startGrid.x >= _mapManager.navigationGrid.GetLength(0) || startGrid.y < 0 || startGrid.y >= _mapManager.navigationGrid.GetLength(1) ||
           targetGrid.x < 0 || targetGrid.x >= _mapManager.navigationGrid.GetLength(0) || targetGrid.y < 0 || targetGrid.y >= _mapManager.navigationGrid.GetLength(1)) {
            Debug.LogWarning($"CalculatePath: Start or Target position is out of bounds, start: {startGrid}, target: {targetGrid}");
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

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime, moveSpeed);


        if (Vector3.Distance(transform.position, targetPosition) < 0.01f) {
            _pathIndex++;
        }
    }
}