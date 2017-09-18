﻿﻿﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System;
using System.Text;

using Complete;
using UnityEventUtils;

namespace Manager {
	// 管理游戏中的所有坦克,  包括自己的和 网络上其它玩家的
	public class TanksManager : MonoBehaviour    
	{
		#region  -------------------------初始化
		[HideInInspector] public OtherTank[] m_OtherTanks;
		[HideInInspector] public SelfTank m_SelfTank;
		public GameObject m_Prefab;
		public GameObject m_GameJoystick;
		public GameObject m_GameShootButton;
		public GameObject m_GameSpeedupButtion;
		public Text m_SelfMessage;
		private bool m_isInited = false;
		private int m_TankCount = 10;
        private int[,] m_positionArray = new int[4, 2] {{125, -120}, {125, 25}, {30, -120}, {30, 27}};
		#endregion

		#region  -------------------------网络
        [HideInInspector] public UEvent_by m_OverEvent = new UEvent_by();
        [HideInInspector] public UnityEvent m_RelifeEvent = new UnityEvent();
		private NetworkingManager m_NetworkingManager;
        #endregion

        #region  -------------------------冷却
        public Image m_CoolingImage;
		private bool m_isCooling = false;
		private float m_CoolTime = 5.0f;
		private float m_Timer = 0;
		private WaitForSeconds m_ReliveWait = new WaitForSeconds (10f);
        #endregion

        #region  -------------------------测试
        public FollowControl m_followControl;
        [SerializeField] WorldMessageControl _worldMessageControl;
		#endregion

		void Start ()
		{
            m_NetworkingManager = NetworkingManager.getInstance();
            if (m_NetworkingManager != null){
				m_NetworkingManager.m_NotifyEvent.AddListener(SynOtherTankMove);
				m_NetworkingManager.m_CreateEvent.AddListener(createOtherTanks);
				m_NetworkingManager.m_ShootEvent.AddListener(SynOtherShoot);
				m_NetworkingManager.m_HealthEvent.AddListener(SynOtherHealth);
				m_NetworkingManager.m_AllDataEvent.AddListener(SynAllSynDataOfSelfTank);
				m_NetworkingManager.m_DeathEvent.AddListener(SynOtherDeath);
				m_NetworkingManager.m_LockedTargetEvent.AddListener(SynOtherLockedTarget); 
                m_NetworkingManager.m_speedUpEvent.AddListener(SynOtherSpeed);
                m_NetworkingManager.m_stopMoveEvent.AddListener(SynOtherStopMove);
            }

			m_OtherTanks = new OtherTank[m_TankCount];
		}

		void Update ()
		{
			if (m_isInited && m_isCooling) {
				m_Timer += Time.deltaTime;

				m_CoolingImage.fillAmount = (m_CoolTime - m_Timer) / m_CoolTime;
				if (m_Timer > m_CoolTime) {
					setShootButtonCooling (false);
					m_Timer = 0;
				}
			}
		}
			

		private IEnumerator Relive() {
	
			yield return m_ReliveWait; 

            createSelfTank (m_NetworkingManager.getClientIndex());
            m_followControl.SetColorCorrectionCurvesSaturation(1);
            m_RelifeEvent.Invoke();
		}

		public void setShootButtonCooling(bool isCooling) 
		{
			m_isCooling = isCooling;
			m_SelfMessage.text = isCooling ? "<color=#2D44B0FF>正在装配子弹....</color>" : "子弹装配完成";

            m_SelfTank.m_ShootButton.activated = !isCooling;
		}


		private Color getRandomColor (int seed) {
			Color color = new Color ();
            UnityEngine.Random.InitState (seed);
			color.r = UnityEngine.Random.value;
			color.g = UnityEngine.Random.value;
			color.b = UnityEngine.Random.value;
			return color;
		}

