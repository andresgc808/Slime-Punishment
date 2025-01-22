using UnityEngine;

public interface IProjectile {

    Vector2 Direction { get; set; }

    float Speed { get; set; }

    float Damage { get; set; }

    GameObject GameObject { get; }

    void LaunchProjectile(Vector2 startingPosition, Vector2 direction);

    void DestroyProjectile();

    Transform Transform { get; }

}