using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeProjectile : MonoBehaviour, IProjectile {
    public Vector2 Direction { get; set; }
    public float Speed { get; set; } = 5f; // Default speed
    public float BaseDamage { get; set; } = 10f; // Default damage
    public float DamageMultiplier { get; set; } = 1f;
    public float Damage {get{return BaseDamage * DamageMultiplier;} }
    public float healAmount { get; set; } = 10f; // how much to heal
    public GameObject GameObject => gameObject;
    public Transform Transform => transform;
    public float collisionRadius = 0.5f; // Radius to check for collisions
    public LayerMask targetLayer;
    public LayerMask playerLayer;
    public float lifeTime = 3f; // Projectile lifetime


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
                if (collider.TryGetComponent(out IDamageable target)) {
                    target.TakeDamage(Damage);
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