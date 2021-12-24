using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// Скрипт движения npc
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class PathBuildHelper : MonoBehaviour {
	/// <summary>
	/// Строитель пути
	/// </summary>
	private Seeker seeker;

	/// <summary>
	/// Путь
	/// </summary>
	private Path path;

	/// <summary>
	/// Индекс точки, к которой нужно идти
	/// </summary>
	private int indexPoint = 0;

	/// <summary>
	/// Rigidbody2D персонажа
	/// </summary>
	private Rigidbody2D rb;

	/// <summary>
	/// Скорость в данный момент
	/// </summary>
	private Vector3 nowSpeed;

	/// <summary>
	/// Ускорение в данный момент
	/// </summary>
	private Vector3 accseleration;

	// =====================================================================

	[Header("Движение")]
	#region Move
	/// <summary>
	/// Максимальная скорость персонажа
	/// </summary>
	[Tooltip("Скорость")]
	[SerializeField]
	private float speed = 1;
	/// <summary>
	/// Коофициент снижения скорости движения персонажа в воздухе
	/// </summary>
	[SerializeField]
	[Tooltip("Снижение скорости движения персонажа в воздухе")]
	[Range(0, 1)]
	public float speedMoveAir = 0.5f;
	/// <summary>
	/// Цель движения
	/// </summary>
	[SerializeField]
	[Tooltip("Цель для пути")]
	public Transform Target;

	#endregion

	[Header("Поиск пути")]
	#region SearchPath
	/// <summary>
	/// Максимальная дистанция, на которую может подойти персонаж к своей цели
	/// </summary>
	[SerializeField]
	[Tooltip("Дистанция когда персонаж останавливается у цели")]
	public float distanceStop = 0.1f;
	/// <summary>
	/// Время перестройки пути
	/// </summary>
	[SerializeField]
	[Tooltip("Время для обновления пути")]
	[Range(0.3f, 1f)]
	private float timeUpdatePath = 0.5f;
	/// <summary>
	/// Допустимая погрешность в движении персонажа по пути
	/// </summary>
	[SerializeField]
	[Tooltip("Расстояние на которое персонаж может не дойти до точки пути")]
	public float distanceTouchPoint = 0.3f;
	#endregion

	[Header("Продвинутые настройки")]
	#region Setting
	/// <summary>
	/// Нормализованное значение вектора направления для разрешения прыжка (от позиции персонажа до следующей точки)
	/// </summary>
	[SerializeField]
	[Tooltip("Разрешенное направление прыжка прыжка (Нормализованное значение вектора направления для разрешения прыжка (от позиции персонажа до следующей точки))")]
	[Range(0, 1)]
	public float jumpSide = 0.5f;
	/// <summary>
	/// Время разгона и торможения персонажа в сек.
	/// </summary>
	[Tooltip("Ускорение персонажа")]
	[SerializeField]
	private float timeSmooth = 0.1f;
	/// <summary>
	/// Нормализованное значение вектора (по x) направления для разрешения поворота (от позиции персонажа до следующей точки)
	/// </summary>
	[SerializeField]
	[Tooltip("Разрешенное направление для поворота (Нормализованное значение вектора (по x) направления для разрешения поворота (от позиции персонажа до следующей точки))")]
	[Range(0, 1)]
	public float flipSide = 0.5f;
	#endregion

	[Header("Прыжок")]
	#region JumpSetting
	/// <summary>
	/// Разрешен ли прыжок
	/// </summary>
	[SerializeField]
	[Tooltip("Разрешение прыжка")]
	public bool isCanJump = true;
	/// <summary>
	/// Центр окружности, для проверки наличия земли
	/// </summary>
	[SerializeField]
	[Tooltip("Цетр окружности проверки земли")]
	public Transform pointJump;
	/// <summary>
	/// Коофициент силы прыжка
	/// </summary>
	[SerializeField]
	[Tooltip("Сила прыжка")]
	public float jumpForce = 1;
	/// <summary>
	/// Радиус окружности проверки земли
	/// </summary>
	[SerializeField]
	[Tooltip("Радиус окружности проверки земли")]
	public float radiusGroundCheck = 0.5f;
	public LayerMask layerGround;
	#endregion

	[Header("Gizmos")]
	#region GizmosSetting
	/// <summary>
	/// Рисовать ли окружность проверки земли под персонажем
	/// </summary>
	[SerializeField]
	[Tooltip("Рисовать окружность проверки земли под персонажем")]
	public bool isDraw;
	/// <summary>
	/// Цвет рисования окружности
	/// </summary>
	[SerializeField]
	[Tooltip("Цвет рисовки")]
	public Color colorDraw = Color.blue;
	#endregion

	// =====================================================================

	/// <summary>
	/// Запрет движения персонажа
	/// </summary>
	public bool Stop { get; set; }

	/// <summary>
	/// Движется ли сейчас персонаж
	/// </summary>
	public bool isWalk { get; private set; }

	/// <summary>
	/// Близко ли находится персонаж к цели
	/// </summary>
	public bool IsCloseToTarget { get; private set; }

	/// <summary>
	/// Касается ли персонаж земли
	/// </summary>
	public bool isGround {
		get {
			return GetIsGround();
		}
	}

	// =====================================================================

	private void Awake() {
		// Получение компонентов
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();

		// Задание начальных данных
		Stop = false;
		isWalk = false;
		IsCloseToTarget = false;
	}

	private void Start() {
		// Запуск повторение построения пути 
		InvokeRepeating(nameof(PathBuilder), 0, timeUpdatePath);
	}

	private void OnDrawGizmos() {
		// Проверка необходимости рисования
		if (!isDraw) return;

		Gizmos.color = colorDraw;

		// Рисование круга проверки земли, только если есть вокруг чего рисовать
		if (pointJump != null) Gizmos.DrawWireSphere(pointJump.position, radiusGroundCheck);
	}

	/// <summary>
	/// Строительство пути
	/// </summary>
	private void PathBuilder() {
		// Путь начинается строится, только если не строится сейчас
		if (seeker.IsDone()) {
			seeker.StartPath(transform.position, Target.position, EndPath);
		}
	}

	/// <summary>
	/// Метод, который вызывается, если путь закончил строительство
	/// </summary>
	/// <param name="p">Новый путь</param>
	private void EndPath(Path p) {
		path = p;
		// Так как путь задается списком точек, а путь построился заново, индекс становится 0
		indexPoint = 0;
	}

	/// <summary>
	/// Проверка наличия земли под персонажем
	/// </summary>
	/// <returns>Есть ли земля под персонажем</returns>
	private bool GetIsGround() {
		// Массив коллайдеров под персонажем
		Collider2D[] colliders = Physics2D.OverlapCircleAll(pointJump.position, radiusGroundCheck, layerGround);

		// Если есть хоть один колайдер, персонаж стоит на земле
		return colliders.Length > 0;
	}

	/// <summary>
	/// Поворот персонажа
	/// </summary>
	/// <param name="side">Направление поворота side > 0 - поворот направо, side < 0 - поворот налево, side == 0 - нет поворота</param>
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
	/// Движение персонажа
	/// </summary>
	public void Move() {
		// Проверка достижение цели
		IsCloseToTarget = Vector3.Distance(transform.position, Target.position) <= distanceStop;

		// Проверки необходимости движения
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

		// Расчет вектора движения
		Vector3 vectorWay = path.vectorPath[indexPoint] - transform.position;
		// Вектор направления движения
		Vector3 sideWay = vectorWay.normalized;

		// Рисование направления движения
		if (isDraw) Debug.DrawLine(transform.position, transform.position + vectorWay, colorDraw);

		// Проверка необходимости поворота
		if (Mathf.Abs(sideWay.x) > flipSide && (isGround || !isCanJump)) {
			Flip(sideWay.x);
		}

		// Проверка необходимости прыжка
		if (rb.gravityScale != 0 &&
			isCanJump &&
			isGround &&
			indexPoint + 1 < path.vectorPath.Count &&
			(path.vectorPath[indexPoint + 1] - path.vectorPath[indexPoint]).normalized.y >= jumpSide) {

			rb.Sleep();
			rb.AddForce(((Vector2)sideWay + Vector2.up) * jumpForce, ForceMode2D.Impulse);
		}

		// Если гравитация включена, то персонаж не двигается по y
		if (rb.gravityScale != 0) nowSpeed.y = 0;

		// Если персонаж в воздухе, то скорость умножается на коофициент
		if (!isGround) sideWay *= speedMoveAir;

		// Расчет скорости
		nowSpeed = Vector3.SmoothDamp(nowSpeed, sideWay * speed, ref accseleration, timeSmooth);
		// Движение персонажа
		transform.position += nowSpeed * Time.fixedDeltaTime;

		// Проверка достижения точки движения
		if (vectorWay.sqrMagnitude <= distanceTouchPoint) {
			indexPoint += 1;
		}
	}

	private void FixedUpdate() {
		Move();
	}
}