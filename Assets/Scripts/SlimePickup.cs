using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubstance {
    SubstanceData SubstanceData { get; set; }
    GameObject GameObject { get; }
    Transform Transform { get; }
}

public class SlimePickup : MonoBehaviour, ISubstance {
    public SubstanceData SubstanceData { get; set; }
    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    public SubstanceData minSubstanceData = new SubstanceData {
        sizeLoss = 0.2f,
        damageIncrease = 0.2f,
        healthLoss = 20f, // Assuming health is a flat value, not a percentage
        speedIncrease = 0.2f
    };

    private void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log($"SlimePickup Trigger: Trigger with {collision.gameObject.name}, with tag {collision.gameObject.tag}, pos: {collision.transform.position}, layer: {LayerMask.LayerToName(collision.gameObject.layer)}");

        if (collision.TryGetComponent(out PlayerHealth target)) {
            if (collision.TryGetComponent(out PlayerController player)) {

                SubstanceData finalSubstanceData = new SubstanceData {
                    sizeLoss = Mathf.Max(SubstanceData.sizeLoss, minSubstanceData.sizeLoss),
                    damageIncrease = Mathf.Max(SubstanceData.damageIncrease, minSubstanceData.damageIncrease),
                    healthLoss = Mathf.Max(SubstanceData.healthLoss, minSubstanceData.healthLoss),
                    speedIncrease = Mathf.Max(SubstanceData.speedIncrease, minSubstanceData.speedIncrease)
                };
                player.IncreaseSizeAndDamage(finalSubstanceData);
                Destroy(gameObject);
            } else {
                Debug.Log($"Does not have player controller");
            }
        } else {
            Debug.Log($"Does not have player health");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log($"SlimePickup Collision: Collision with {collision.gameObject.name}, with tag {collision.gameObject.tag}, pos: {collision.transform.position}, layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
    }
}
