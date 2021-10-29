using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Coin : MonoBehaviour
{
    public UnityEvent OnCoinUp;

	private void OnTriggerEnter2D(Collider2D collision) {
		OnCoinUp?.Invoke();
	}
}
