using UnityEngine;
using System.Collections;

namespace Complete{
	public class OtherTank : TankBase
	{
        public bool initTank (byte playernum, Color color, string name, Vector3 pos, float health) {
			m_PlayerNumber = playernum;
			m_TankColor = color;
			m_Name = name;
			m_Instance.transform.position = pos;

			Setup ();

			m_Instance.layer = 28;

            m_Shooting.m_isSelf = false;
			m_Movement.m_isSelf = false;
			m_Health.m_isTeamMate = false;
			m_Health.SetHealthUI (health);
			EnableControl ();
			return true;
		}

		public void synMove(Vector2 pos) {
            m_Movement.OnMove (pos);
		}

		public void SetTeamMateByIndex (uint index, int selfIndex) {
			if ((index + selfIndex) % 2 == 0) 
			{
				m_Health.m_isTeamMate = true;
				return;
			} 
			m_Health.m_isTeamMate = false;
		}

		public override void DisbleControl()
		{
			base.DisbleControl();
		}

		public override void EnableControl()
		{
			base.EnableControl();
		}
	}
}


