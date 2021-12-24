using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionDoor : ReactionToAction {
    public override void React() {
        Debug.Log(gameObject.name);
    }
}