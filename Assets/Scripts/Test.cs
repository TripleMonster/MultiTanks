using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	void Start () {
		
	}

	void Update () {
		float dy = Input.GetAxis("Vertical");
		float dx = Input.GetAxis("Horizontal");

		if (Input.GetKey("up")) {
			transform.Translate(0, 0, dy);
		}

		if (Input.GetKey("down")) {
			transform.Translate(0, 0, dy);
		}

		if (Input.GetKey("left")) {
			transform.Translate(dx, 0, 0);
		}

		if (Input.GetKey("right")) {
			transform.Translate(dx, 0, 0);
		}
	}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);
    }
}
