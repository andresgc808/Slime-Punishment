using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttacker {
    void Attack(IDamageable target);
    float AttackRange { get; }
    float AttackPower { get; }
    float FireRate { get; }
}