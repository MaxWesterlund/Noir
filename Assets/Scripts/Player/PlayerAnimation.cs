using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour {
    Animator animator;
    Rigidbody rb;

    void Awake() {
        animator = GetComponent<Animator>();
        rb = transform.parent.GetComponent<Rigidbody>();
    }

    void Update() {
        if (rb.velocity.magnitude > .01f) {
            animator.SetBool("IsWalking", true);
        }
        else {
            animator.SetBool("IsWalking", false);
        }
    }
}