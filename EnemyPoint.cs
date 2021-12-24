using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoint : MonoBehaviour {
    public List<Transform> poinst;

    private int indexPoint = 0;

    public float speed = 2;
    public float pointDelta = 2;
    public bool isPlayerNear = false;

    public void NextPoint() {
        indexPoint++;

        if (indexPoint >= poinst.Count) {
            indexPoint = 0;
        }
    }

    private void Patrul() {
        Vector3 nextVector = poinst[indexPoint].position - transform.position;

        transform.position += nextVector.normalized * speed * Time.fixedDeltaTime;

        if (nextVector.sqrMagnitude < pointDelta) {
            NextPoint();
        }
    }

    private void FixedUpdate() {
        if (isPlayerNear) {

            return;
        }

        Patrul();
    }
}