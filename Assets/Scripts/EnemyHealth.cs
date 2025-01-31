using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SubstanceData
{
    public float sizeLoss;
    public float damageIncrease;
    public float healthLoss;
    public float speedIncrease;
}

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float MaxHealth = 100f;
    public float Health { get; set; }
    public bool IsAlive { get { return Health > 0; } }
    public event Action OnDeath;
    public float contactDamagePercent = 0.1f;
    public float damageInterval = 1f;
    public string collisionSoundName;
    private float _storedSizeLoss = 0f;
    private float _storedDamageIncrease = 0f;
    private float _storedHealthLost = 0f;
    private float _storedSpeedIncrease = 0f;

    private bool _collidingWithPlayer = false;
    private Coroutine _damageCoroutine;

    [SerializeField] public bool isSpanwer = false;

    [SerializeField] public float spawnerNumber = 0;



    private void Start()
    {
        Health = MaxHealth;
    }
    public void StoreSubstance(float sizeLoss, float damageIncrease, float healthLoss, float speedIncrease)
    {
        _storedSizeLoss += sizeLoss;
        _storedDamageIncrease += damageIncrease;
        _storedHealthLost += healthLoss;
        _storedSpeedIncrease += speedIncrease;
    }
    public void StoreSubstance(float substance)
    {
        _storedSizeLoss += substance;
    }

    public void TakeDamage(float damage)
    {
        if (Health <= 0) return;

        Health -= damage;
        //_storedSubstance += damage; // Add to stored substance

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {Health}");

        if (!IsAlive)
        {
            OnDeath?.Invoke();

            if (SoundManager.instance != null)
                SoundManager.instance.Stop(collisionSoundName);

            if (RunManager.Instance != null)
                RunManager.Instance.DecreaseRemainingEnemies();

            Debug.Log($"{gameObject.name} has died.");
            ExcreteSubstance(); // Excrete if there is substance

            if (isSpanwer) {
                // find object of type spawner and decrease count
                Spawner spawner = FindObjectOfType<Spawner>();
                if (spawner != null) {
                    spawner.DecreaseSpawnerCount(spawnerNumber);
                }
            }
            Destroy(gameObject);

        }
    }
  
    private void ExcreteSubstance()
    {
        if (_storedSizeLoss <= 0 && _storedDamageIncrease <= 0 && _storedHealthLost <= 0 && _storedSpeedIncrease <= 0) return;
        GameObject substance = Instantiate(Resources.Load<GameObject>("Prefabs/SlimePickup"), transform.position, Quaternion.identity);
        ISubstance substanceComponent = substance.GetComponent<ISubstance>();
        if (substanceComponent != null)
        {
            SubstanceData substanceData = new SubstanceData
            {
                sizeLoss = _storedSizeLoss,
                damageIncrease = _storedDamageIncrease,
                healthLoss = _storedHealthLost,
                speedIncrease = _storedSpeedIncrease
            };
            substanceComponent.SubstanceData = substanceData;
            Debug.Log($"Excreting substance: SizeLoss: {_storedSizeLoss}, DamageIncrease: {_storedDamageIncrease}, HealthLost: {_storedHealthLost}, SpeedIncrease: {_storedSpeedIncrease}");
        }
        _storedSizeLoss = 0;
        _storedDamageIncrease = 0;
        _storedHealthLost = 0;
        _storedSpeedIncrease = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collidingWithPlayer = true;
            if (_damageCoroutine == null)
                _damageCoroutine = StartCoroutine(ApplyContinuousDamage());

            if (SoundManager.instance != null)
                SoundManager.instance.Play(collisionSoundName);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _collidingWithPlayer = false;
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;

                if (SoundManager.instance != null)
                    SoundManager.instance.Stop(collisionSoundName);
            }
        }
    }

    private IEnumerator ApplyContinuousDamage()
    {
        while (_collidingWithPlayer)
        {
            float damage = contactDamagePercent * MaxHealth;
            TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
        _damageCoroutine = null; // reset coroutine reference once ended.
    }
}
