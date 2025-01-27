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
    public float TotalSubstance { get; private set; } = 0;
    private float _totalSubstanceLost;
    private float _totalSizeLost;
    private float _totalDamageLost;
     private float _totalHealthLost;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();
        if (_playerHealth == null)
            Debug.LogError("PlayerHealth component is missing!");
        _baseMoveSpeed = _moveSpeed;
        _baseScale = transform.localScale;
         _totalSubstanceLost = 0;
        _totalSizeLost = 0;
        _totalDamageLost = 0;
         _totalHealthLost = 0;
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
            float reduceAmount = _playerHealth.MaxHealth * SlimeCostPercent / 100f;
            _totalHealthLost += reduceAmount;
            _playerHealth.ReduceMaxHealth(reduceAmount);
             AdjustSizeAndDamage();
             _totalSubstanceLost += reduceAmount;
               Debug.Log($"Size Multiplier after shot: {SizeMultiplier} Damage Multiplier after shot: {DamageMultiplier}, Local Scale after shot: {transform.localScale} Total Substance Lost: {_totalSubstanceLost}");
        }
    }


    private void AdjustSizeAndDamage()
    {
          // Calculate size change
          float sizeChange = SizeMultiplier * _sizeDecreasePerShot;
          // Reduce size, clamped by the minimum size
         SizeMultiplier = Mathf.Max(SizeMultiplier * (1 - _sizeDecreasePerShot), _minSizePercent);
          _totalSizeLost += sizeChange;
        // Increase damage
          float damageIncrease = _damageIncreasePerShot;
        DamageMultiplier += _damageIncreasePerShot;
        _totalDamageLost += damageIncrease;


        UpdateMovement(); // ensure move speed and scale are updated
    }
      public void IncreaseSizeAndDamage(float substance)
    {
         float sizeIncrease = Mathf.Min(substance / _totalSubstanceLost, 1f);
         float damageIncrease = Mathf.Min(substance / _totalSubstanceLost, 1f);
        float healthIncrease = Mathf.Min(substance / _totalSubstanceLost, 1f);


        SizeMultiplier += _totalSizeLost * sizeIncrease;
        DamageMultiplier += _totalDamageLost * damageIncrease;
         _playerHealth.Heal(_totalHealthLost * healthIncrease);



      TotalSubstance -= substance;
       _totalSizeLost -= _totalSizeLost * sizeIncrease;
       _totalDamageLost -= _totalDamageLost * damageIncrease;
        _totalHealthLost -= _totalHealthLost * healthIncrease;
        UpdateMovement(); // ensure move speed and scale are updated
          Debug.Log($"Size Multiplier after pickup: {SizeMultiplier} Health after pickup: {_playerHealth.MaxHealth} Damage Multiplier after pickup: {DamageMultiplier}, Local Scale after pickup: {transform.localScale} Total Substance: {TotalSubstance}");
    }
}