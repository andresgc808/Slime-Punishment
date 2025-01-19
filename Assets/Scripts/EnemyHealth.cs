using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable {
    public float MaxHealth = 100f;
    public float Health { get; set; }
    public bool IsAlive { get { return Health > 0; } }
    public event Action OnDeath;
    public float contactDamagePercent = 0.1f; // Percentage of max health to take on contact
    public float damageInterval = 1f; // Time between damage ticks

    private bool _collidingWithPlayer = false;
    private Coroutine _damageCoroutine;


    private void Start() {
        Health = MaxHealth;
    }

    public void TakeDamage(float damage) {
        if (Health <= 0) return;

        Health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {Health}");

        if (!IsAlive) {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            _collidingWithPlayer = true;
            if (_damageCoroutine == null)
                _damageCoroutine = StartCoroutine(ApplyContinuousDamage());

        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            _collidingWithPlayer = false;
            if (_damageCoroutine != null) {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }
        }
    }



    private IEnumerator ApplyContinuousDamage() {
        while (_collidingWithPlayer) {
            float damage = contactDamagePercent * MaxHealth;
            TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
        _damageCoroutine = null; // reset coroutine reference once ended.
    }
}