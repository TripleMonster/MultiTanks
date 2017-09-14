using System;

using UnityEngine;

namespace NanoBuffers {
    // 支持 Varint 的简易序列化工具
	// 参考: https://bitbucket.org/Unity-Technologies/networking/src/78ca8544bbf4e87c310ce2a9a3fc33cdad2f9bb1/Runtime/NetworkWriter.cs?at=5.3&fileviewer=file-view-default
	// 
	// 特点: 变长度存储数值
	// 1, 无符号整数根据数值变长度编码: http://sqlite.org/src4/doc/trunk/www/varint.wiki
	// 2, 有符号整数转为无符号整数, 最低位用于表示符号, 参考 protobuf EncodeZigZag32, DecodeZigZag32
	// 3, float, double 可以设置保存精度(缺省为原始精度)
	//
	// 
	public class NanoWriter {
		private byte [] data;
		private int pos = 0;

		private void _putUInt8 (byte v) {
			resize (pos+1);
			data [pos] = v;
			pos++;
		}

		private void _putUInt32 (UInt32 v) {
			if (v <= 240) {
				_putUInt8 ((byte)v);
			} else if (v <= 2287) {
				_putUInt8 ((byte)((v - 240) / 256 + 241));
				_putUInt8 ((byte)((v - 240) % 256));
			} else if (v <= 67823) {
				_putUInt8 ((byte)249);
				_putUInt8 ((byte)((v - 2288) / 256));
				_putUInt8 ((byte)((v - 2288) % 256));
			} else if (v <= 16777215) {
				_putUInt8 ((byte)250);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
			} else {
				_putUInt8 ((byte)251);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
				_putUInt8 ((byte)((v >> 24) & 0xFF));
			}
		}

		private void _putInt32 (Int32 v) {
			_putUInt32 ((UInt32)((v << 1) ^ (v >> 31)));
		}

		private void _putUInt64 (UInt64 v) {
			if (v <= 240) {
				_putUInt8 ((byte)v);
			} else if (v <= 2287) {
				_putUInt8 ((byte)((v - 240) / 256 + 241));
				_putUInt8 ((byte)((v - 240) % 256));
			} else if (v <= 67823) {
				_putUInt8 ((byte)249);
				_putUInt8 ((byte)((v - 2288) / 256));
				_putUInt8 ((byte)((v - 2288) % 256));
			} else if (v <= 16777215) {
				_putUInt8 ((byte)250);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
			} else if (v <= 4294967295) {
				_putUInt8 ((byte)251);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
				_putUInt8 ((byte)((v >> 24) & 0xFF));
			} else if (v <= 1099511627775) {
				_putUInt8 ((byte)252);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
				_putUInt8 ((byte)((v >> 24) & 0xFF));
				_putUInt8 ((byte)((v >> 32) & 0xFF));
			} else if (v <= 281474976710655) {
				_putUInt8 ((byte)253);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
				_putUInt8 ((byte)((v >> 24) & 0xFF));
				_putUInt8 ((byte)((v >> 32) & 0xFF));
				_putUInt8 ((byte)((v >> 40) & 0xFF));
			} else if (v <= 72057594037927935) {
				_putUInt8 ((byte)254);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
				_putUInt8 ((byte)((v >> 24) & 0xFF));
				_putUInt8 ((byte)((v >> 32) & 0xFF));
				_putUInt8 ((byte)((v >> 40) & 0xFF));
				_putUInt8 ((byte)((v >> 48) & 0xFF));
			} else {
				_putUInt8 ((byte)255);
				_putUInt8 ((byte)(v & 0xFF));
				_putUInt8 ((byte)((v >> 8) & 0xFF));
				_putUInt8 ((byte)((v >> 16) & 0xFF));
				_putUInt8 ((byte)((v >> 24) & 0xFF));
				_putUInt8 ((byte)((v >> 32) & 0xFF));
				_putUInt8 ((byte)((v >> 40) & 0xFF));
				_putUInt8 ((byte)((v >> 48) & 0xFF));
				_putUInt8 ((byte)((v >> 56) & 0xFF));
			}
		}

		private void _putInt64 (Int64 v) {
			_putUInt64 ((UInt64)((v << 1) ^ (v >> 63)));
		}

