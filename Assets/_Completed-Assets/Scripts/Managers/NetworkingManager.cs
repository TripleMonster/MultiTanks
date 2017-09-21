using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.Events;

using Nanolink;
using UnityEventUtils;

namespace Manager {
	public class CreateEvent : UnityEvent<TANK_DATA, bool> { }

	// 游戏联网服务
    public class NetworkingManager : NanoClient { 
		[HideInInspector] public CreateEvent m_CreateEvent = new CreateEvent ();   // 创建网络坦克的事件监听
        [HideInInspector] public UEvent_by_f_f m_NotifyEvent = new UEvent_by_f_f ();	 // 收到网络信息的事件监听
		[HideInInspector] public UEvent_by_f_f m_ShootEvent = new UEvent_by_f_f ();       // 收到射击动作的事件监听
		[HideInInspector] public UEvent_by_f_f m_HealthEvent = new UEvent_by_f_f ();    // 收到血量更新的事件监听
		[HideInInspector] public UnityEvent m_AllDataEvent = new UnityEvent();
        [HideInInspector] public UEvent_by m_ConnectedEvent = new UEvent_by();
        [HideInInspector] public UEvent_by_by m_DeathEvent = new UEvent_by_by();
		[HideInInspector] public UEvent_by_f_f m_LockedTargetEvent = new UEvent_by_f_f();
        [HideInInspector] public UEvent_by_f m_speedUpEvent = new UEvent_by_f();
        [HideInInspector] public UEvent_by m_stopMoveEvent = new UEvent_by();
        [HideInInspector] public UEvent_by m_disconnectedEvent = new UEvent_by();

        [HideInInspector] public bool[] m_notFirst = new bool[10];   // 记录所有连接到当前房间的id,  为了断线重连用的s


        private static NetworkingManager s_instance = null;
        private NetworkingManager() {}
        public static NetworkingManager getInstance()
        {
            if (s_instance == null) {
                s_instance = new NetworkingManager();
            }
            return s_instance;
        }

        public void init() {
            init("58e33562af2b4124", 3);
            config("time-machine", "true");
            config("debug-level", "7");
            connect(1);
        }

        public byte getClientIndex() {
            return (byte)getInt("client-index");
        }

        public void Disconnected() {
            m_disconnectedEvent.Invoke(getClientIndex());
            disconnect();
        }

        protected override void onStatusChanged(string newStatus, string oldStatus){
            
        }

		protected override void onConnected () 
		{
			base.onConnected ();

			status = "connected";

            m_ConnectedEvent.Invoke (getClientIndex());
		}

        protected override void onDisconnected (int error) {
			status = "disconnected";
            m_disconnectedEvent.Invoke(getClientIndex());
		}

        protected override void onResync (byte fromIndex) {
            m_AllDataEvent.Invoke ();
		}

        protected override void onMessage (byte[] data, byte fromIndex) { // onRemoteEvent
            byte index = fromIndex;
            Hashtable values = TankUtils.fromBytes(data);
			string name = (string)values ["name"];

			switch (name) {
			case SYN_OTHER.POSTION:
				{
                   m_NotifyEvent.Invoke (index, (float)values ["x"], (float)values ["z"]);	
				}
				break;
			case SYN_OTHER.SHOOT:
				{
					int isshoot = (int)values ["isshoot"];
					if (isshoot == 1) {
						m_ShootEvent.Invoke (index , 0, 0);
					}
				}
				break;
			case SYN_OTHER.HEALTH:
				{
					float health = (float)values ["health"];
					m_HealthEvent.Invoke (index, health, 0);
				}
				break;
			case SYN_OTHER.DEATH:
				{
                    if(getInt("is-master") == 1) 
                    {
                        if (!m_notFirst[index])
                            return;
						m_notFirst[index] = false;
						int killerIndex = (int)values["index"];
						m_DeathEvent.Invoke(index, (byte)killerIndex);
                    }
				}
				break; 
			case SYN_OTHER.LOCKED_TARGET:
				{
					m_LockedTargetEvent.Invoke (index, (float)values["x"], (float)values["z"]);
				}
				break;
			case SYN_OTHER.ALL_DATA:
				{
                        // 同步全量数据, 如果该index在本地已经存储了,就更新本地数据, 否则就新创建
					TANK_DATA tankData = new TANK_DATA (index, new Vector2((float)values ["x"], (float)values ["z"]), (Color)values ["color"], (float)values["health"]);
					m_CreateEvent.Invoke (tankData, m_notFirst[index]);
					m_notFirst [index] = true;
				}
				break;
            case SYN_OTHER.SPEED_UP:
                {
                    float speed = (float)values["speed"];
                    m_speedUpEvent.Invoke(index, speed);     
                }
                break;
            case SYN_OTHER.STOP_MOVE:
                {
                    m_stopMoveEvent.Invoke(index);
                }
                break;
            case SYN_OTHER.DISCONNECTED:
                {
                    m_disconnectedEvent.Invoke(index);
                }
                break;
			}
		}

