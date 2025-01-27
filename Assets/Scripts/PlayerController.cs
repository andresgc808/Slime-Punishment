using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private LayerMask _projectileLayer;

    private Vector2 _movement;
    private Rigidbody2D _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        _movement.Set(InputManager.PlayerMovement.x, InputManager.PlayerMovement.y);

        _rb.velocity = _movement * _moveSpeed;

        // calculate angle of mouse, if left click, shoot projectile
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

            GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/SlimeProjectile"), transform.position, Quaternion.identity);
            IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
            if (projectileComponent != null)
                projectileComponent.LaunchProjectile(transform.position, direction);
        }
    }

    private void OnEnable() {
        Physics2D.IgnoreLayerCollision(gameObject.layer, _projectileLayer, true);
    }

} 