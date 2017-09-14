using UnityEngine;

namespace Manager{
    public class CameraControl : MonoBehaviour
    {
		[HideInInspector] public Transform m_Target;

        [SerializeField] float m_DampTime = 1.0f;                 // Approximate time for the camera to refocus.
        [SerializeField] Vector3 m_initPosition;
		[SerializeField] LayerMask m_SlefTankLayerMask;

        private Camera m_Camera;                        // Used for referencing the camera.
        private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position.
        private Vector3 m_DesiredPosition;              // The position the camera is moving towards.

		public Transform m_testTarget;

        private void Awake ()
        {
            m_Camera = GetComponentInChildren<Camera> ();
        }

		void Update () 
		{

		}


        private void FixedUpdate ()
        {
			if (m_Target != null) 
				Move ();
        }
			

        private void Move ()
        {
			Vector3 lookAtPos = m_Target.position;
			transform.LookAt(lookAtPos);

            m_DesiredPosition = m_Target.position + m_initPosition;
			transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);

            SelfTurn(m_DesiredPosition - transform.position);
        }

		void SelfTurn(Vector3 direction)
		{
            Vector3 target_direction = direction.normalized;
			float angle = 90 - Mathf.Atan2(target_direction.z, target_direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.up), Time.deltaTime * 15f);
		}

		bool CheckTankIsInView () {
			Ray ray = Camera.main.ScreenPointToRay (m_testTarget.position);
			RaycastHit hit;
			bool isInView = Physics.Raycast (ray, out hit, Mathf.Infinity, m_SlefTankLayerMask);
			return isInView;
		}


	}
}