		private void _putBytes (byte [] v) {
			resize (pos+v.Length);

			v.CopyTo (data, pos);
			pos += v.Length;
		}

		private int resize (int n) {
			if (n > data.Length) {
				if (n < data.Length * 2)
					n = data.Length * 2;
				byte[] newData = new byte [n];

				data.CopyTo (newData, 0);

				data = newData;
			}

			return data.Length;
		}

		private void fit () {
			if (pos == 0) {
				data = null;
				return;
			}

			byte[] newData = new byte [pos];

			for (int i = 0; i < pos; i ++)
				newData [i] = data [i];
			//data.CopyTo (newData, 0);

			data = newData;
		}

		public NanoWriter (int n = 0) {
			n = Math.Max (n, 64);

			data = new byte [n];
			pos = 0;
		}

		public int getBytesLeft () {
			return data.Length - pos;
		}

		public int getLength () {
			return pos;
		}

		public byte [] getBytes () {
			fit ();
			return data;
		}

		public NanoWriter putInt (char v) {
			_putInt64 (v);
			return this;
		}

		public NanoWriter putInt (byte v) {
			_putUInt64 (v);
			return this;
		}

		public NanoWriter putInt (Int16 v) {
			_putInt64 (v);
			return this;
		}

		public NanoWriter putInt (UInt16 v) {
			_putUInt64 (v);
			return this;
		}

		public NanoWriter putInt (Int32 v) {
			_putInt64 (v);
			return this;
		}

		public NanoWriter putInt (UInt32 v) {
			_putUInt64 (v);
			return this;
		}

		public NanoWriter putInt (Int64 v) {
			_putInt64 (v);
			return this;
		}

		public NanoWriter putInt (UInt64 v) {
			_putUInt64 (v);
			return this;
		}

		public NanoWriter putString (string v) {
			byte [] v2 = System.Text.Encoding.UTF8.GetBytes (v);

			_putUInt32 ((UInt32)v2.Length);
			_putBytes (v2);

			return this;
		}

		public NanoWriter putFloat (float v, int precision = -1) {
			if (precision < 0)
				_putBytes (BitConverter.GetBytes(v));
			else
				_putInt32 ((Int32)((v*Math.Pow (10, precision+1) + 5)/10));
			return this;
		}

		public NanoWriter putDouble (double v, int precision = -1) {
			if (precision < 0)
				_putBytes (BitConverter.GetBytes(v));
			else
				_putInt64 ((Int64)((v*Math.Pow (10, precision)+5)/10));
			return this;
		}

		public NanoWriter put (Vector2 v, int precision = -1) {
			return putFloat (v.x, precision).putFloat (v.y, precision);
		}

		public NanoWriter put (Vector3 v, int precision = -1) {
			return putFloat (v.x, precision).putFloat (v.y, precision).putFloat (v.z, precision);
		}

		public NanoWriter put (Vector4 v, int precision = -1) {
			return putFloat (v.x, precision).putFloat (v.y, precision).putFloat (v.z, precision).putFloat (v.w, precision);
		}

		public NanoWriter put (Quaternion v, int precision = -1) {
			return putFloat (v.x, precision).putFloat (v.y, precision).putFloat (v.z, precision).putFloat (v.w, precision);
		}

		public NanoWriter put (Color v, int precision = -1) {
			return putFloat (v.r, precision).putFloat (v.g, precision).putFloat (v.b, precision).putFloat (v.a, precision);
		}

		public NanoWriter put (Color32 v) {
			return putInt (v.r).putInt (v.g).putInt (v.b).putInt (v.a);
		}
	}

	public class NanoReader {
		private byte [] data;
		private int pos = 0;

		private byte _getUInt8 () {
			return data [pos++];
		}

		private UInt64 _getUInt64 () {
			byte a0 = _getUInt8 ();
			if (a0 < 241) {
				return a0;
			}

			byte a1 = _getUInt8 ();
			if (a0 >= 241 && a0 <= 248) {
				return (UInt32)(240 + 256 * (a0 - 241) + a1);
			}

			byte a2 = _getUInt8 ();
			if (a0 == 249) {
				return (UInt32)(2288 + 256 * a1 + a2);
			}

			byte a3 = _getUInt8 ();
			if (a0 == 250) {
				return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16);
			}

