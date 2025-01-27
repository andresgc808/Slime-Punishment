using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubstance
{
    float SubstanceAmount { get; set; }
    GameObject GameObject { get; }
    Transform Transform { get; }

}
public class SlimePickup : MonoBehaviour, ISubstance
{
    public float SubstanceAmount { get; set; }
     public GameObject GameObject => gameObject;
      public Transform Transform => transform;

      private void OnTriggerEnter2D(Collider2D collision) {
           Debug.Log($"SlimePickup Trigger: Trigger with {collision.gameObject.name}, with tag {collision.gameObject.tag}, pos: {collision.transform.position}, layer: {LayerMask.LayerToName(collision.gameObject.layer)}");

          if(collision.TryGetComponent(out PlayerHealth target))
          {
            if (collision.TryGetComponent(out PlayerController player))
                {
                    player.IncreaseSizeAndDamage(SubstanceAmount);
                   Destroy(gameObject);
                 }
              else
                {
                     Debug.Log($"Does not have player controller");
                 }
             }
            else
                {
                     Debug.Log($"Does not have player health");
                 }
      }

    private void OnCollisionEnter2D(Collision2D collision)
        {
             Debug.Log($"SlimePickup Collision: Collision with {collision.gameObject.name}, with tag {collision.gameObject.tag}, pos: {collision.transform.position} , layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
         }
}