using UnityEngine;
using System.Collections;

public class UIMaskDependent : MonoBehaviour {

    public GameObject parent;
	void Update () {
        if (parent == null) { DestroyImmediate(gameObject); }
	}
}
