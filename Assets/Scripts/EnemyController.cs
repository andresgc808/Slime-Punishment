using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private Transform target;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

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
       _rb.velocity = direction * _moveSpeed;

        UpdateSpriteFlip(direction);
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        // deal 30 damage to player and self desrtuct
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(30);
            Destroy(gameObject);
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

}