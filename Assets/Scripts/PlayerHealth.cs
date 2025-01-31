using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable {
    public float MaxHealth;
    public float Health { get; set; }
    public float Armor { get; set; }
    public bool IsAlive { get { return Health > 0; } }

    public event Action OnDeath;

    public HealthTracker healthTracker;

    public bool IsInvulnerable { get; set; }

    public void Start() {
        Health = MaxHealth;

        if (healthTracker != null) // initally invisible
            healthTracker.gameObject.SetActive(false);
    }

    public void TakeDamage(float damage) {
        if (Health <= 0) return;

        if (IsInvulnerable) return;

        float damageReceived = damage;
        Health -= damageReceived; // armor can be changed later to be more complex

        Debug.Log($"{gameObject.name} took {damageReceived} damage. Health: {Health}");

        UpdateHealthUI();

        if (!IsAlive) {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");

            // reset the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void Heal(float healAmount) {
        if (Health >= MaxHealth) return;
        float healed = Mathf.Min(healAmount, MaxHealth - Health);
        Health += healed;
        Debug.Log($"{gameObject.name} healed {healed}. Health: {Health}");
        UpdateHealthUI();
    }

    public void ReduceMaxHealth(float reduceAmount) {
        if (MaxHealth <= 90) return; // Ensure MaxHealth does not go below 90
        MaxHealth = Mathf.Max(90, MaxHealth - reduceAmount);
        Health = Mathf.Min(Health, MaxHealth); // makes sure current health isn't more than new maxhealth

        Debug.Log($"{gameObject.name} Max health Reduced by {reduceAmount}. Health: {Health}. Max Health: {MaxHealth}");
        UpdateHealthUI();
    }

    private void UpdateHealthUI() {
        if (healthTracker != null) {
            healthTracker.gameObject.SetActive(true);
            healthTracker.UpdateSliderValue(Health, MaxHealth);
        }
    }
}
