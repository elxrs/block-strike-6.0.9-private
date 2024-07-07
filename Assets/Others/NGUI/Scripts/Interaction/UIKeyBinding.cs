﻿using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Key Binding")]
public class UIKeyBinding : MonoBehaviour
{
	[DoNotObfuscateNGUI]
	public enum Action
	{
		PressAndClick,
		Select,
		All
	}

	[DoNotObfuscateNGUI]
	public enum Modifier
	{
		Any,
		Shift,
		Ctrl,
		Alt,
		None
	}

	private static List<UIKeyBinding> mList = new List<UIKeyBinding>();

	public KeyCode keyCode;

	public Modifier modifier;

	public Action action;

	[NonSerialized]
	private bool mIgnoreUp;

	[NonSerialized]
	private bool mIsInput;

	[NonSerialized]
	private bool mPress;

	public string captionText
	{
		get
		{
			string text = NGUITools.KeyToCaption(keyCode);
			if (modifier == Modifier.None || modifier == Modifier.Any)
			{
				return text;
			}
			return string.Concat(modifier, "+", text);
		}
	}

	public static bool IsBound(KeyCode key)
	{
		int i = 0;
		for (int count = mList.Count; i < count; i++)
		{
			UIKeyBinding uIKeyBinding = mList[i];
			if (uIKeyBinding != null && uIKeyBinding.keyCode == key)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void OnEnable()
	{
		mList.Add(this);
	}

	protected virtual void OnDisable()
	{
		mList.Remove(this);
	}

	protected virtual void Start()
	{
		UIInput component = GetComponent<UIInput>();
		mIsInput = component != null;
		if (component != null)
		{
			EventDelegate.Add(component.onSubmit, OnSubmit);
		}
	}

	protected virtual void OnSubmit()
	{

	}

	protected virtual bool IsModifierActive()
	{
		return IsModifierActive(modifier);
	}

	public static bool IsModifierActive(Modifier modifier)
	{
		switch (modifier)
		{
			case Modifier.Any:
				return true;
			case Modifier.Alt:
				if (UICamera.GetKey(KeyCode.LeftAlt) || UICamera.GetKey(KeyCode.RightAlt))
				{
					return true;
				}
				break;
			case Modifier.Ctrl:
				if (UICamera.GetKey(KeyCode.LeftControl) || UICamera.GetKey(KeyCode.RightControl))
				{
					return true;
				}
				break;
			case Modifier.Shift:
				if (UICamera.GetKey(KeyCode.LeftShift) || UICamera.GetKey(KeyCode.RightShift))
				{
					return true;
				}
				break;
			case Modifier.None:
				return !UICamera.GetKey(KeyCode.LeftAlt) && !UICamera.GetKey(KeyCode.RightAlt) && !UICamera.GetKey(KeyCode.LeftControl) && !UICamera.GetKey(KeyCode.RightControl) && !UICamera.GetKey(KeyCode.LeftShift) && !UICamera.GetKey(KeyCode.RightShift);
		}
		return false;
	}

	protected virtual void Update()
	{
		if ((keyCode != KeyCode.Numlock && UICamera.inputHasFocus) || keyCode == KeyCode.None || !IsModifierActive())
		{
			return;
		}
		bool flag = UICamera.GetKeyDown(keyCode);
		bool flag2 = UICamera.GetKeyUp(keyCode);
		if (flag)
		{
			mPress = true;
		}
		if (action == Action.PressAndClick || action == Action.All)
		{
			if (flag)
			{
				UICamera.currentTouchID = -1;
				UICamera.currentKey = keyCode;
				OnBindingPress(true);
			}
			if (mPress && flag2)
			{
				UICamera.currentTouchID = -1;
				UICamera.currentKey = keyCode;
				OnBindingPress(false);
				OnBindingClick();
			}
		}
		if ((action == Action.Select || action == Action.All) && flag2)
		{
			if (mIsInput)
			{
				if (!mIgnoreUp && (keyCode == KeyCode.Numlock || !UICamera.inputHasFocus) && mPress)
				{
					UICamera.selectedObject = gameObject;
				}
				mIgnoreUp = false;
			}
			else if (mPress)
			{
				UICamera.hoveredObject = gameObject;
			}
		}
		if (flag2)
		{
			mPress = false;
		}
	}

	protected virtual void OnBindingPress(bool pressed)
	{
		UICamera.Notify(gameObject, "OnPress", pressed);
	}

	protected virtual void OnBindingClick()
	{
		UICamera.Notify(gameObject, "OnClick", null);
	}

	public override string ToString()
	{
		return GetString(keyCode, modifier);
	}

	public static string GetString(KeyCode keyCode, Modifier modifier)
	{
		return (modifier == Modifier.None) ? NGUITools.KeyToCaption(keyCode) : string.Concat(modifier, "+", NGUITools.KeyToCaption(keyCode));
	}

	public static bool GetKeyCode(string text, out KeyCode key, out Modifier modifier)
	{
		key = KeyCode.None;
		modifier = Modifier.None;
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		if (text.Length > 2 && text.Contains("+") && text[text.Length - 1] != '+')
		{
			string[] array = text.Split(new char[1] { '+' }, 2);
			key = NGUITools.CaptionToKey(array[1]);
			try
			{
				modifier = (Modifier)(int)Enum.Parse(typeof(Modifier), array[0]);
			}
			catch (Exception)
			{
				return false;
			}
		}
		else
		{
			modifier = Modifier.None;
			key = NGUITools.CaptionToKey(text);
		}
		return true;
	}

	public static Modifier GetActiveModifier()
	{
		Modifier result = Modifier.None;
		if (UICamera.GetKey(KeyCode.LeftAlt) || UICamera.GetKey(KeyCode.RightAlt))
		{
			result = Modifier.Alt;
		}
		else if (UICamera.GetKey(KeyCode.LeftShift) || UICamera.GetKey(KeyCode.RightShift))
		{
			result = Modifier.Shift;
		}
		else if (UICamera.GetKey(KeyCode.LeftControl) || UICamera.GetKey(KeyCode.RightControl))
		{
			result = Modifier.Ctrl;
		}
		return result;
	}
}
