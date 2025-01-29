using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private float agroRange = 5f;

    private Coroutine _attackCoroutine; // Store the coroutine reference
    private bool _isAttacking = false;

    [SerializeField] private float _fireRate = 1f;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        if(target == null) // sets target to player if not set
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if(player!=null)
              target = player.transform;
        }
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if(target == null) return; // don't do anything if the player doesn't exist
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;

        UpdateSpriteFlip(direction);

        if (Vector2.Distance(transform.position, target.position) < agroRange)
        {
            StartAttackCoroutine(); // Start attack coroutine if within range
        }
        else
        {
            StopAttackCoroutine(); // Stop attack coroutine when out of range
        }
    }

    private void UpdateSpriteFlip(Vector2 direction)
    {

        if (direction.x < 0)
        {
            _spriteRenderer.flipX = false;

        }
        else if (direction.x > 0)
        {
            _spriteRenderer.flipX = true;
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
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized; // Use the target transform
        GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/BlueBullet"), transform.position, Quaternion.identity); // Load BlueBullet
        IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
        if (projectileComponent != null) {
            projectileComponent.LaunchProjectile(transform.position, direction);
        }
    }

}