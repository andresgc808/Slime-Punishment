using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _baseMoveSpeed = 5f;
    [SerializeField] private float _maxSpeedMultiplier = 1.7f;
    [SerializeField] private float _sizeDecreasePerShot = 0.05f; // percentage
    [SerializeField] private float _damageIncreasePerShot = 0.10f; // percentage
    [SerializeField] private float _minSizePercent = 0.2f;
    [SerializeField] private float _maxSizeMultiplier = 1f;


    private Vector3 _baseScale;
    private Vector2 _movement;
    private Rigidbody2D _rb;
    private PlayerHealth _playerHealth;

    public float SlimeCostPercent { get; set; } = 2f;
    public float SizeMultiplier { get; private set; } = 1;
    public float DamageMultiplier { get; private set; } = 1;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();
        if (_playerHealth == null)
            Debug.LogError("PlayerHealth component is missing!");
        _baseMoveSpeed = _moveSpeed;
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        _movement.Set(InputManager.PlayerMovement.x, InputManager.PlayerMovement.y);

        UpdateMovement();

        // Handle shooting
        if (Mouse.current.leftButton.wasPressedThisFrame && _playerHealth.IsAlive)
        {
            ShootProjectile();
        }
    }

      private void UpdateMovement()
    {
        // Calculate speed based on multipliers
        float speedMultiplier = Mathf.Lerp(1f, _maxSpeedMultiplier, 1 - SizeMultiplier);
        _moveSpeed = _baseMoveSpeed * speedMultiplier;

        // Apply movement and scale
        transform.localScale = _baseScale * SizeMultiplier;
        _rb.velocity = _movement * _moveSpeed;
    }

    private void ShootProjectile()
    {
         // Log for debugging
         Debug.Log($"Size Multiplier before shot: {SizeMultiplier} Damage Multiplier before shot: {DamageMultiplier}, Local Scale before shot: {transform.localScale}");

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        Vector2 spawnPosition = (Vector2)transform.position + (direction * 0.5f);

        GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/SlimeProjectile"), spawnPosition, Quaternion.identity);
        IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
        if (projectileComponent != null)
        {
            if (projectileComponent is SlimeProjectile slimeProjectile)
            {
                 slimeProjectile.DamageMultiplier = DamageMultiplier;
                 slimeProjectile.SetDamage();
            }
            
            projectileComponent.LaunchProjectile(spawnPosition, direction);
             _playerHealth.ReduceMaxHealth(SlimeCostPercent);
             AdjustSizeAndDamage();
              Debug.Log($"Size Multiplier after shot: {SizeMultiplier} Damage Multiplier after shot: {DamageMultiplier}, Local Scale after shot: {transform.localScale}");
        }
    }


    private void AdjustSizeAndDamage()
    {
        // Reduce size, clamped by the minimum size
        SizeMultiplier = Mathf.Max(SizeMultiplier * (1 - _sizeDecreasePerShot), _minSizePercent);
        // Increase damage
        DamageMultiplier += _damageIncreasePerShot;

        UpdateMovement(); // ensure move speed and scale are updated
    }
}