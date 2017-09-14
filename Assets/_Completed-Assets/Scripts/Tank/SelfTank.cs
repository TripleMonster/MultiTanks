using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Complete
{
	public class SelfTank : TankBase
	{
		[HideInInspector] public ETCJoystick m_Joystick;
		[HideInInspector] public ETCButton m_ShootButton;
		[HideInInspector] public ETCButton m_SpeedupButton;

        public bool initTank(byte playernum, Color color, string name, Vector3 pos)
		{
			m_PlayerNumber = playernum;
			m_TankColor = color;
			m_Name = name;

			Setup ();

			//m_Instance.layer = 26;

			m_Movement.m_isSelf = true;
			m_Health.m_isTeamMate = true;
			initEasyTouch ();
			EnableControl ();

			return true;
		}
			
		private void initEasyTouch ()
		{
			if (m_Joystick != null) {
				m_Joystick.onMove.AddListener (m_Movement.OnMove);
				m_Joystick.onMoveEnd.AddListener (m_Movement.OnMoveEnd);
			}

			if (m_ShootButton != null) {
				m_ShootButton.onUp.AddListener (m_Shooting.OnUp);
				m_ShootButton.onDown.AddListener (m_Shooting.OnDown);
			} 

			if (m_SpeedupButton != null) {
				m_SpeedupButton.onPressed.AddListener (m_Movement.OnSpeedupPressed);
				m_SpeedupButton.onUp.AddListener (m_Movement.OnSpeedupUp);
			}
		}

		public void DisbleControl(){
			m_Movement.enabled = false;
			m_Shooting.enabled = false;
			m_Joystick.enabled = false;
			m_ShootButton.enabled = false;
		}

		public void EnableControl(){
			m_Movement.enabled = true;
			m_Shooting.enabled = true;
			m_Joystick.enabled = true;
			m_ShootButton.enabled = true;
		}
	}

}
