using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPMiniBoss : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private List<Transform> teleportPositions;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float minPlayerCirclingDistance = 2f;
    [SerializeField] private float maxPlayerCirclingDistance = 4f;
    [SerializeField] private float _moveSpeed = 2f;

    [SerializeField] private float agroRange = 5f;

    private Rigidbody2D _rb;

    private enum State
    {
        CirclePlayer,
        TeleportAttack,

        Idle,
    }

    private State _currentState;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (target == null) // sets target to player if not set
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void Start()
    {
        _currentState = State.Idle;
        StartCoroutine(StateMachine());
    }

    private void Update()
    {
        if (target == null) return; // don't do anything if the player doesn't exist

        // being attack when player enters agro range for the first time 
        if (_currentState == State.Idle && Vector2.Distance(transform.position, target.position) < agroRange)
        {
            _currentState = State.CirclePlayer;
        }

    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            switch (_currentState)
            {
                case State.Idle:
                    // do nothing for now
                    break;
                case State.CirclePlayer:
                    yield return StartCoroutine(CirclePlayerCoroutine());
                    break;
                case State.TeleportAttack:
                    yield return StartCoroutine(TeleportAttackCoroutine());
                    break;
            }
        }
    }

    private IEnumerator CirclePlayerCoroutine()
    {
        // Moves in a circle around the player
        // the enemy never stops moving while in this state
        // maintains a min distance minPlayerCirclingDistance and a max distance of maxPlayerCirclingDistance from the player
        // updates distance from player as a sine wave with a period of 2 seconds between minPlayerCirclingDistance and maxPlayerCirclingDistance
        // while circling, shoots a bullet at the player every 0.5 seconds
        // transitions to the teleport attack state after 15 seconds
        float stateDuration = 15f;
        float elapsedTime = 0f;

        while (elapsedTime < stateDuration)
        {
            float distance = minPlayerCirclingDistance + (maxPlayerCirclingDistance - minPlayerCirclingDistance) * Mathf.Sin(Time.time / 2);
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector2 circlePosition = (Vector2)target.position + perpendicular * distance;
            Vector2 moveDirection = (circlePosition - (Vector2)transform.position).normalized;
            _rb.velocity = moveDirection * _moveSpeed;

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(CircleShootingAttack());

            elapsedTime += 0.5f;
        }

        _currentState = State.TeleportAttack;
    }

    private IEnumerator CircleShootingAttack()
    {
        GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/BlueBullet"), transform.position, Quaternion.identity);
        IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
        if (projectileComponent != null)
        {
            projectileComponent.LaunchProjectile(transform.position, ((Vector2)target.position - (Vector2)transform.position).normalized);
        }
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator TeleportAttackCoroutine()
    {
        int teleportCount = 3;

        for (int i = 0; i < teleportCount; i++)
        {
            Transform teleportPosition = pickTeleportPosition();
            transform.position = teleportPosition.position;

            yield return new WaitForSeconds(1f);
            ShootBarrage();
            yield return new WaitForSeconds(0.5f);
        }

        _currentState = State.CirclePlayer;
    }

    private void ShootBarrage()
    {
        StartCoroutine(ShootBarrageCoroutine());
    }

    private IEnumerator ShootBarrageCoroutine()
    {
        Vector2 direction = ((Vector2)playerPosition.position - (Vector2)transform.position).normalized;
        Vector2 leftDirection = Quaternion.Euler(0, 0, 5) * direction;
        Vector2 rightDirection = Quaternion.Euler(0, 0, -5) * direction;

        for (int i = 0; i < 3; i++)
        {
            GameObject projectile = Instantiate(Resources.Load<GameObject>("Prefabs/BlueBullet"), transform.position, Quaternion.identity);
            IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
            if (projectileComponent != null)
            {
                if (i == 0)
                {
                    projectileComponent.LaunchProjectile(transform.position, direction);
                }
                else if (i == 1)
                {
                    projectileComponent.LaunchProjectile(transform.position, leftDirection);
                }
                else
                {
                    projectileComponent.LaunchProjectile(transform.position, rightDirection);
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private Transform pickTeleportPosition()
    {
        return teleportPositions[Random.Range(0, teleportPositions.Count)];
    }
}