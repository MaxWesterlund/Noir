using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    Rigidbody rb;

    [SerializeField] InputActionAsset controls;

    [SerializeField] float moveSpeed;
    
    Camera cam;

    InputAction moveAction;
    InputAction lookAction;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Start() {
        cam = Camera.main;

        moveAction = controls.FindAction("Move");
        lookAction = controls.FindAction("Look");
    }
    
    void OnEnable() {
        controls.Enable();
    }

    void Update() {
        Move();
        Look();
    }

    void Move() {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveVec = new Vector3(moveInput.x, 0, moveInput.y);

        rb.velocity = moveVec * moveSpeed;
    }

    void Look() {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        Vector3 mousePos = cam.ScreenToWorldPoint(lookInput);
        mousePos.y = 0;
        
        transform.LookAt(mousePos + Vector3.up * transform.position.y);
    }
}