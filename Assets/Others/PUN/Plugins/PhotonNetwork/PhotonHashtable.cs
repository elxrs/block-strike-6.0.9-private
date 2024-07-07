using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class PhotonHashtable
{
	private struct Byte4
	{
		public byte b1;

		public byte b2;

		public byte b3;

		public byte b4;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct FloatBytesUnion
	{
		[FieldOffset(0)]
		public float f;

		[FieldOffset(0)]
		public Byte4 b4;
	}

	private BetterList<byte> keys = new BetterList<byte>();

	private BetterList<short> nums = new BetterList<short>();

	private BetterList<byte> values = new BetterList<byte>();

	private FloatBytesUnion floatConverter = default(FloatBytesUnion);

	private Byte4 floatBytes = default(Byte4);

	public int Count
	{
		get
		{
			return keys.size;
		}
	}

	public bool ContainsKey(byte key)
	{
		for (int i = 0; i < keys.size; i++)
		{
			if (keys.buffer[i] == key)
			{
				return true;
			}
		}
		return false;
	}

	public byte[] ToArray()
	{
		byte[] array = new byte[keys.size + nums.size * 2 + values.size + 2];
		array[0] = (byte)keys.size;
		int num = 1;
		short num2 = 0;
		for (int i = 0; i < keys.size; i++)
		{
			array[num] = keys.buffer[i];
			num++;
		}
		for (int j = 0; j < nums.size; j++)
		{
			num2 = nums.buffer[j];
			array[num] = (byte)(num2 >> 8);
			array[num + 1] = (byte)num2;
			num += 2;
		}
		for (int k = 0; k < values.size; k++)
		{
			array[num] = values.buffer[k];
			num++;
		}
		return array;
	}

	public void SetData(byte[] bytes)
	{
		Clear();
		int num = bytes[0];
		int num2 = 1;
		keys.Clear();
		for (int i = 0; i < num; i++)
		{
			keys.Add(bytes[num2]);
			num2++;
		}
		nums.Clear();
		for (int j = 0; j < num; j++)
		{
			nums.Add((short)((bytes[num2] << 8) | bytes[num2 + 1]));
			num2 += 2;
		}
		num = bytes.Length - num2;
		values.Clear();
		for (int k = 0; k < num; k++)
		{
			values.Add(bytes[num2]);
			num2++;
		}
	}

	public void Clear()
	{
		keys.Clear();
		nums.Clear();
		values.Clear();
	}

	public void Add(byte key, byte value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		values.Add(value);
	}

	public void Add(byte key, byte[] value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		short num = (short)value.Length;
		values.Add((byte)(num << 8));
		values.Add((byte)num);
		for (int i = 0; i < value.Length; i++)
		{
			values.Add(value[i]);
		}
	}

	public void Add(byte key, bool value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		values.Add((byte)(value ? 1u : 0u));
	}

	public void Add(byte key, int value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		values.Add((byte)(value >> 24));
		values.Add((byte)(value >> 16));
		values.Add((byte)(value >> 8));
		values.Add((byte)value);
	}

	public void Add(byte key, float value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		floatConverter.f = value;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
	}

	public void Add(byte key, short value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		values.Add((byte)(value >> 8));
		values.Add((byte)value);
	}

	public void Add(byte key, string value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		short num = (short)bytes.Length;
		values.Add((byte)(num << 8));
		values.Add((byte)num);
		for (int i = 0; i < bytes.Length; i++)
		{
			values.Add(bytes[i]);
		}
	}

	public void Add(byte key, Vector3 value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		floatConverter.f = value.x;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
		floatConverter.f = value.y;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
		floatConverter.f = value.z;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
	}

	public void Add(byte key, Quaternion value)
	{
		if (ContainsKey(key))
		{
			Debug.Log("Exists");
			return;
		}
		keys.Add(key);
		nums.Add((short)values.size);
		floatConverter.f = value.x;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
		floatConverter.f = value.y;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
		floatConverter.f = value.z;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
		floatConverter.f = value.w;
		values.Add(floatConverter.b4.b1);
		values.Add(floatConverter.b4.b2);
		values.Add(floatConverter.b4.b3);
		values.Add(floatConverter.b4.b4);
	}

	public byte GetByte(byte key)
	{
		return values.buffer[GetNum(key)];
	}

	public byte[] GetBytes(byte key)
	{
		int num = GetNum(key);
		short num2 = (short)((values.buffer[num] << 8) | values.buffer[num + 1]);
		byte[] array = new byte[num2];
		num += 2;
		for (int i = 0; i < num2; i++)
		{
			array[i] = values.buffer[num];
			num++;
		}
		return array;
	}

	public bool GetBool(byte key)
	{
		return values.buffer[GetNum(key)] != 0;
	}

	public int GetInt(byte key)
	{
		int num = GetNum(key);
		return (values.buffer[num] << 24) | (values.buffer[num + 1] << 16) | (values.buffer[num + 2] << 8) | values.buffer[num + 3];
	}

	public float GetFloat(byte key)
	{
		int num = GetNum(key);
		floatBytes.b1 = values.buffer[num];
		floatBytes.b2 = values.buffer[num + 1];
		floatBytes.b3 = values.buffer[num + 2];
		floatBytes.b4 = values.buffer[num + 3];
		floatConverter.b4 = floatBytes;
		return floatConverter.f;
	}

	public short GetShort(byte key)
	{
		int num = GetNum(key);
		return (short)((values.buffer[num] << 8) | values.buffer[num + 1]);
	}

	public string GetString(byte key)
	{
		int num = GetNum(key);
		short num2 = (short)((values.buffer[num] << 8) | values.buffer[num + 1]);
		byte[] array = new byte[num2];
		num += 2;
		for (int i = 0; i < num2; i++)
		{
			array[i] = values.buffer[num];
			num++;
		}
		return Encoding.UTF8.GetString(array);
	}

	public Vector3 GetVector3(byte key)
	{
		int num = GetNum(key);
		Vector3 result = default(Vector3);
		floatBytes.b1 = values.buffer[num];
		floatBytes.b2 = values.buffer[num + 1];
		floatBytes.b3 = values.buffer[num + 2];
		floatBytes.b4 = values.buffer[num + 3];
		floatConverter.b4 = floatBytes;
		result.x = floatConverter.f;
		floatBytes.b1 = values.buffer[num + 4];
		floatBytes.b2 = values.buffer[num + 5];
		floatBytes.b3 = values.buffer[num + 6];
		floatBytes.b4 = values.buffer[num + 7];
		floatConverter.b4 = floatBytes;
		result.y = floatConverter.f;
		floatBytes.b1 = values.buffer[num + 8];
		floatBytes.b2 = values.buffer[num + 9];
		floatBytes.b3 = values.buffer[num + 10];
		floatBytes.b4 = values.buffer[num + 11];
		floatConverter.b4 = floatBytes;
		result.z = floatConverter.f;
		return result;
	}

	public Quaternion GetQuaternion(byte key)
	{
		int num = GetNum(key);
		Quaternion result = default(Quaternion);
		floatBytes.b1 = values.buffer[num];
		floatBytes.b2 = values.buffer[num + 1];
		floatBytes.b3 = values.buffer[num + 2];
		floatBytes.b4 = values.buffer[num + 3];
		floatConverter.b4 = floatBytes;
		result.x = floatConverter.f;
		floatBytes.b1 = values.buffer[num + 4];
		floatBytes.b2 = values.buffer[num + 5];
		floatBytes.b3 = values.buffer[num + 6];
		floatBytes.b4 = values.buffer[num + 7];
		floatConverter.b4 = floatBytes;
		result.y = floatConverter.f;
		floatBytes.b1 = values.buffer[num + 8];
		floatBytes.b2 = values.buffer[num + 9];
		floatBytes.b3 = values.buffer[num + 10];
		floatBytes.b4 = values.buffer[num + 11];
		floatConverter.b4 = floatBytes;
		result.z = floatConverter.f;
		floatBytes.b1 = values.buffer[num + 12];
		floatBytes.b2 = values.buffer[num + 13];
		floatBytes.b3 = values.buffer[num + 14];
		floatBytes.b4 = values.buffer[num + 15];
		floatConverter.b4 = floatBytes;
		result.w = floatConverter.f;
		return result;
	}

	private int GetNum(byte key)
	{
		for (int i = 0; i < keys.size; i++)
		{
			if (keys.buffer[i] == key)
			{
				return nums.buffer[i];
			}
		}
		return 0;
	}
}
