using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float _moveSpeed = 5f;

    private Vector2 _movement;
    private Rigidbody2D _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        _movement.Set(InputManager.PlayerMovement.x, InputManager.PlayerMovement.y);

        _rb.velocity = _movement * _moveSpeed;
    }

} 