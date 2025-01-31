using System.Collections;
using UnityEngine;

public class BlueBullet : MonoBehaviour, IProjectile {
    public Vector2 Direction { get; set; }
    public float Speed { get; set; } = 7f; 
    public float BaseDamage { get; set; } = 15f;
    public float Damage { get; private set; }
    public float DamageMultiplier { get; set; } = 1f; 
    public GameObject GameObject => gameObject;
    public Transform Transform => transform;
    public float collisionRadius = 0.3f;
    public LayerMask targetLayer;
    public float lifeTime = 3f;

    public void Awake() {
        SetDamage();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // Put it on a higher layer name like "Foreground"
        sr.sortingLayerName = "Foreground"; 
        // And/or bump up that sorting order to some big spicy number
        sr.sortingOrder = 999; 
    }

    public void SetDamage() {
        Damage = BaseDamage * DamageMultiplier;
        //Debug.Log($"Set Damage called, Base Damage: {BaseDamage}, Damage Multiplier: {DamageMultiplier}, Resulting Damage: {Damage}");
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
                if (collider.TryGetComponent(out IDamageable target)) {
                    Debug.Log($"Projectile hit target! Dealing {Damage} damage.");
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

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent(out IDamageable target)) {
            // check if is player
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
                target.TakeDamage(Damage);
                DestroyProjectile();
            }
        }
    }
}
