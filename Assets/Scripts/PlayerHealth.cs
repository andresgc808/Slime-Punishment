// PlayerHealth.cs
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float MaxHealth;
    public float Health { get; set; }
    public float Armor { get; set; }
    public bool IsAlive { get { return Health > 0; } }

    public event Action OnDeath;

    public HealthTracker healthTracker;

    public void Start()
    {
        Health = MaxHealth;

        if (healthTracker != null) // initally invisible
            healthTracker.gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (Health <= 0) return;

        float damageReceived = damage;
        Health -= damageReceived; // armor can be changed later to be more complex

        Debug.Log($"{gameObject.name} took {damageReceived} damage. Health: {Health}");

        UpdateHealthUI();

        if (!IsAlive)
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
        }
    }
     public void Heal(float healAmount)
    {
        if (Health >= MaxHealth) return;
        float healed = Mathf.Min(healAmount, MaxHealth - Health);
        Health += healed;
        Debug.Log($"{gameObject.name} healed {healed}. Health: {Health}");
         UpdateHealthUI();
    }
     public void ReduceMaxHealth(float reducePercent) // Changed this to a percentage not value
    {
        if (MaxHealth <= 0) return;
        float reduced = MaxHealth * reducePercent / 100f; // Calculate reduction as a percentage
        MaxHealth -= reduced;
         Health = Mathf.Min(Health,MaxHealth); // makes sure current health isn't more than new maxhealth

        Debug.Log($"{gameObject.name} Max health Reduced by {reduced}. Health: {Health}. Max Health: {MaxHealth}");
         UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {

        if (healthTracker != null)
        {
            healthTracker.gameObject.SetActive(true);
            healthTracker.UpdateSliderValue(Health, MaxHealth);
        }
    }
}