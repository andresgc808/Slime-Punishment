using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float _currentSpeedMultiplier = 1f;
    [SerializeField] private float _minSpeedMultiplier = 1f;
    [SerializeField] private float _maxSpeedMultiplier = 1.7f;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _baseMoveSpeed;

    [SerializeField] private float _sizeDecreasePerShot = 0.05f; // percentage
    [SerializeField] private float _minSizePercent = 0.5f;
    private Vector3 _baseScale;


    [SerializeField] private float _damageIncreasePerShot = 0.05f; // percentage
    [SerializeField] private float _currentDamageMultiplier = 1f;
    [SerializeField] private float _maxDamageMultiplier = 3f;

    [SerializeField] private float _healthDecreasePerShot = 0.05f; //percentage

    private Vector2 _movement;
    private Rigidbody2D _rb;
    private PlayerHealth _playerHealth;


    private float _totalSizeLost;
    private float _totalDamageIncrease;
    private float _totalHealthLost;
    private float _totalSpeedIncrease;

    private SpriteRenderer _spriteRenderer;
    private Transform _spriteTransform;

    private bool canDodge;
    private float lastDodgeTime;

    private bool dodging;


    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();
        if (_playerHealth == null)
            Debug.LogError("PlayerHealth component is missing!");
        _baseMoveSpeed = _moveSpeed;

        // Get the sprite renderer, then get the transform, for child sprite.
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer == null)
            Debug.LogError("SpriteRenderer component is missing!");

        _spriteTransform = _spriteRenderer.transform; // Get the child sprite's transform

        _baseScale = _spriteTransform.localScale;  // Base scale is now from the child.
        ResetLostSubstance();

    }
    private (float sizeLoss, float damageIncrease, float healthLoss, float speedIncrease)
        UpdateStats() {
        // Calculate changes as percentages of current values
        float sizeLoss = _sizeDecreasePerShot;
        float damageIncrease = _currentDamageMultiplier * _damageIncreasePerShot;
        float healthLoss = _playerHealth.MaxHealth * _healthDecreasePerShot / 100f;
        float speedIncrease = _currentSpeedMultiplier * 0.1f;

        // Apply size change
        _totalSizeLost += sizeLoss;
        float newSize = Mathf.Max(_minSizePercent, 1 - _totalSizeLost);


        Debug.Log($"SizeLoss: {sizeLoss}, NewSize: {newSize}, MinSizePercent: {_minSizePercent}");

        _spriteTransform.localScale = _baseScale * newSize;

        // Apply damage change
        _currentDamageMultiplier = Mathf.Min(_maxDamageMultiplier, _currentDamageMultiplier + damageIncrease);
        _totalDamageIncrease += damageIncrease;

        // Apply health change
        _playerHealth.ReduceMaxHealth(_healthDecreasePerShot * 100f);
        _totalHealthLost += healthLoss;
        // Apply Speed change
        _currentSpeedMultiplier = Mathf.Min(_maxSpeedMultiplier, _currentSpeedMultiplier + speedIncrease);
        _totalSpeedIncrease += speedIncrease;
        _moveSpeed = _baseMoveSpeed * _currentSpeedMultiplier; // update speed
        return (sizeLoss, damageIncrease, healthLoss, speedIncrease);
    }


    private void Update() {

        if (!dodging) {
            _movement.Set(InputManager.PlayerMovement.x, InputManager.PlayerMovement.y);

            _rb.velocity = _movement * _moveSpeed;
            UpdateSpriteFlip();
        }
        

        // Handle shooting
        if (Mouse.current.leftButton.wasPressedThisFrame && _playerHealth.IsAlive) {
            ShootProjectile();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(HandleDodge());
        }
    }

    private void UpdateSpriteFlip() {
        if (_movement.x > 0) {
            _spriteRenderer.flipX = false;

        } else if (_movement.x < 0) {
            _spriteRenderer.flipX = true;
        }
    }

    private void ShootProjectile() {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        Vector2 spawnPosition = (Vector2)transform.position + (direction * 0.5f);

        GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/SlimeProjectile"), spawnPosition, Quaternion.identity);
        IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
        if (projectileComponent != null) {
            if (projectileComponent is SlimeProjectile slimeProjectile) {
                slimeProjectile.DamageMultiplier = _currentDamageMultiplier;
                slimeProjectile.SetDamage();
                // Calculate the changes for this shot before updating stats.
                (float sizeLoss, float damageIncrease, float healthLoss, float speedIncrease) = UpdateStats();

                // Set the substance data on the projectile
                slimeProjectile.SetSubstanceData(sizeLoss, damageIncrease, healthLoss, speedIncrease);
            }
            projectileComponent.LaunchProjectile(spawnPosition, direction);
        }

    }

    public void IncreaseSizeAndDamage(SubstanceData substance) {
        // Recover size
        _totalSizeLost = Mathf.Max(0, _totalSizeLost - substance.sizeLoss);
        float newSize = Mathf.Max(_minSizePercent, 1 - _totalSizeLost);
        _spriteTransform.localScale = _baseScale * newSize;

        // Recover damage
        _totalDamageIncrease = Mathf.Max(0, _totalDamageIncrease - substance.damageIncrease);
        _currentDamageMultiplier = Mathf.Max(1f, _currentDamageMultiplier - substance.damageIncrease);

        // Recover health
        _totalHealthLost = Mathf.Max(0, _totalHealthLost - substance.healthLoss);
        _playerHealth.Heal(substance.healthLoss * 100f);

        // Recover speed
        _totalSpeedIncrease = Mathf.Max(0, _totalSpeedIncrease - substance.speedIncrease);
        _currentSpeedMultiplier = Mathf.Max(_minSpeedMultiplier, _currentSpeedMultiplier - substance.speedIncrease);
        _moveSpeed = _baseMoveSpeed * _currentSpeedMultiplier;

        Debug.Log($"Recovering substance: SizeLoss: {substance.sizeLoss}, DamageIncrease: {substance.damageIncrease}, HealthLost: {substance.healthLoss}, SpeedIncrease: {substance.speedIncrease}, _totalSizeLost: {_totalSizeLost}, _totalDamageIncrease: {_totalDamageIncrease}, _totalHealthLost: {_totalHealthLost}, _totalSpeedIncrease: {_totalSpeedIncrease}, newSize: {newSize}");
    }

    private IEnumerator HandleDodge() {
        Debug.Log("Dodge!");

        // check if player can dodge (3 seconds cooldown)
        if (Time.time - lastDodgeTime > 3) {
            lastDodgeTime = Time.time;
            canDodge = true;
        }

        // become invulnerable for 0.25 seconds
        if (canDodge) {
            canDodge = false;
            dodging = true;
            _playerHealth.IsInvulnerable = true;

            // apply burst of speed
            _rb.velocity = _movement * _moveSpeed * 2;

            // make sprite transparent
            _spriteRenderer.color = new Color(1, 1, 1, 0.5f);

            yield return new WaitForSeconds(0.25f); // become invulnerable for 0.25 seconds

            // reset speed
            _rb.velocity = _movement * _moveSpeed;

            // reset sprite transparency
            _spriteRenderer.color = new Color(1, 1, 1, 1);

            _playerHealth.IsInvulnerable = true;

            dodging = false;
        }
        
    }

    private void ResetLostSubstance() {
        _totalSizeLost = 0;
        _totalDamageIncrease = 0;
        _totalHealthLost = 0;
        _totalSpeedIncrease = 0;
        float newSize = Mathf.Max(_minSizePercent, 1 - _totalSizeLost);
        _spriteTransform.localScale = _baseScale * newSize;
        Debug.Log($"Substance reset, newSize:{newSize}");
    }
}