using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class PathBuildHelper : MonoBehaviour {
	private Seeker seeker;
	private Path path;
	private int indexPoint = 0;
	private Rigidbody2D rb;

	private Vector3 nowSpeed;
	private Vector3 accseleration;

	[SerializeField]
	private float speed;
	[SerializeField]
	private float timeSmooth = 0.1f;
	[SerializeField]
	public float jumpForce;
	[SerializeField]
	public float distanceStop;
	[SerializeField]
	private float timeUpdatePath;
	[SerializeField]
	public float distanceTouchPoint;
	[SerializeField]
	public bool isCanJump;
	[SerializeField]
	[Range(0,1)]
	public float jumpSide;


	public Transform pointJump;
	public float radiusGroundCheck;
	public LayerMask layerGround;
	public Transform Target;

	public bool isDraw;
	public Color colorDraw = Color.blue;

	public bool Stop { get; set; }
	public bool isWalk { get; private set; }
	public bool IsCloseToTarget { get; private set; }
	public bool isGround { get; private set; }

	private void Awake() {
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();
		Stop = false;
		isWalk = false;
		IsCloseToTarget = false;
		isGround = true;
	}

	private void Start() {
		InvokeRepeating(nameof(PathBuilder), 0, timeUpdatePath);
	}

	private void OnDrawGizmos() {
		if (!isDraw) return;

		Gizmos.color = colorDraw;
		Gizmos.DrawWireSphere(pointJump.position, radiusGroundCheck);
	}

	private void PathBuilder(){
		if (seeker.IsDone()) {
			seeker.StartPath(transform.position, Target.position, EndPath);
		}
	}

	private void EndPath(Path p){
		path = p;
		indexPoint = 0;
	}

	private bool GetIsGround(){
		Collider2D[] colliders = Physics2D.OverlapCircleAll(pointJump.position, radiusGroundCheck, layerGround);

		return colliders.Length > 0;
	}

	private void Flip(float side){
		if (side > 0){
			if (transform.localScale.x < 0){
				Vector3 temp = transform.localScale;
				temp.x *= -1;
				transform.localScale = temp;
			}
			return;
		}

		if(side < 0){
			if (transform.localScale.x > 0) {
				Vector3 temp = transform.localScale;
				temp.x *= -1;
				transform.localScale = temp;
			}
			return;
		}
	}

	public void Move() {
		IsCloseToTarget = Vector3.Distance(transform.position, Target.position) <= distanceStop;

		if (
		Stop ||
		IsCloseToTarget ||
		path == null ||
		indexPoint >= path.vectorPath.Count
		) {
			isWalk = false;
			return;
		}

		isWalk = true;

		Vector3 vectorWay = path.vectorPath[indexPoint] - transform.position;
		Vector3 sideWay = vectorWay.normalized;

		bool stayOnGround = GetIsGround();

		Flip(sideWay.x);

		if (isCanJump && stayOnGround) {
			if (sideWay.y >= jumpSide) {
				rb.Sleep();
				rb.AddForce(((Vector2)sideWay + Vector2.up) * jumpForce, ForceMode2D.Impulse);
			}
		}

		sideWay.y = 0;
		nowSpeed = Vector3.SmoothDamp(nowSpeed, sideWay * speed, ref accseleration, timeSmooth);
		transform.position += nowSpeed * Time.fixedDeltaTime;

		if (vectorWay.magnitude <= distanceTouchPoint) {
			indexPoint+=2;
		}
	}

	private void FixedUpdate() {
		Move();
	}
}