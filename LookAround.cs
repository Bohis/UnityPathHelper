using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAround : MonoBehaviour {
    public float radius;
    public Transform center;
    public LayerMask layers;

    public Color color;
    public bool isDraw;

    private void OnDrawGizmos() {
        if (isDraw && center) {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(center.position, radius);
        }
    }

    public void PickUp() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center.position, radius, layers);

        for (int i = 0; i < colliders.Length; ++i) {
            colliders[i].GetComponent<ReactionToAction>()?.React();
        }
    }

}