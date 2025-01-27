// EnemyHealth.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable {
    public float MaxHealth = 100f;
    public float Health { get; set; }
    public bool IsAlive { get { return Health > 0; } }
    public event Action OnDeath;
    public float contactDamagePercent = 0.1f;
    public float damageInterval = 1f;
    public float substanceAbsorbtionRate = 0.5f;
    private float _storedSubstance = 0f;
     private bool _collidingWithPlayer = false;
    private Coroutine _damageCoroutine;

    private void Start() {
        Health = MaxHealth;
    }
    public void TakeDamage(float damage) {
         if (Health <= 0) return;

        Health -= damage;
        _storedSubstance += damage * substanceAbsorbtionRate; // Add to stored substance

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {Health}");

        if (!IsAlive) {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
            ExcreteSubstance(); // Excrete if there is substance
            Destroy(gameObject);

        }
    }
     private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent(out IProjectile projectile))
          TakeDamage(projectile.Damage);

    }
    private void ExcreteSubstance()
    {
      if(_storedSubstance <= 0) return;
        GameObject substance = Instantiate(Resources.Load<GameObject>("Prefabs/SlimePickup"), transform.position, Quaternion.identity);
        ISubstance substanceComponent = substance.GetComponent<ISubstance>();
         if(substanceComponent != null)
         {
           substanceComponent.SubstanceAmount = _storedSubstance;
            Debug.Log($"Excreting substance: {_storedSubstance}");
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