		// 本地玩家更新信息
		public void doSynLocalData (Hashtable values) {
            send(TankUtils.toBytes(values));
		}

		public void drawGUI()
		{
			// 回放不支持 stats 数据, 忽略
			if (getInt("mode") == 0)
				return;

			GUIStyle guiStyle = new GUIStyle();
            guiStyle.normal.textColor = Color.blue;
			guiStyle.fontSize = 27;

			// 延迟 us->ms
			float latency = getInt("latency") / 1000.0f;

			// 当前的状态
			string s = getStatus();
			if (s == "connected")
			{
				if (getInt("is-master") == 1)
					s += ", 主场";
				else
					s += ", 客场";
			}

			// 对方 ID (或房间号)
			string targetId = getString("target-id");
			if (targetId.Length > 8)
				targetId = targetId.Substring(targetId.Length - 8);

			// 当前玩家 ID
			string clientId = getString("client-id");
			if (clientId.Length > 8)
				clientId = clientId.Substring(clientId.Length - 8);

			// Stats 数据
			string stats = getString("stats");

			// Stats 数据映射字典
			Dictionary<string, string> dictionary = new Dictionary<string, string>();

			string[] strs = stats.Split('\n');
			foreach (var str in strs)
			{
				string[] keyValue = str.Split('=');
				if (keyValue.Length > 1)
				{
					dictionary.Add(keyValue[0], keyValue[1]);
				}
			}

			string sendTaskIndex = dictionary["send.task.index"];
			string sendTaskBytes = dictionary["send.task.bytes"];
			string sendTotalBytes = dictionary["send.total.bytes"];
			string recvTaskIndex = "-";
			string recvTaskBytes = "-";
			string recvTotalBytes = "-";

			string targetIdStr = "";

			// 根据连接模式区分输出信息
			if (getInt("mode") == 3)
			{
				targetIdStr = "房间: " + targetId;
			}
			else
			{
				targetIdStr = "对方: " + targetId;

				// 1 VS 1 延迟／2
				latency = latency / 2;

				recvTaskIndex = dictionary["recv.task.index"];
				recvTaskBytes = dictionary["recv.task.bytes"];
				recvTotalBytes = dictionary["recv.total.bytes"];
			}

            StringBuilder sb1 = new StringBuilder();
            sb1.Append("状态:");
            sb1.Append(s);
            GUI.Label(new Rect(10, 10, 200, 200), sb1.ToString(), guiStyle);

			StringBuilder sb2 = new StringBuilder();
			sb2.Append(targetIdStr);
			sb2.Append(",设备:");
            sb2.Append(clientId);
            GUI.Label(new Rect(10, 30, 200, 200), sb2.ToString(), guiStyle);

			// 延迟
            if (latency >= 0) {
				StringBuilder sb3 = new StringBuilder();
                sb3.Append("时延:");
                sb3.Append(latency);
                sb3.Append("ms");
                GUI.Label(new Rect(10, 50, 200, 200), sb3.ToString(), guiStyle);
            }

			StringBuilder sb4 = new StringBuilder();
            sb4.Append("发送:");
            sb4.Append(sendTaskIndex);
            sb4.Append("次/");
            sb4.Append(sendTaskBytes);
            sb4.Append("字节/");
            sb4.Append(sendTotalBytes);
            sb4.Append("字节, 接收:");
            sb4.Append(recvTaskIndex);
            sb4.Append("次/");
            sb4.Append(recvTotalBytes);
            sb4.Append("字节/");

            GUI.Label(new Rect(10, 70, 200, 200), sb4.ToString(), guiStyle);

		}
	}
}