        public void createSelfTank (byte index)
		{
			m_SelfTank = new SelfTank ();
            float x = m_positionArray[index % 4, 0];
            float z = m_positionArray[index % 4, 1];
			Vector3 initPos = new Vector3 (x, 0f, z);
            Color initColor = getRandomColor(System.DateTime.Now.Millisecond);

			m_SelfTank.m_Instance = Instantiate (m_Prefab, initPos, m_Prefab.transform.rotation) as GameObject;
			m_SelfTank.m_Joystick = m_GameJoystick.GetComponent<ETCJoystick> ();
			m_SelfTank.m_ShootButton = m_GameShootButton.GetComponent<ETCButton> ();
			m_SelfTank.m_SpeedupButton = m_GameSpeedupButtion.GetComponent<ETCButton> ();

            StringBuilder sb = new StringBuilder();
            sb.Append("坦克");
            sb.Append(index);
            sb.Append("号");

            if (m_SelfTank.initTank (index, initColor, sb.ToString(), initPos)) {
                m_followControl.m_followTarget = m_SelfTank.m_Instance.transform;

				m_SelfTank.m_Movement.m_moveEvent.AddListener (SynSelfTankMove);
				m_SelfTank.m_Shooting.m_ShootEvent.AddListener (SynSelfShoot);
				m_SelfTank.m_Health.m_HealthEvent.AddListener (SynSelfHealth);
				m_SelfTank.m_Shooting.m_CoolingEvent.AddListener (setShootButtonCooling);
				m_SelfTank.m_Health.m_DeathEvent.AddListener (SynSelfDeath);
                m_SelfTank.m_Movement.m_speedEvent.AddListener(SynSelfSpeed);
                m_SelfTank.m_Movement.m_stopMoveEvent.AddListener(SynSelfStopMove);

				m_GameJoystick.SetActive (true);
				m_GameShootButton.SetActive (true);

				m_isInited = true;
				SynAllSynDataOfSelfTank ();
			}
		}

		public void SynAllSynDataOfSelfTank () {
			Hashtable table = new Hashtable ();
			table.Add ("name", SYN_SELF.ALL_DATA);
			table.Add ("x", m_SelfTank.m_Instance.transform.position.x);
			table.Add ("z", m_SelfTank.m_Instance.transform.position.z);
			table.Add ("color", m_SelfTank.m_TankColor);
			table.Add ("health", m_SelfTank.m_Health.getCurrentHealth());
			table.Add ("death", m_SelfTank.m_Health.getDeath());

			m_NetworkingManager.doSynLocalData (table);
		}
				
		public void createOtherTanks(TANK_DATA tankData,bool isUpdate) {
			// 如果isUpdate == true , 就更新本地其它坦克数据, 否则创建新坦克  
			if (isUpdate) {
				updateLocalDataByIndex (tankData);
				return;
			}
				
			OtherTank otherTank = new OtherTank ();
			otherTank.m_Instance = Instantiate (m_Prefab, new Vector3(tankData.position.x, 0f, tankData.position.y), m_Prefab.transform.rotation) as GameObject;

			StringBuilder sb = new StringBuilder();
			sb.Append("坦克");
			sb.Append(tankData.index);
			sb.Append("号");

            if(otherTank.initTank (tankData.index, tankData.color, sb.ToString(), new Vector3(tankData.position.x, 0f, tankData.position.y), tankData.health) ){
				m_OtherTanks [tankData.index] = otherTank;
			}
		}

		private void updateLocalDataByIndex (TANK_DATA tankData) {
			OtherTank tank = m_OtherTanks [tankData.index];

			if (tank.m_Instance != null && tank.m_Movement != null && tank.m_Health != null) {
				tank.m_Instance.transform.position = new Vector3 (tankData.position.x, 0, tankData.position.y);
				tank.m_TankColor = tankData.color;
				tank.m_Health.SetHealthUI (tankData.health);
				tank.EnableControl ();
			}
		}
			
		/*
		 * 同步位置
		 */
		public void SynSelfTankMove() {
            float x = m_SelfTank.m_Instance.transform.position.x;
            float z = m_SelfTank.m_Instance.transform.position.z;

			Hashtable table = new Hashtable ();
			table.Add ("name", SYN_SELF.POSTION);
            table.Add ("x", x);
            table.Add ("z", z);
			m_NetworkingManager.doSynLocalData (table);
		}


