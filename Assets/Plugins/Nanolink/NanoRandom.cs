using UnityEngine;

using System.Runtime.InteropServices;
using System;

namespace Nanolink {
	public class NanoRandom {
		private static long seed = 0;
		private static long id = 0;

		public static float Range (float v1, float v2, int i) {
			if (seed == 0) {
				if (NanoClient.getInt("client-index") == 0) {
					Int64.TryParse (NanoClient.getString("client-id"), out id);
				} else {
					Int64.TryParse (NanoClient.getString("target-id"), out id);
				}
			}

			long _seed = id + i*31;

			long v = (_seed * 1103515245U + 12345U) & 0x7fffffffU;

			return (((float)v/0x7fffffffU) * (v2-v1) + v1);
		}

		public static int Range (int v1, int v2, int i) {
			return (int) Range ((float)v1, (float)v2, i);
		}

		public static float Range (float v1, float v2) {
			if (seed == 0) {
				if (NanoClient.getInt("client-index") == 0) {
					Int64.TryParse (NanoClient.getString("client-id"), out id);
				} else {
					Int64.TryParse (NanoClient.getString("target-id"), out id);
				}
			}

			seed = (seed * 1103515245U + 12345U) & 0x7fffffffU;

			return (((float)seed/0x7fffffffU) * (v2-v1) + v1);
		}

		public static int Range (int v1, int v2) {
			return (int) Range ((float)v1, (float)v2);
		}

		public static long Range (long v1, long v2) {
			return (long) Range ((float)v1, (float)v2);
		}

		public static long Seed {
			get { 
				return seed; 
			}
			set { 
				seed = value; 
			}
		}

		public static Vector2 insideUnitCircle {
			get {
				return new Vector2 (Range (0.01f, 1.0f), Range (0.01f, 1.0f)).normalized * Range (0, 1.0f);
			}
		}

		public static Vector3 insideUnitSphere {
			get {
				return new Vector3 (Range (0.01f, 1.0f), Range (0.01f, 1.0f), Range (0.01f, 1.0f)).normalized * Range (0, 1.0f);
			}
		}

		public static Vector3 onUnitSphere {
			get {
				return new Vector3 (Range (0.01f, 1.0f), Range (0.01f, 1.0f), Range (0.01f, 1.0f)).normalized;
			}
		}
	}
}