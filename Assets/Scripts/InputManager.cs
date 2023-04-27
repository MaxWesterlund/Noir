using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    [SerializeField] InputActionAsset controls;

    public InputAction Move { get; private set; }
    public InputAction Look { get; private set; }

    void Awake() {
        if (Instance != null) {
            Destroy(this);
        }
        else {
            Instance = this;
        }
    }

    void Start() {
        Move = controls.FindAction("Move");
        Look = controls.FindAction("Look");
    }

    void OnEnable() {
        controls.Enable();
    }
}