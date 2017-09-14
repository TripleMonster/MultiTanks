using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellMove : MonoBehaviour {

    Rigidbody rigiBody;
    int m_speed = 10;   // 越大越快
    bool m_IsFire = false;
    [HideInInspector] public Vector3 m_TargetPos;

	void Start () {
        rigiBody = GetComponent<Rigidbody>();
	}
	
	void Update () {
        if (m_IsFire) {
            Move();   
        }
	}

    void Move () {
        Vector3 targetPos = new Vector3(m_TargetPos.x, rigiBody.position.y, m_TargetPos.z);
        rigiBody.position = Vector3.Lerp(rigiBody.position, m_TargetPos, Time.deltaTime * m_speed);
    }

    public void SetIsFire(bool isFire) {
        m_IsFire = isFire;
    }

	public void SetFirePosition(Vector3 pos)
	{
		m_TargetPos = pos;
	}
}
