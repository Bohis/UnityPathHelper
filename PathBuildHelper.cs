using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// ������ �������� npc
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class PathBuildHelper : MonoBehaviour {
	/// <summary>
	/// ��������� ����
	/// </summary>
	private Seeker seeker;

	/// <summary>
	/// ����
	/// </summary>
	private Path path;

	/// <summary>
	/// ������ �����, � ������� ����� ����
	/// </summary>
	private int indexPoint = 0;

	/// <summary>
	/// Rigidbody2D ���������
	/// </summary>
	private Rigidbody2D rb;

	/// <summary>
	/// �������� � ������ ������
	/// </summary>
	private Vector3 nowSpeed;

	/// <summary>
	/// ��������� � ������ ������
	/// </summary>
	private Vector3 accseleration;

	// =====================================================================

	[Header("��������")]
	#region Move
	/// <summary>
	/// ������������ �������� ���������
	/// </summary>
	[Tooltip("��������")]
	[SerializeField]
	private float speed = 1;
	/// <summary>
	/// ���������� �������� �������� �������� ��������� � �������
	/// </summary>
	[SerializeField]
	[Tooltip("�������� �������� �������� ��������� � �������")]
	[Range(0, 1)]
	public float speedMoveAir = 0.5f;
	/// <summary>
	/// ���� ��������
	/// </summary>
	[SerializeField]
	[Tooltip("���� ��� ����")]
	public Transform Target;

	#endregion

	[Header("����� ����")]
	#region SearchPath
	/// <summary>
	/// ������������ ���������, �� ������� ����� ������� �������� � ����� ����
	/// </summary>
	[SerializeField]
	[Tooltip("��������� ����� �������� ��������������� � ����")]
	public float distanceStop = 0.1f;
	/// <summary>
	/// ����� ����������� ����
	/// </summary>
	[SerializeField]
	[Tooltip("����� ��� ���������� ����")]
	[Range(0.3f, 1f)]
	private float timeUpdatePath = 0.5f;
	/// <summary>
	/// ���������� ����������� � �������� ��������� �� ����
	/// </summary>
	[SerializeField]
	[Tooltip("���������� �� ������� �������� ����� �� ����� �� ����� ����")]
	public float distanceTouchPoint = 0.3f;
	#endregion

	[Header("����������� ���������")]
	#region Setting
	/// <summary>
	/// ��������������� �������� ������� ����������� ��� ���������� ������ (�� ������� ��������� �� ��������� �����)
	/// </summary>
	[SerializeField]
	[Tooltip("����������� ����������� ������ ������ (��������������� �������� ������� ����������� ��� ���������� ������ (�� ������� ��������� �� ��������� �����))")]
	[Range(0, 1)]
	public float jumpSide = 0.5f;
	/// <summary>
	/// ����� ������� � ���������� ��������� � ���.
	/// </summary>
	[Tooltip("��������� ���������")]
	[SerializeField]
	private float timeSmooth = 0.1f;
	/// <summary>
	/// ��������������� �������� ������� (�� x) ����������� ��� ���������� �������� (�� ������� ��������� �� ��������� �����)
	/// </summary>
	[SerializeField]
	[Tooltip("����������� ����������� ��� �������� (��������������� �������� ������� (�� x) ����������� ��� ���������� �������� (�� ������� ��������� �� ��������� �����))")]
	[Range(0, 1)]
	public float flipSide = 0.5f;
	#endregion

	[Header("������")]
	#region JumpSetting
	/// <summary>
	/// �������� �� ������
	/// </summary>
	[SerializeField]
	[Tooltip("���������� ������")]
	public bool isCanJump = true;
	/// <summary>
	/// ����� ����������, ��� �������� ������� �����
	/// </summary>
	[SerializeField]
	[Tooltip("���� ���������� �������� �����")]
	public Transform pointJump;
	/// <summary>
	/// ���������� ���� ������
	/// </summary>
	[SerializeField]
	[Tooltip("���� ������")]
	public float jumpForce = 1;
	/// <summary>
	/// ������ ���������� �������� �����
	/// </summary>
	[SerializeField]
	[Tooltip("������ ���������� �������� �����")]
	public float radiusGroundCheck = 0.5f;
	public LayerMask layerGround;
	#endregion

	[Header("Gizmos")]
	#region GizmosSetting
	/// <summary>
	/// �������� �� ���������� �������� ����� ��� ����������
	/// </summary>
	[SerializeField]
	[Tooltip("�������� ���������� �������� ����� ��� ����������")]
	public bool isDraw;
	/// <summary>
	/// ���� ��������� ����������
	/// </summary>
	[SerializeField]
	[Tooltip("���� �������")]
	public Color colorDraw = Color.blue;
	#endregion

	// =====================================================================

	/// <summary>
	/// ������ �������� ���������
	/// </summary>
	public bool Stop { get; set; }

	/// <summary>
	/// �������� �� ������ ��������
	/// </summary>
	public bool isWalk { get; private set; }

	/// <summary>
	/// ������ �� ��������� �������� � ����
	/// </summary>
	public bool IsCloseToTarget { get; private set; }

	/// <summary>
	/// �������� �� �������� �����
	/// </summary>
	public bool isGround {
		get {
			return GetIsGround();
		}
	}

	// =====================================================================

	private void Awake() {
		// ��������� �����������
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();

		// ������� ��������� ������
		Stop = false;
		isWalk = false;
		IsCloseToTarget = false;
	}

	private void Start() {
		// ������ ���������� ���������� ���� 
		InvokeRepeating(nameof(PathBuilder), 0, timeUpdatePath);
	}

	private void OnDrawGizmos() {
		// �������� ������������� ���������
		if (!isDraw) return;

		Gizmos.color = colorDraw;

		// ��������� ����� �������� �����, ������ ���� ���� ������ ���� ��������
		if (pointJump != null) Gizmos.DrawWireSphere(pointJump.position, radiusGroundCheck);
	}

	/// <summary>
	/// ������������� ����
	/// </summary>
	private void PathBuilder() {
		// ���� ���������� ��������, ������ ���� �� �������� ������
		if (seeker.IsDone()) {
			seeker.StartPath(transform.position, Target.position, EndPath);
		}
	}

	/// <summary>
	/// �����, ������� ����������, ���� ���� �������� �������������
	/// </summary>
	/// <param name="p">����� ����</param>
	private void EndPath(Path p) {
		path = p;
		// ��� ��� ���� �������� ������� �����, � ���� ���������� ������, ������ ���������� 0
		indexPoint = 0;
	}

	/// <summary>
	/// �������� ������� ����� ��� ����������
	/// </summary>
	/// <returns>���� �� ����� ��� ����������</returns>
	private bool GetIsGround() {
		// ������ ����������� ��� ����������
		Collider2D[] colliders = Physics2D.OverlapCircleAll(pointJump.position, radiusGroundCheck, layerGround);

		// ���� ���� ���� ���� ��������, �������� ����� �� �����
		return colliders.Length > 0;
	}

	/// <summary>
	/// ������� ���������
	/// </summary>
	/// <param name="side">����������� �������� side > 0 - ������� �������, side < 0 - ������� ������, side == 0 - ��� ��������</param>
	private void Flip(float side) {
		if (side > 0) {
			if (transform.localScale.x < 0) {
				Vector3 temp = transform.localScale;
				temp.x *= -1;
				transform.localScale = temp;
			}
			return;
		}

		if (side < 0) {
			if (transform.localScale.x > 0) {
				Vector3 temp = transform.localScale;
				temp.x *= -1;
				transform.localScale = temp;
			}
			return;
		}
	}

	/// <summary>
	/// �������� ���������
	/// </summary>
	public void Move() {
		// �������� ���������� ����
		IsCloseToTarget = Vector3.Distance(transform.position, Target.position) <= distanceStop;

		// �������� ������������� ��������
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

		// ������ ������� ��������
		Vector3 vectorWay = path.vectorPath[indexPoint] - transform.position;
		// ������ ����������� ��������
		Vector3 sideWay = vectorWay.normalized;

		// ��������� ����������� ��������
		if (isDraw) Debug.DrawLine(transform.position, transform.position + vectorWay, colorDraw);

		// �������� ������������� ��������
		if (Mathf.Abs(sideWay.x) > flipSide && (isGround || !isCanJump)) {
			Flip(sideWay.x);
		}

		// �������� ������������� ������
		if (rb.gravityScale != 0 &&
			isCanJump &&
			isGround &&
			indexPoint + 1 < path.vectorPath.Count &&
			(path.vectorPath[indexPoint + 1] - path.vectorPath[indexPoint]).normalized.y >= jumpSide) {

			rb.Sleep();
			rb.AddForce(((Vector2)sideWay + Vector2.up) * jumpForce, ForceMode2D.Impulse);
		}

		// ���� ���������� ��������, �� �������� �� ��������� �� y
		if (rb.gravityScale != 0) nowSpeed.y = 0;

		// ���� �������� � �������, �� �������� ���������� �� ����������
		if (!isGround) sideWay *= speedMoveAir;

		// ������ ��������
		nowSpeed = Vector3.SmoothDamp(nowSpeed, sideWay * speed, ref accseleration, timeSmooth);
		// �������� ���������
		transform.position += nowSpeed * Time.fixedDeltaTime;

		// �������� ���������� ����� ��������
		if (vectorWay.sqrMagnitude <= distanceTouchPoint) {
			indexPoint += 1;
		}
	}

	private void FixedUpdate() {
		Move();
	}
}