        public void SynOtherTankMove(byte index, float x, float z){
			OtherTank tank = m_OtherTanks [index];

			if (tank.m_Movement != null) {
                tank.m_Movement.OnOtherMove(new Vector2(x, z));
			}
		}

		/*
		 * 同步普通射击
		 */
		public void SynSelfShoot(){
			if (m_isInited && !m_isCooling)
			{
				m_SelfMessage.text = string.Format("当前剩余子弹数:<color=#00FF01FF>{0}</color>", m_SelfTank.m_Shooting.GetCurrentlyHavedShootTimes());
			}

			Hashtable table = new Hashtable ();
			table.Add ("name", SYN_SELF.SHOOT);
			table.Add ("isshoot", 1);
			m_NetworkingManager.doSynLocalData (table);
		}

        public void SynOtherShoot(byte index, float param1, float param2){
			OtherTank tank = m_OtherTanks [index];

			if (tank.m_Shooting != null) {
				tank.m_Shooting.OnUp ();
			}
		}

		/*
		 * 同步血量
		 */
		public void SynSelfHealth(float health) {
			Hashtable table = new Hashtable ();
			table.Add ("name", SYN_SELF.HEALTH);
			table.Add ("health", health);
			m_NetworkingManager.doSynLocalData (table);
		}

        public void SynOtherHealth(byte index, float health, float param) {
			OtherTank tank = m_OtherTanks [index];

			if (tank.m_Health != null) 
			{
				tank.m_Health.SynOtherHealth (health);
			}
		}

		/*
		 * 同步死亡讯息  index是杀死你的人
		*/
        public void SynSelfDeath(byte index) {
            m_OverEvent.Invoke(index);
            m_followControl.SetColorCorrectionCurvesSaturation(0);

			Hashtable table = new Hashtable ();
			table.Add ("name", SYN_SELF.DEATH);
            table.Add ("index", (int)index);
			m_NetworkingManager.doSynLocalData (table);

			m_SelfTank.DisbleControl ();
			StartCoroutine (Relive());
		}

        public void SynOtherDeath(byte index, byte killerIndex) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<color=#2D44B0FF>坦克");
            sb.Append(killerIndex);
            sb.Append("号</color>干掉了<color=#FF00B0FF>坦克");
            sb.Append(index);
            sb.Append("号</color>");
            _worldMessageControl.setContent(sb.ToString());

			OtherTank tank = m_OtherTanks [index];
			if (tank.m_Health != null) 
			{
                tank.m_Health.TakeDamage(index, 0);
			}
		}

        public void SynSelfSpeed (float speed) {
            Hashtable table = new Hashtable();
            table.Add("name", SYN_SELF.SPEED_UP);
            table.Add("speed", speed);
            m_NetworkingManager.doSynLocalData(table);
        }

        public void SynOtherSpeed (byte index, float speed) {
            OtherTank tank = m_OtherTanks[index];

            if (tank.m_Movement != null) 
            {
                tank.m_Movement.setSpeed(speed);
            }
        }

        public void SynSelfStopMove () {
            Hashtable table = new Hashtable();
            table.Add("name", SYN_SELF.STOP_MOVE);
            table.Add("stopmove", 1);
            m_NetworkingManager.doSynLocalData(table);
        }

        public void SynOtherStopMove (byte index) {
            OtherTank tank = m_OtherTanks[index];

            if (tank.m_Movement != null)
            {
                tank.m_Movement.OnMoveEnd();
            }
        }

		public void SynSelfLockedTarget(float x, float z) {
			Hashtable table = new Hashtable ();
			table.Add ("name", SYN_SELF.LOCKED_TARGET);
			table.Add ("x", x);
			table.Add ("z", z);
			m_NetworkingManager.doSynLocalData (table);
		}

        public void SynOtherLockedTarget(byte index, float x, float z) {
			OtherTank tank = m_OtherTanks [index];

			if (tank.m_Shooting != null) {
				Vector3 new_pos = new Vector3 (x, 0, z);
				Rigidbody new_rigi = new Rigidbody ();
				new_rigi.position = new_pos;
			}
		}
			
	}
}

