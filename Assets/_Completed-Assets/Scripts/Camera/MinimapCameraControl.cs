using UnityEngine;
using System.Collections;

public class MinimapCameraControl : MonoBehaviour
{
	private Vector3 m_LeftTop = new Vector3 (-16, 58, -27);
	private Vector3 m_RightTop = new Vector3 (16, 58, -27);
	private Vector3 m_LeftDown = new Vector3 (-31, 44, -50);
	private Vector3 m_RightDown = new Vector3 (31, 44, -50);
	private Camera m_Camera;                        // Used for referencing the camera.
	private Vector3 m_DesiredPosition;
	private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position.

	[HideInInspector] public Transform m_Target;
	public float m_Speed;
	public GameObject m_MiniMap;


	private void Awake ()
	{
		m_Camera = GetComponentInChildren<Camera> ();
        RenderTexture rt = m_Camera.targetTexture;
	}


	private void FixedUpdate ()
	{
		
	}

	private void FindAdjustPosition () {
		Vector3 tankPosition = m_Target.position;
		Vector3 forward = m_Target.forward;
		forward.Normalize ();
		transform.forward = forward;
		transform.LookAt (tankPosition);

		m_DesiredPosition = transform.forward * m_Speed * Time.deltaTime;
	}

	private bool IsCheckedToBoundary () {
		Vector3 curPos = transform.position;

		if (curPos.x <= m_LeftTop.x || curPos.z >= m_LeftTop.z)
			return false;

		if (curPos.x <= m_LeftDown.x || curPos.z <= m_LeftDown.z)
			return false;

		if (curPos.x >= m_RightTop.x || curPos.z >= m_RightTop.z)
			return false;

		if (curPos.x >= m_RightDown.x || curPos.z <= m_RightDown.z)
			return false;


		return true;
	}
}

