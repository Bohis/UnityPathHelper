using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public float speed;
	public float jumpForce;

	private Controls inputs;
	private Rigidbody2D rb;

	private bool isCanJump = false;

	private LookAround lookAround;

	private void Awake() {
		inputs = new Controls();
		rb = GetComponent<Rigidbody2D>();
		lookAround = GetComponent<LookAround>();
	}

	private void OnEnable() {
		inputs.Enable();
	}

	private void OnDisable() {
		inputs.Enable();
	}

	private void Start() {
		inputs.Player.Jump.performed += Jump_performed;
        inputs.Player.PickUp.performed += PickUp_performed;
	}

    private void PickUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		lookAround.PickUp();
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		if (isCanJump) rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
	}
	private void Move() {
		float side = inputs.Player.Move.ReadValue<float>();
		transform.position += Vector3.right * side * speed * Time.fixedDeltaTime;
	}

	private void FixedUpdate() {
		Move();
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		isCanJump = true;
	}

	private void OnCollisionExit2D(Collision2D collision) {
		isCanJump = false;
	}

	private void OnCollisionStay2D(Collision2D collision) {
		isCanJump = true;
	}
}