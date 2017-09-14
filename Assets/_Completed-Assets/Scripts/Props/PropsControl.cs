using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsControl : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 28){
			Destroy(gameObject);            
        }
    }
}
