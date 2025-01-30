using System.Collections;
using UnityEngine;

public class SlimeProjectile : MonoBehaviour, IProjectile {
    public Vector2 Direction { get; set; }
    public float Speed { get; set; } = 5f;
    public float BaseDamage { get; set; } = 10f;
    public float DamageMultiplier { get; set; } = 1f;
    public float Damage { get; private set; }
    public float healAmount { get; set; } = 10f;
    public GameObject GameObject => gameObject;
    public Transform Transform => transform;
    public float collisionRadius = 0.5f;
    public LayerMask targetLayer;
    public LayerMask playerLayer;
    public float lifeTime = 3f;

    public float SizeLost { get; private set; }
    public float DamageIncrease { get; private set; }
    public float HealthLost { get; private set; }
    public float SpeedIncrease { get; private set; }

    private void Awake() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Foreground";
        sr.sortingOrder = 999;
    }

    public void SetSubstanceData(float sizeLost, float damageIncrease, float healthLost, float speedIncrease) {
        SizeLost = sizeLost;
        DamageIncrease = damageIncrease;
        HealthLost = healthLost;
        SpeedIncrease = speedIncrease;
    }

    public void SetDamage() {
        Damage = BaseDamage * DamageMultiplier;
        Debug.Log($"Set Damage called, Base Damage: {BaseDamage}, Damage Multiplier: {DamageMultiplier}, Resulting Damage: {Damage}");
    }


    public void LaunchProjectile(Vector2 startingPosition, Vector2 direction) {
        transform.position = startingPosition;
        StartCoroutine(MoveProjectile(direction));
    }

    public void DestroyProjectile() {
        Destroy(gameObject);
    }

    private IEnumerator MoveProjectile(Vector2 direction) {
        float lifeTimer = 0f;
        while (lifeTimer < lifeTime) {
            transform.Translate(direction * Speed * Time.deltaTime);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, collisionRadius, targetLayer);

            foreach (var collider in colliders) {
                if (collider == null)
                    continue;
                if (collider.TryGetComponent(out IDamageable target)) {

                    Debug.Log($"Projectile hit target! Dealing {Damage} damage, sizeLost:{SizeLost}, DamageIncrease: {DamageIncrease}, HealthLost: {HealthLost}, SpeedIncrease: {SpeedIncrease}");
                    if (target is EnemyHealth enemyHealth) {
                        enemyHealth.StoreSubstance(SizeLost, DamageIncrease, HealthLost, SpeedIncrease);
                        if (target != null)
                            target.TakeDamage(Damage);

                    }
             
                    DestroyProjectile();
                    yield break;
                }
            }

            lifeTimer += Time.deltaTime;
            yield return null;
        }
        DestroyProjectile();
    }
}