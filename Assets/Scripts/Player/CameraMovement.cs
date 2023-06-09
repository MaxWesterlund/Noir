using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    [SerializeField] LayerMask ground;

    [SerializeField] Transform player;
    
    [SerializeField] float height;

    [SerializeField] float moveSpeed;

    [Range(0, 1)]
    [SerializeField] float positionInterpolationValue;

    Camera cam;

    void Start() {
        cam = Camera.main;
    }

    void FixedUpdate() {
        Vector3 positionA = new Vector3(player.position.x, 0, player.position.z);
        Ray camRay = cam.ScreenPointToRay(InputManager.Instance.Look.ReadValue<Vector2>());
        Vector3 cursorPos = camRay.origin;
        Vector3 positionB = new Vector3(cursorPos.x, 0, cursorPos.z);

        Vector3 targetPosition = Vector3.Lerp(positionA, positionB, positionInterpolationValue) + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
    }
}