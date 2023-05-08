using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    MapGeneration mapGeneration;

    public delegate void mapGenerated();
    public mapGenerated MapGenerated; 

    void Awake() {
        if (Instance != null) {
            Destroy(this);
        }
        else {
            Instance = this;
        }

        mapGeneration = GetComponent<MapGeneration>();
    }

    void Start() {
        mapGeneration.OnFinished += OnMapGenerated;

        mapGeneration.Generate();
    }

    void OnMapGenerated() {
        MapGenerated?.Invoke();
    }
}