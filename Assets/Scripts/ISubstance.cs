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
          if(collision.TryGetComponent(out PlayerHealth target))
          {
            if (collision.TryGetComponent(out PlayerController player))
                {
                   target.Heal(SubstanceAmount);
                    Destroy(gameObject);
                 }
             }
      }


}