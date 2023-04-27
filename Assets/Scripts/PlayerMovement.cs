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

    [Header("Speeds")]
    [SerializeField] float moveSpeed;
    [SerializeField] float rotationSpeed;
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
    }

    void Update() {
        Move();
        Rotate();
        HeadRotation();
        BodyRotation();
    }

    void Move() {
        Vector2 moveInput = InputManager.Instance.Move.ReadValue<Vector2>();
        Vector3 moveVec = new Vector3(moveInput.x, 0, moveInput.y);

        rb.velocity = moveVec * moveSpeed;
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