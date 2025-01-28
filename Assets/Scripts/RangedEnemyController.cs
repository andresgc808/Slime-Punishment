using System.Collections;
using UnityEngine;

public class RangedEnemyController : MonoBehaviour {
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _minDistanceToTarget = 3f;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private float _attackPower = 10f;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform target; // Assign the player's transform here

    private Rigidbody2D _rb;
    private Coroutine _attackCoroutine; // Store the coroutine reference
    private bool _isAttacking = false;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        if (target == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        _fireRate = 1f; //sets default attack speed to 1
    }

    private void Update() {
        if (target == null) return; // Don't do anything if the player doesn't exist

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > _minDistanceToTarget) {
            _rb.velocity = direction * _moveSpeed;
            StopAttackCoroutine(); // Stop attack coroutine when out of range
        } else {
            _rb.velocity = Vector2.zero;
            StartAttackCoroutine(); // Start attack coroutine if within range
        }
    }

    private void StartAttackCoroutine() {
        if (_attackCoroutine != null) return; // Prevent starting a new coroutine if one exists
        _attackCoroutine = StartCoroutine(AttackCoroutine());
        _isAttacking = true;
    }

    private void StopAttackCoroutine() {
        if (_attackCoroutine == null) return; // prevent stopping a non existent coroutine
        StopCoroutine(_attackCoroutine);
        _attackCoroutine = null;
        _isAttacking = false;
    }

    private IEnumerator AttackCoroutine() {
        while (_isAttacking && target != null) {
            Attack(); // Call attack
            yield return new WaitForSeconds(1f / _fireRate);

        }
        _attackCoroutine = null;
        _isAttacking = false;
    }


    private void Attack() {
        if (target == null) return;
        Vector2 direction = ((Vector2)target.position - (Vector2)_firePoint.position).normalized; // Use the target transform
        GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/BlueBullet"), _firePoint.position, Quaternion.identity); // Load BlueBullet
        IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
        if (projectileComponent != null) {
            projectileComponent.LaunchProjectile(_firePoint.position, direction);
        }
    }
}