using System;
using UnityEngine;

[Serializable]
public struct CryptoBool : IEquatable<CryptoBool>
{
    [SerializeField]
    private byte cryptoKey;

    [SerializeField]
    private byte hiddenValue;

    [SerializeField]
    private bool fakeValue;

    [SerializeField]
    private bool fakeValueChanged;

    [SerializeField]
    private bool inited;

    private CryptoBool(bool value)
	{
		cryptoKey = (byte)CryptoManager.random.Next(1, 200);
		hiddenValue = (byte)(((!value) ? 32 : 18) ^ cryptoKey);
		fakeValue = (CryptoManager.fakeValue && value);
		fakeValueChanged = false;
		inited = true;
#if UNITY_EDITOR
        cryptoKeyEditor = cryptoKey;
#endif
    }

    public void UpdateValue()
	{
		SetValue(GetValue());
	}

#if UNITY_EDITOR
    public static byte cryptoKeyEditor;
#endif

    public static int Encrypt(bool value)
    {
        return Encrypt(value, 0);
    }

    public static int Encrypt(bool value, byte key)
    {
        if (key == 0)
        {
#if UNITY_EDITOR
            key = cryptoKeyEditor;
#endif
        }
        int num = (!value) ? 32 : 18;
        return num ^ (int)key;
    }

    public static bool Decrypt(int value)
    {
        return Decrypt(value, 0);
    }

    public static bool Decrypt(int value, byte key)
    {
        if (key == 0)
        {
#if UNITY_EDITOR
            key = cryptoKeyEditor;
#endif
        }
        value ^= (int)key;
        return value != 32;
    }

    public void SetValue(bool value)
	{
		cryptoKey = (byte)CryptoManager.random.Next(1, 200);
		hiddenValue = (byte)(((!value) ? 32 : 18) ^ cryptoKey);
		fakeValue = (CryptoManager.fakeValue && value);
		fakeValueChanged = true;
	}

	private bool GetValue()
	{
		if (!inited)
		{
			cryptoKey = (byte)CryptoManager.random.Next(1, 200);
			hiddenValue = (byte)(32 ^ cryptoKey);
			fakeValue = false;
			fakeValueChanged = false;
			inited = true;
		}
		bool flag = (hiddenValue ^ cryptoKey) != 32;
		if (CryptoManager.fakeValue && fakeValueChanged && fakeValue != flag)
		{
			CryptoManager.CheatingDetected();
		}
		return flag;
	}

	public override bool Equals(object obj)
	{
		return obj is CryptoBool && Equals((CryptoBool)obj);
	}

	public bool Equals(CryptoBool obj)
	{
		return GetValue() == obj.GetValue();
	}

	public override int GetHashCode()
	{
		return GetValue().GetHashCode();
	}

	public override string ToString()
	{
		return GetValue().ToString();
	}

	public static implicit operator CryptoBool(bool value)
	{
		CryptoBool result = new CryptoBool(value);
		return result;
	}

	public static implicit operator bool(CryptoBool value)
	{
		return value.GetValue();
	}
}