			byte a4 = _getUInt8 ();
			if (a0 == 251) {
				return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16) + (((UInt32)a4) << 24);
			}

			byte a5 = _getUInt8 ();
			if (a0 == 252) {
				return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
			}

			byte a6 = _getUInt8 ();
			if (a0 == 253) {
				return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
			}

			byte a7 = _getUInt8 ();
			if (a0 == 254) {
				return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
			}

			byte a8 = _getUInt8 ();
			if (a0 == 255) {
				return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48)  + (((UInt64)a8) << 56);
			}

			throw new IndexOutOfRangeException("getUInt64 () failure: " + a0);
		}

		private Int64 _getInt64() {
			UInt64 v = _getUInt64 ();

			return (Int64) ((long)(v >> 1) ^ -(long)(v & 1));
		}

		private byte [] _getBytes (int n) {
			byte [] buf = new byte [n];

			for (int i = 0; i < n; i++) {
				buf [i] = data [pos + i];
			}
			pos += n;

			return buf;
		}

		public NanoReader (byte [] buf) {
			data = new byte[buf.Length];

			buf.CopyTo (data, 0);
			pos = 0;
		}

		public int getBytesLeft () {
			return data.Length - pos;
		}

		public int getLength () {
			return pos;
		}

		public byte [] getBytes () {
			return data;
		}

		public NanoReader getInt (out byte v) {
			v = (byte) _getUInt64 ();

			return this;
		}

		public NanoReader getInt (out short v) {
			v = (Int16) _getInt64 ();

			return this;
		}

		public NanoReader getInt (out ushort v) {
			v = (UInt16) _getUInt64 ();

			return this;
		}

		public NanoReader getInt (out Int32 v) {
			v = (Int32) _getInt64 ();

			return this;
		}

		public NanoReader getInt (out UInt32 v) {
			v = (UInt32) _getUInt64 ();

			return this;
		}

		public NanoReader getInt (out Int64 v) {
			v = _getInt64 ();

			return this;
		}

		public NanoReader getInt (out UInt64 v) {
			v = _getUInt64 ();

			return this;
		}

		public NanoReader getString (out string v) {
			UInt32 len = (UInt32) _getUInt64 ();

			v = System.Text.Encoding.UTF8.GetString (_getBytes ((Int32)len));

			return this;
		}

		public NanoReader getFloat (out float v, int precision = -1) {
			if (precision < 0)
				v =  BitConverter.ToSingle (_getBytes (sizeof (float)), 0);
			else
				v =  ((float) _getInt64 () / Mathf.Pow (10.0f, precision));

			return this;
		}

		public NanoReader getDouble (out double v, int precision = -1) {
			if (precision < 0)
				v =  BitConverter.ToDouble (_getBytes (sizeof (double)), 0);
			else
				v =  ((double) _getInt64 () / Math.Pow (10.0, precision));

			return this;
		}

		public NanoReader get (out Vector2 v, int precision = -1) {
			v = new Vector2 ();

			return getFloat (out v.x, precision).getFloat (out v.y, precision);
		}

		public NanoReader get (out Vector3 v, int precision = -1) {
			v = new Vector3 ();

			return getFloat (out v.x, precision).getFloat (out v.y, precision).getFloat (out v.z, precision);
		}

		public NanoReader get (out Vector4 v, int precision = -1) {
			v = new Vector3 ();

			return getFloat (out v.x, precision).getFloat (out v.y, precision).getFloat (out v.z, precision).getFloat (out v.w, precision);
		}

		public NanoReader get (out Quaternion v, int precision = -1) {
			v = new Quaternion ();

			return getFloat (out v.x, precision).getFloat (out v.y, precision).getFloat (out v.z, precision).getFloat (out v.w, precision);
		}

		public NanoReader get (out Color v, int precision = -1) {
			v = new Color ();

			return getFloat (out v.r, precision).getFloat (out v.g, precision).getFloat (out v.b, precision).getFloat (out v.a, precision);
		}

		public NanoReader get (out Color32 v) {
			v = new Color32 ();

			return getInt (out v.r).getInt (out v.g).getInt (out v.b).getInt (out v.a);
		}
	}
}

