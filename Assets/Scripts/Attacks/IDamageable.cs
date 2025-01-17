using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(float damage);
    bool IsAlive { get; }
    event Action OnDeath;
}
