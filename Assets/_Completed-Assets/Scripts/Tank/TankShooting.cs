using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEventUtils;

namespace Complete
{
	public class TankShooting : MonoBehaviour
	{
        [SerializeField] GameObject m_Shell;                   // Prefab of the shell.
		[SerializeField] Transform m_FireTransform;           // A child of the tank where the shells are spawned.
		[SerializeField] AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
		[SerializeField] AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
		[SerializeField] AudioClip m_FireClip;                // Audio that plays when each shot is fired.
		[SerializeField] Transform m_TurretTransform;
		[SerializeField] float m_InitialVelocity;

		#region  fire
		private const uint m_TotalShootTimes = 10;    // 限制射击次数为10次一冷却
		private uint m_ShootTimes = 0;
		private float m_FireTimeInterval = 0;
		private Vector3 m_TargetPosition;
        public Transform m_FireTarget;
		
        List<Shell> shellList = new List<Shell>();     // 子弹列表
        int m_CurShellIndex;

        [HideInInspector] public byte m_index;
		#endregion

		[HideInInspector] public UnityEvent m_ShootEvent;
        [HideInInspector] public UEvent_bo m_CoolingEvent = new UEvent_bo ();

		private void OnEnable()
		{
			if (m_ShootEvent == null) {
				m_ShootEvent = new UnityEvent ();
			}
		}

		private void Start ()
		{

		}

        void AutoFire () {
			if (m_FireTarget != null)
			{
				m_FireTimeInterval += Time.deltaTime;
				if (m_FireTimeInterval > 1.0f)
				{
					SetRotationDirection(m_FireTarget.position);
					Fire();

					m_FireTimeInterval = 0;
				}
			}
        }
			
		private void Fire ()
		{
			if (m_FireTarget == null)
				return;

			GameObject shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as GameObject;
            ShellMove shellMove = shellInstance.GetComponent<ShellMove>();
            ShellExplosion shellExplosion = shellInstance.GetComponent<ShellExplosion>();
            shellExplosion.m_index = m_index;
            shellMove.SetIsFire(true);
            Vector3 newPos = new Vector3(m_FireTarget.position.x, m_FireTarget.position.y, m_FireTarget.position.z);
            shellMove.SetFirePosition(newPos);

            m_ShootingAudio.clip = m_FireClip;
			m_ShootingAudio.Play ();

			m_ShootEvent.Invoke ();
		}

		public void OnUp() {
			Fire ();
		}

		public void OnDown() {
			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play ();
		}

		public uint GetCurrentlyHavedShootTimes () {
			return m_TotalShootTimes - m_ShootTimes;
		}

		public void SetRotationDirection (Vector3 target_pos) {
			m_TargetPosition = target_pos;
			Vector3 target_direction = target_pos - m_TurretTransform.transform.position;
			float angle = 90 - Mathf.Atan2(target_direction.z, target_direction.x) * Mathf.Rad2Deg;
			//m_TurretTransform.transform.rotation = Quaternion.Lerp (m_TurretTransform.transform.rotation, Quaternion.AngleAxis (angle, Vector3.up), Time.deltaTime * 10.0f);
			m_TurretTransform.transform.rotation = Quaternion.AngleAxis (angle, Vector3.up);
		}
	}
}