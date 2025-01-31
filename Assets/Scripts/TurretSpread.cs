using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSpread : MonoBehaviour {
    [SerializeField] private Transform target;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private float agroRange = 5f;

    private Coroutine _attackCoroutine; // Store the coroutine reference
    private bool _isAttacking = false;

    [SerializeField] private float _fireRate = 1f;

    [SerializeField] public float bulletCount = 5;

    [SerializeField] public float spreadAngle = 15;

    [SerializeField] public float numberOfBarrages = 3;

    private Animator _animator;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        if (target == null) // sets target to player if not set
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // grab animator
        _animator = GetComponent<Animator>();
    }
    private void Update() {
        if (target == null) return; // don't do anything if the player doesn't exist
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;

        UpdateSpriteFlip(direction);

        if (Vector2.Distance(transform.position, target.position) < agroRange) {
            StartAttackCoroutine(); // Start attack coroutine if within range
        } else {
            StopAttackCoroutine(); // Stop attack coroutine when out of range
        }
    }

    private void UpdateSpriteFlip(Vector2 direction) {

        if (direction.x < 0) {
            _spriteRenderer.flipX = false;

        } else if (direction.x > 0) {
            _spriteRenderer.flipX = true;
        }
    }

    private void StartAttackCoroutine() {
        if (_attackCoroutine != null) return; // Prevent starting a new coroutine if one exists
        _attackCoroutine = StartCoroutine(AttackCoroutine());
        _isAttacking = true;

        // Set the inAttackRange bool to true to begin the shooting animation
        if (_animator != null) {
            _animator.SetBool("inAttackRange", true);
        }
    }

    private void StopAttackCoroutine() {
        if (_attackCoroutine == null) return; // prevent stopping a non existent coroutine
        StopCoroutine(_attackCoroutine);
        _attackCoroutine = null;
        _isAttacking = false;

        // Set the inAttackRange bool to false to stop the shooting animation
        if (_animator != null) {
            _animator.SetBool("inAttackRange", false);
        }
    }

    private IEnumerator AttackCoroutine() {
        while (_isAttacking && target != null) {
            yield return StartCoroutine(Attack()); // Call attack coroutine
            yield return new WaitForSeconds(1f / _fireRate);
        }
        _attackCoroutine = null;
        _isAttacking = false;
    }

    private IEnumerator Attack() {
        if (target == null) yield break;
        

        // iterate through the number of barrages
        for (int i = 0; i < numberOfBarrages; i++) {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized; // Use the target transform
            // iterate through the number of bullets
            for (int j = 0; j < bulletCount; j++) {
                // calculate the angle of the bullet
                float angle = (j % 2 == 0 ? 1 : -1) * (spreadAngle) * (j / 2);
                Vector2 bulletDirection = Quaternion.Euler(0, 0, angle) * direction;

                // create the bullet
                GameObject bullet = Instantiate(Resources.Load<GameObject>("Prefabs/PinkBullet"), transform.position, Quaternion.identity);
                IProjectile projectileComponent = bullet.GetComponent<IProjectile>();
                if (projectileComponent != null) {
                    projectileComponent.LaunchProjectile(transform.position, bulletDirection);
                }
            }
            yield return new WaitForSeconds(0.25f); // Wait 0.25 seconds between barrages
        }
    }
}
