using UnityEngine;
using System.Collections;
using System;

using NanoBuffers;

/*同步自己数据的字段*/
public struct SYN_SELF
{
	public const string POSTION = "selfpos";
	public const string SHOOT = "selfshoot";
	public const string HEALTH = "selfhealth";
	public const string FORCE_SHOOT = "selfForceShoot";
	public const string DEATH = "selfdeath";
	public const string LOCKED_TARGET = "selflockedtarget";
	public const string ALL_DATA = "selfalldata";
    public const string SPEED_UP = "selfspeedup";
    public const string STOP_MOVE = "selfstopmove";
    public const string DISCONNECTED = "selfdisconnected";
}

/*同步别人数据的字段*/
public struct SYN_OTHER
{
	public const string POSTION = "otherpos";
	public const string SHOOT = "othershoot";
	public const string HEALTH = "otherhealth";
	public const string FORCE_SHOOT = "otherForceShoot";
	public const string DEATH = "otherdeath";
	public const string LOCKED_TARGET = "otherlockedtarget";
	public const string ALL_DATA = "otheralldata";
    public const string SPEED_UP = "otherspeedup";
    public const string STOP_MOVE = "otherstopmove";
    public const string DISCONNECTED = "otherdisconnected";
}

/*自己的全量数据,方便同步用的*/
public struct TANK_DATA
{
    public byte index;
	public Vector2 position;
	public Color color;
	public float health;

    public TANK_DATA (byte pIndex, Vector2 pPositon, Color pColor, float pHealth) {
		index = pIndex;
		position = pPositon;
		color = pColor;
		health = pHealth;
	}
}

public class TankUtils 
{
	private const byte CMD_POSITION = 1;
	private const byte CMD_SHOOT = 2;
	private const byte CMD_HEALTH = 3;
	private const byte CMD_FORCE_SHOOT = 4;
	private const byte CMD_DEATH = 5;
	private const byte CMD_LOCKED_TARGET = 6;
	private const byte CMD_ALL_DATA = 7;
    private const byte CMD_SPEED_UP = 8;
    private const byte CMD_STOP_MOVE = 9;
    private const byte CMD_DISCONNECTED = 10;

	public static byte [] toBytes (Hashtable values) {
		NanoWriter serialize = new NanoWriter ();

		string name = (string) values ["name"];

		switch (name) {
		case SYN_SELF.POSTION:
                serialize.putInt (CMD_POSITION).putFloat ((float)values ["x"]).putFloat ((float)values ["z"]);
			break;
		case SYN_SELF.SHOOT:
			serialize.putInt (CMD_SHOOT).putInt ((int)values ["isshoot"]);
			break;
		case SYN_SELF.HEALTH:
			serialize.putInt(CMD_HEALTH).putFloat((float)values["health"]);
			break;
		case SYN_SELF.FORCE_SHOOT:
			serialize.putInt(CMD_FORCE_SHOOT).putFloat((float)values["force"]);
			break;
		case SYN_SELF.DEATH:
                serialize.putInt (CMD_DEATH).putInt((int)values["index"]);
			break;
		case SYN_SELF.LOCKED_TARGET:
			serialize.putInt (CMD_LOCKED_TARGET).putFloat ((float)values ["x"]).putFloat ((float)values["z"]);
			break;
		case SYN_SELF.ALL_DATA:
			serialize.putInt (CMD_ALL_DATA).putFloat ((float)values ["x"]).putFloat ((float)values ["z"]).put ((Color)values ["color"]).putFloat ((float)values ["health"]);
			break;
        case SYN_SELF.SPEED_UP:
                serialize.putInt(CMD_SPEED_UP).putFloat((float)values ["speed"]);
            break;
        case SYN_SELF.STOP_MOVE:
                serialize.putInt(CMD_STOP_MOVE).putInt((int)values["stopmove"]);
            break;
        case SYN_SELF.DISCONNECTED:
            serialize.putInt(CMD_DISCONNECTED).putInt((int)values["disconnected"]);
            break;
		} 

		return serialize.getBytes ();
	}

	public static Hashtable fromBytes (byte [] buf) {
		Hashtable values = new Hashtable ();
        NanoReader serialize = new NanoReader (buf);

		byte cmd;
		serialize.getInt (out cmd);

		switch (cmd) {
		case CMD_POSITION:
			{
                float x;
                float z;
                float ry;

                serialize.getFloat(out x).getFloat(out z);
				values.Add ("name", SYN_OTHER.POSTION);
				values.Add ("x", x);
				values.Add ("z", z);
			}
			break;
		case CMD_SHOOT:
			{
				int isShoot;

				serialize.getInt (out isShoot);
				values.Add ("name", SYN_OTHER.SHOOT);
				values.Add ("isshoot", isShoot);
			}
			break;
		case CMD_HEALTH:
			{
				float health;

				serialize.getFloat (out health);
				values.Add ("name", SYN_OTHER.HEALTH);
				values.Add ("health", health);
			}
			break;
		case CMD_FORCE_SHOOT:
			{
				float force;

				serialize.getFloat (out force);
				values.Add ("name", SYN_OTHER.FORCE_SHOOT);
				values.Add ("force", force);
			}
			break;
		case CMD_DEATH:
			{
                    int index;
                serialize.getInt(out index);
				values.Add ("name", SYN_OTHER.DEATH);
                values.Add("index", index);
			}
			break;
		case CMD_LOCKED_TARGET:
			{
				float x;
				float z;
				serialize.getFloat (out x).getFloat (out z);
				values.Add ("name", SYN_OTHER.LOCKED_TARGET);
				values.Add ("x", x);
				values.Add ("z", z);
			}
			break;
		case CMD_ALL_DATA:
			{
				float x;
				float z;
				Color c;
				float h;

				serialize.getFloat (out x).getFloat (out z).get (out c).getFloat (out h);
				values.Add ("name", SYN_OTHER.ALL_DATA);
				values.Add ("x", x);
				values.Add ("z", z);
				values.Add ("color", c);
				values.Add ("health", h);
			}
			break;
        case CMD_SPEED_UP:
            {
                float speed;

                serialize.getFloat(out speed);
                values.Add("name", SYN_OTHER.SPEED_UP);
                values.Add("speed", speed);
            }
            break;
        case CMD_STOP_MOVE:
            {
                int stopMove;
                serialize.getInt(out stopMove);
                values.Add("name", SYN_OTHER.STOP_MOVE);
                values.Add("stopmove", stopMove);
            }
            break;
        case CMD_DISCONNECTED:
			{
				int disconnected;
				serialize.getInt(out disconnected);
                values.Add("name", SYN_OTHER.DISCONNECTED);
				values.Add("disconnected", disconnected);
			}
			break;
		}

		return values;
	}
}

