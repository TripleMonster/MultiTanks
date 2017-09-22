using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Complete
{
	public class TankBase
	{
		enum TankStatus {
			TANK_IDLE,
			TANKE_MOVING,
			TANK_SHOOTING,
			TANK_ROTATE_TURRET,
			TANK_LOCKED_TARGET,
			TANKE_TRACKING_TARGET,
		}

		public Color m_TankColor;
		public Transform m_SpawnPoint; 
		public string m_Name;

        [HideInInspector] public byte m_PlayerNumber;
		[HideInInspector] public GameObject m_Instance;
		[HideInInspector] public TankMovement m_Movement;
		[HideInInspector] public TankShooting m_Shooting;
		[HideInInspector] public TankHealth m_Health;

		private GameObject m_CanvasGameObject;

		public void Setup ()
		{
			m_Movement = m_Instance.GetComponent<TankMovement> ();
			m_Shooting = m_Instance.GetComponent<TankShooting> ();
			m_Health = m_Instance.GetComponent<TankHealth> ();

            m_Shooting.m_index = m_PlayerNumber;
            m_Health.m_index = m_PlayerNumber;

			m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas> ().gameObject;

			MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();
			for (int i = 0; i < renderers.Length; i++)
			{
                renderers[i].material.color = m_TankColor;
			}

            Text tankName  = m_CanvasGameObject.transform.Find("TankName").GetComponent<Text>();
            tankName.text = m_Name;
		}

        public virtual void EnableControl()
		{
			m_Movement.enabled = true;
			m_Shooting.enabled = true;
			m_Instance.gameObject.SetActive(true);
		}

        public virtual void DisbleControl()
		{
			m_Movement.enabled = false;
			m_Shooting.enabled = false;
		}
	}
}

