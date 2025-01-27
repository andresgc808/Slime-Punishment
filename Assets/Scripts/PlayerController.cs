using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _baseMoveSpeed = 5f; // Track base speed
    [SerializeField] private float _maxSpeedMultiplier = 1.7f; // max speed to multiply by

    private Vector3 _baseScale; // Store base scale
    [SerializeField] private float _minSizePercent = 0.5f;
    [SerializeField] private float _maxSizeMultiplier = 1f;
    private Vector2 _movement;
    private Rigidbody2D _rb;

    private PlayerHealth _playerHealth;
    public float SlimeCost { get; set; } = 2f; // cost per shot

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();
        if (_playerHealth == null)
            Debug.LogError("PlayerHealth component is missing!");
        _baseMoveSpeed = _moveSpeed;
        _baseScale = transform.localScale; // gets the initial scale
    }

    private void Update()
    {
        _movement.Set(InputManager.PlayerMovement.x, InputManager.PlayerMovement.y);
        // calculate speed and size based on health
        float healthPercent = Mathf.Clamp01(_playerHealth.Health / _playerHealth.MaxHealth);
        float speedMultiplier = Mathf.Lerp(_maxSpeedMultiplier, 1f, healthPercent);
        float sizeMultiplier;
        if(healthPercent <= 1 && healthPercent >= _minSizePercent)
           sizeMultiplier = Mathf.Lerp(_minSizePercent, _maxSizeMultiplier, healthPercent);
        else
             sizeMultiplier = Mathf.Lerp(_minSizePercent, _maxSizeMultiplier, 1f);


        _moveSpeed = _baseMoveSpeed * speedMultiplier;
        transform.localScale = _baseScale * sizeMultiplier; // scale based on the _baseScale
        _rb.velocity = _movement * _moveSpeed;


        // calculate angle of mouse, if left click, shoot projectile
        if (Mouse.current.leftButton.wasPressedThisFrame && _playerHealth.Health > 0) // Dont shoot if dead
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            Vector2 spawnPosition = (Vector2)transform.position + (direction * 0.5f);

            GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/SlimeProjectile"), spawnPosition, Quaternion.identity);
            IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
            if (projectileComponent != null)
            {
                 Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectile.GetComponent<Collider2D>(), true);
                projectileComponent.LaunchProjectile(spawnPosition, direction);
                // Set projectile damage based on the current scale
                if(projectileComponent is SlimeProjectile slimeProjectile)
                {
                 slimeProjectile.DamageMultiplier = 1/sizeMultiplier;
                }

                // Decrease stats on shoot
                _playerHealth.ReduceMaxHealth(SlimeCost);

            }
        }
    }


}