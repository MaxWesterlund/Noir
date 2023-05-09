using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Rigidbody rb;

    [SerializeField] LayerMask ground;

    [Header("Body Part Transforms")]
    [SerializeField] Transform head;
    [SerializeField] Transform body;
    [SerializeField] Transform legs;

    [Header("Speed")]
    [SerializeField] float moveSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float rotationSpeed;

    [Header("Rotation")]
    [SerializeField] float bodyRotationSpeed;
    [SerializeField] float headRotationSpeed;

    Camera cam;

    Quaternion lastTargetRotation;

    bool canChangeAngle;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Start() {
        cam = Camera.main;

        GameManager.Instance.MapGenerated += OnMapGenerated;
    }

    void Update() {
        Move();
        Rotate();
        HeadRotation();
        BodyRotation();
    }

    void OnMapGenerated() {
        transform.position = GameObject.Find("Spawn").transform.position;
    }

    void Move() {
        Vector2 moveInput = InputManager.Instance.Move.ReadValue<Vector2>();

        Vector3 moveVec = new Vector3(moveInput.x, 0, moveInput.y);

        rb.velocity = Vector3.Lerp(rb.velocity, moveVec * moveSpeed, acceleration * Time.deltaTime);
    }

    void Rotate() {
        transform.rotation = Quaternion.Slerp(transform.rotation, head.rotation, rotationSpeed * Time.deltaTime);
    }

    void HeadRotation() {
        Vector2 lookInput = InputManager.Instance.Look.ReadValue<Vector2>();
        Ray camRay = cam.ScreenPointToRay(lookInput);
        Vector3 cursorPos = Vector3.zero;
        if (Physics.Raycast(camRay.origin, camRay.direction, out RaycastHit hit, Mathf.Infinity, ground)) {
            cursorPos = hit.point;
        }

        cursorPos.y = head.position.y;

        Vector3 direction = (head.InverseTransformPoint(cursorPos) - head.localPosition).normalized;

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(0, angle, 0), headRotationSpeed * Time.deltaTime);
    }

    void BodyRotation() {
        body.rotation = Quaternion.Slerp(body.rotation, head.rotation, bodyRotationSpeed * Time.deltaTime);
    }
}