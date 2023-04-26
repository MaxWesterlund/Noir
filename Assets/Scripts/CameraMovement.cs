using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour {
    [SerializeField] InputActionAsset controls;

    [SerializeField] LayerMask ground;

    [SerializeField] Transform player;
    
    [SerializeField] float height;

    [SerializeField] float moveSpeed;

    [Range(0, 1)]
    [SerializeField] float positionInterpolationValue;

    Camera cam;

    InputAction lookAction;

    void OnEnable() {
        controls.Enable();
    }

    void Start() {
        cam = Camera.main;
        
        lookAction = controls.FindAction("Look");
    }

    void FixedUpdate() {
        Vector3 positionA = new Vector3(player.position.x, 0, player.position.z);
        Ray camRay = cam.ScreenPointToRay(lookAction.ReadValue<Vector2>());
        Vector3 cursorPos = Vector3.zero;
        if (Physics.Raycast(camRay.origin, camRay.direction, out RaycastHit hit, Mathf.Infinity, ground)) {
            cursorPos = hit.point;
        }
        Vector3 positionB = new Vector3(cursorPos.x, 0, cursorPos.z);
        Vector3 targetPosition = Vector3.Lerp(positionA, positionB, positionInterpolationValue) + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
    }
}