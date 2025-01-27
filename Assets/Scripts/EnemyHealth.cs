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
    public float substanceAbsorbtionRate = 0.5f; //how much "substance" to store
    public float substanceExcreteTime = 30f; // how long before excreting
    private float _storedSubstance = 0f;
    private bool _collidingWithPlayer = false;
    private Coroutine _damageCoroutine;
    private bool _isExcreting = false;


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
            if(_storedSubstance > 0)
                ExcreteSubstance(); // on death, always excrete if there is some.
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
      if(_isExcreting) return;
      _isExcreting = true;
      StartCoroutine(ExcreteSubstanceCoroutine());

    }


      private IEnumerator ExcreteSubstanceCoroutine() {
        yield return new WaitForSeconds(substanceExcreteTime);

       GameObject substance = Instantiate(Resources.Load<GameObject>("Prefabs/SlimePickup"), transform.position, Quaternion.identity);
        ISubstance substanceComponent = substance.GetComponent<ISubstance>();
         if(substanceComponent != null)
         {
           substanceComponent.SubstanceAmount = _storedSubstance;
          _storedSubstance = 0; // reset stored substance
          _isExcreting = false;
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