using UnityEngine;

using System.Runtime.InteropServices;
using System;

using System.Collections;
using System.Collections.Generic;

namespace Nanolink {
	public abstract class NanoClient {
		protected static string status;
		protected static bool disconnectedBySelf;
		protected static byte players;
		protected static int[] last_times = new int [64];

		public static int init(string appKey, int mode = 2) {
			disconnectedBySelf = false;

			players = 0;
			for (int i = 0; i < 64; i ++)
				last_times [i] = -1;

			return nano_init (appKey, mode);
		}

		public static int config(string name, string value) {
			return nano_config (name, value);
		}

		public static int connect(uint level, uint mode = 0, int timeout = 15000) {
			return nano_connect (level, mode, timeout);
		}

		public static int connect(string group, int timeout = 15000) {
			return nano_connect2 (group, timeout);
		}

		public static int disconnect() {
			disconnectedBySelf = true; // 主动断开
			return nano_disconnect ();
		}

		public static int send(byte[] data) {
			return nano_send (data, (uint)data.Length);
		}

		private static int _sendmark(uint mark = 0) {
			return nano_sendmark (mark);
		}

		// 接收数据
		// 建议用 doUpdate 配合 onMessage 中处理接收到的消息
		public static int recv(byte[] data, out byte playerIndex) {
			return nano_recv (data, (uint) data.Length, out playerIndex);
		}

		public static int load (string filePath) {
			return nano_load (filePath, -1);
		}

		public static int save (string filePath) {
			return nano_save (filePath, -1);
		}

		public static int getInt(string name, int n = -1) {
			return nano_getInt (name, n);
		}

		public static string getString(string name) {
			// 接收数据 buf， 转为 string 返回
			byte[] buf = new byte [4096];

			int len = nano_getString(name, buf, buf.Length);
			while (len > buf.Length) {
				buf = new byte [len];
				len = nano_getString(name, buf, len);
			}

			if(len < 0) {
				return "";
			}

			return System.Text.Encoding.UTF8.GetString (buf, 0, len);
		}

		public static string getStatus () {
			return getString ("status");
		}

		// 判断连接是否建立
		public static bool isConnected() {
			return (getStatus() == "connected");
		}

		public static bool isDisconnect () {
			return (getStatus() == "disconnect");
		}

		public virtual int doUpdate () {
			// 状态变化时, 调用相应状态变化函数
			if (status != getStatus()) {
				string newStatus = getStatus ();
				if (newStatus == "connected") {
					onConnected ();

					_sendmark (0);
					onResync ((byte) getInt ("client-index"));
				}
				if (newStatus == "disconnect")
					onDisconnected (getInt ("last-error"));

				onStatusChanged (getStatus(), status);

				status = newStatus;
			}

			// 还没有连接成功，忽略后续的接收处理
			if (!isConnected ())
				return -1;

			// 检查是否有人 "离开" 或 "回来"
			int timeout = 2000;
			for (byte i = 0; i < players; i ++) {
				int old_last_time = last_times [i];
				last_times [i] = getInt ("last-time", i);

				if (old_last_time < timeout &&  last_times [i] > timeout) {
					onPlayer ((byte)i, "left"); // 离开
				} else if (old_last_time > timeout &&  last_times [i] < timeout) {
					onPlayer ((byte)i, "return"); // 回来
				}
			}

			byte fromIndex;
			int buf_size = 1024;
			byte[] buf = new byte [buf_size];

			int count = 0;
			while (true) {
				int len = recv (buf, out fromIndex);

				// 如果空间不足, 重新分配
				if (len > buf_size) {
					buf_size = len;
					buf = new byte [buf_size];

					len = recv (buf, out fromIndex);
				}

				// 没有收到指令或数据, 退出
				if (len < 0)
					break;

				if (fromIndex >= players) {
					// 有可能 msg.fromIndex 一下子比 this.players 大不少
					for (int i = players; i < fromIndex+1; i ++) {
						last_times [i] = getInt ("last-time", i);

						onPlayer ((byte)i, "new");
					}
					players = (byte)(fromIndex+1);
				}

				if (len == 0) { // 收到同步指令
					_sendmark (0);
					onResync ((byte) fromIndex);
				} else if (len == 1) { // 收到标记指令
					// onMark (buf [0]);
				} else { // 收到数据
					byte [] newBuf = new byte[len];
					for (int i = 0; i < len; i++)
						newBuf [i] = buf [i];
					onMessage (newBuf, fromIndex);
				}

				count ++;
			}

			return count;
		}

		//===============================
		// 以下虚拟函数需要在开发者的代码中实现
		//===============================

		// 接收到一个事件
		protected virtual void onMessage (byte[] data, byte fromIndex) {
		}

		// 收到 resync 指令
		// 发送客户端的完整数据
		protected virtual void onResync (byte fromIndex) {
		}

		// 状态变化
		protected virtual void onStatusChanged (string newStatus, string oldStatus) {
		}

		// 连接建立
		protected virtual void onConnected () {
		}

		// 连接断开
		protected virtual void onDisconnected (int error) {
		}

		// 玩家新加入("new"), 离开("left"), 回来("return")
		protected virtual void onPlayer (byte clientIndex, string e) {
		}


		//===============================
		// SDK 接口导入, 不要修改 
		//===============================

		#if UNITY_IPHONE && !UNITY_EDITOR

