using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEventUtils;

namespace Complete {

	public class TankInputModule : MonoBehaviour {
		
		public LayerMask m_MapLayerMask;
		public LayerMask m_TankLayerMask;

		private TankShooting m_Shooting;
		public TankShooting shooting
		{
			set { m_Shooting = value; }
			get { return shooting; }
		}
		private TankMovement m_Movement;
		public TankMovement movement
		{
			set {m_Movement = value; }
			get {return movement; }
		}
			
		private float m_FireRange = 50.0f;
		private Rigidbody m_TouchTarget;


        [HideInInspector] public UEvent_f_f m_FireTargetEvent = new UEvent_f_f();

		public bool m_IsDebug;

		void Start () {
			m_TouchTarget = null;
		}

		void Update () {
			if (m_IsDebug) {
				DoMouseRotation ();
			} else {
				DoTouchRotation ();
			}

			if (m_TouchTarget != null && CheckIsInFireRange (m_TouchTarget.position)) {
				SetRotationDirection (m_TouchTarget.position);
			}
		}

		void DoMouseRotation () {
			if (Input.mousePresent) {

				if (Input.GetMouseButton (0)) {
					Ray mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast (mouseRay, out hit, Mathf.Infinity, m_MapLayerMask)) {
						
						//SetRotationDirection (hit.point);
					}

					if (Physics.Raycast (mouseRay, out hit, Mathf.Infinity, m_TankLayerMask)) {
						m_TouchTarget = hit.rigidbody;
					}
				}
			}
		}

		void DoTouchRotation () {
			Touch touch;
			if (Input.touchCount == 1) {
				touch = Input.GetTouch (0);
			} else if (Input.touchCount == 2) {
				touch = Input.GetTouch (1);
			} else {
				return;
			}

			switch (touch.phase) {
			case TouchPhase.Began:
				Ray touchRay = Camera.main.ScreenPointToRay (touch.position);
				RaycastHit hit;
				if (Physics.Raycast (touchRay, out hit, Mathf.Infinity, m_MapLayerMask)) {
					
					//SetRotationDirection (hit.point);
				}

				if (Physics.Raycast (touchRay, out hit, Mathf.Infinity, m_TankLayerMask)) {
					m_TouchTarget = hit.rigidbody;
				}
				break;
			case TouchPhase.Moved:
				break;
			case TouchPhase.Ended:
				break;
			}
		}

		void SetRotationDirection (Vector3 target_pos) {
			m_Shooting.SetRotationDirection (target_pos);
		}

		bool CheckIsInFireRange (Vector3 targetPos) {
            float distance = 0;//Vector3.Magnitude (targetPos - m_Shooting.m_TurretTransform.position);

			bool isRange = distance < m_FireRange ? true : false;
			if (!isRange) {
				m_TouchTarget = null;
			}
			
			
			if (m_TouchTarget != null) {
				m_FireTargetEvent.Invoke (m_TouchTarget.position.x, m_TouchTarget.position.z);
			}

			return isRange;
		}
	}
}

