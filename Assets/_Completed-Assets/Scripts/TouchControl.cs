using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControl : MonoBehaviour {

	public GameObject m_model;
	private Rigidbody m_RigiBody;
	float speed = 5f;

	void Start () {
		m_RigiBody = m_model.GetComponent<Rigidbody> ();
	}

	void Update () 
	{
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Moved) {
			Vector2 delta = Input.GetTouch (0).deltaPosition;

			if (delta.x != 0) {
				Vector3 turnDir;
				if (delta.x > 0) {
					turnDir = new Vector3 (-1, 0, -1);
					if (turnDir != Vector3.zero) {
						Vector3 player = (m_RigiBody.position + turnDir) - m_RigiBody.position;

						Quaternion newQuaternion = Quaternion.LookRotation (player);

						//m_model.transform.localRotation *= newQuaternion;
						m_RigiBody.MoveRotation(newQuaternion);
					}
				} else if (delta.x < 0) {
					turnDir = new Vector3 (1, 0, -1);
					if (turnDir != Vector3.zero) {
						Vector3 player = (m_RigiBody.position + turnDir) - m_RigiBody.position;

						Quaternion newQuaternion = Quaternion.LookRotation (player);

						//m_model.transform.localRotation *= newQuaternion;
						m_RigiBody.MoveRotation(newQuaternion);
					}
				}


			}
		}
	}
}