		// 初始化
		// mode = 0: 用于回放
		// mode = 1: 单人, 仅用于记录
		// mode = 2: 1对1联网方式(缺省)
		// mode = 3: 多人联网方式
		[DllImport ("__Internal")] private static extern int nano_init (string appKey, int mode);

		// 设置协议参数
		[DllImport ("__Internal")] private static extern int nano_config (string name, string value);

		// 全网匹配接口, 如果 timeout <= 0，默认15秒
		// mode: 等级匹配可以分为不同模式, 支持 0-15 模式, 缺省为0
		// 		例如, mode=0, 代表经典模式, mode=1, 代表排位赛, ...
		[DllImport ("__Internal")] private static extern int nano_connect (uint level, uint mode, int timeout);

		// 局域网匹配接口, 同上
		// 如果 group 为空，局域网匹配
		[DllImport ("__Internal")] private static extern int nano_connect2 (string group, int timeout);

		// 断开连接
		[DllImport ("__Internal")] private static extern int nano_disconnect ();

		// 发送数据
		[DllImport ("__Internal")] private static extern int nano_send (byte[] data, uint size);

		// 发送一个协议"分隔符"标记
		// 传输协议收到sendmark指令会忽略收发队列中前面的数据
		[DllImport ("__Internal")] private static extern int nano_sendmark (uint mark);

		// 接收数据
		// 返回接收的长度, 如果接收缓冲区不够长, 返回下个数据的长度, 但数据还在接收队列中
		// XXX 如果返回值为0, 表明是同步指令, 接收到的客户端需要发送全量数据
		// XXX 如果返回值为1, 表明是标记指令
		// XXX 多人联网时, 中途进入的客户端SDK会自动发送同步指令
		[DllImport ("__Internal")] private static extern int nano_recv (byte[] data, uint size, out byte playerIndex);

		// 用于替代其他 get 接口, 分别有 字符串形式和long形式
		// 将来有可能取消其他 get 接口
		// getInt, getString 参数无效时，返回 0
		// 参数：latency, last-time, last-error, client-index, is-master, master-index
		[DllImport ("__Internal")] private static extern int nano_getInt (string name, int n);

		// 参数：server-id, target-id, client-id, status, stats
		[DllImport ("__Internal")] private static extern int nano_getString (string name, byte[] data, int size);

		// 获得与设置日志数据
		[DllImport ("__Internal")] private static extern int nano_load (string path, int size);
		[DllImport ("__Internal")] private static extern int nano_save (string path, int size);

		[DllImport ("__Internal")] private static extern int nano_load (byte[] data, int size);
		[DllImport ("__Internal")] private static extern int nano_save (byte[] data, int size);

		#else

		// 初始化
		// mode = 0: 用于回放
		// mode = 1: 单人, 仅用于记录
		// mode = 2: 1对1联网方式(缺省)
		// mode = 3: 多人联网方式
		[DllImport ("nanolink")] private static extern int nano_init (string appKey, int mode);

		// 设置协议参数
		[DllImport ("nanolink")] private static extern int nano_config (string name, string value);

		// 全网匹配接口, 如果 timeout <= 0，默认15秒
		// mode: 等级匹配可以分为不同模式, 支持 0-15 模式, 缺省为0
		// 		例如, mode=0, 代表经典模式, mode=1, 代表排位赛, ...
		[DllImport ("nanolink")] private static extern int nano_connect (uint level, uint mode, int timeout);

		// 局域网匹配接口, 同上
		// 如果 group 为空，局域网匹配
		[DllImport ("nanolink")] private static extern int nano_connect2 (string group, int timeout);

		// 断开连接
		[DllImport ("nanolink")] private static extern int nano_disconnect ();

		// 发送数据
		[DllImport ("nanolink")] private static extern int nano_send (byte[] data, uint size);

		// 发送一个协议"分隔符"标记
		// 传输协议收到sendmark指令会忽略收发队列中前面的数据
		[DllImport ("nanolink")] private static extern int nano_sendmark (uint mark);

		// 接收数据
		// 返回接收的长度, 如果接收缓冲区不够长, 返回下个数据的长度, 但数据还在接收队列中
		// XXX 如果返回值为0, 表明是同步指令, 接收到的客户端需要发送全量数据
		// XXX 如果返回值为1, 表明是标记指令
		// XXX 多人联网时, 中途进入的客户端SDK会自动发送同步指令
		[DllImport ("nanolink")] private static extern int nano_recv (byte[] data, uint size, out byte playerIndex);

		// 用于替代其他 get 接口, 分别有 字符串形式和long形式
		// 将来有可能取消其他 get 接口
		// getInt, getString 参数无效时，返回 0
		// 参数：latency, last-time, last-error, client-index, is-master, master-index
		[DllImport ("nanolink")] private static extern int nano_getInt (string name, int n);

		// 参数：server-id, target-id, client-id, status, stats
		[DllImport ("nanolink")] private static extern int nano_getString (string name, byte[] data, int size);

		// 获得与设置日志数据
		[DllImport ("nanolink")] private static extern int nano_load (string path, int size);
		[DllImport ("nanolink")] private static extern int nano_save (string path, int size);

		[DllImport ("nanolink")] private static extern int nano_load (byte[] data, int size);
		[DllImport ("nanolink")] private static extern int nano_save (byte[] data, int size);

		#endif
	}
}
