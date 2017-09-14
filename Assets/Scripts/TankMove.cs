using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMove : MonoBehaviour {

    public int m_Speed;
    public Transform m_BodyTransform;

	void Start () {
		
	}
	
	void Update () {
		
	}

    void Move () {
        Vector3 movement = m_BodyTransform.forward * Time.deltaTime * m_Speed;
        Vector3 taregetPos = transform.position + movement;

        transform.Translate(new Vector3(taregetPos.x , 0, taregetPos.z));
    }


}
