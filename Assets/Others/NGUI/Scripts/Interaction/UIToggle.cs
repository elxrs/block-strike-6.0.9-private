﻿using System;
using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Toggle")]
[ExecuteInEditMode]
public class UIToggle : UIWidgetContainer
{
	public delegate bool Validate(bool choice);

	public static BetterList<UIToggle> list = new BetterList<UIToggle>();

	public static UIToggle current;

	public int group;

	public UIWidget activeSprite;

	public bool invertSpriteState;

	public Animation activeAnimation;

	public Animator animator;

	public UITweener tween;

	public bool startsActive;

	public bool instantTween;

	public bool optionCanBeNone;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	public Validate validator;

	[SerializeField]
	[HideInInspector]
	private UISprite checkSprite;

	[SerializeField]
	[HideInInspector]
	private Animation checkAnimation;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string functionName = "OnActivate";

	[SerializeField]
	[HideInInspector]
	private bool startsChecked;

	private bool mIsActive = true;

	private bool mStarted;

	private GameObject mGameObject;

	public bool value
	{
		get
		{
			return (!mStarted) ? startsActive : mIsActive;
		}
		set
		{
			if (!mStarted)
			{
				startsActive = value;
			}
			else if (group == 0 || value || optionCanBeNone || !mStarted)
			{
				Set(value);
			}
		}
	}

	public bool isColliderEnabled
	{
		get
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				return component.enabled;
			}
			Collider2D component2 = GetComponent<Collider2D>();
			return component2 != null && component2.enabled;
		}
	}

	[Obsolete("Use 'value' instead")]
	public bool isChecked
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public static UIToggle GetActiveToggle(int group)
	{
		for (int i = 0; i < list.size; i++)
		{
			UIToggle uIToggle = list.buffer[i];
			if (uIToggle != null && uIToggle.group == group && uIToggle.mIsActive)
			{
				return uIToggle;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnClick));
		list.Add(this);
	}

	private void OnDisable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onClick, new UICamera.VoidDelegate(OnClick));
		list.Remove(this);
	}

	public void Start()
	{
		mGameObject = gameObject;
		if (mStarted)
		{
			return;
		}
		if (startsChecked)
		{
			startsChecked = false;
			startsActive = true;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
		if (!Application.isPlaying)
		{
			if (checkSprite != null && activeSprite == null)
			{
				activeSprite = checkSprite;
				checkSprite = null;
			}
			if (checkAnimation != null && activeAnimation == null)
			{
				activeAnimation = checkAnimation;
				checkAnimation = null;
			}
			if (Application.isPlaying && activeSprite != null)
			{
				activeSprite.alpha = (invertSpriteState ? ((!startsActive) ? 1f : 0f) : ((!startsActive) ? 0f : 1f));
			}
			if (EventDelegate.IsValid(onChange))
			{
				eventReceiver = null;
				functionName = null;
			}
		}
		else
		{
			mIsActive = !startsActive;
			mStarted = true;
			bool flag = instantTween;
			instantTween = true;
			Set(startsActive);
			instantTween = flag;
		}
	}

	private void OnClick(GameObject go)
	{
		if (!(mGameObject != go) && enabled && isColliderEnabled && UICamera.currentTouchID != -2)
		{
			value = !value;
		}
	}

	public void Set(bool state, bool notify = true)
	{
		if (validator != null && !validator(state))
		{
			return;
		}
		if (!mStarted)
		{
			mIsActive = state;
			startsActive = state;
			if (activeSprite != null)
			{
				activeSprite.alpha = (invertSpriteState ? ((!state) ? 1f : 0f) : ((!state) ? 0f : 1f));
			}
		}
		else
		{
			if (mIsActive == state)
			{
				return;
			}
			if (group != 0 && state)
			{
				int num = 0;
				int size = list.size;
				while (num < size)
				{
					UIToggle uIToggle = list.buffer[num];
					if (uIToggle != this && uIToggle.group == group)
					{
						uIToggle.Set(false);
					}
					if (list.size != size)
					{
						size = list.size;
						num = 0;
					}
					else
					{
						num++;
					}
				}
			}
			mIsActive = state;
			if (activeSprite != null)
			{
				if (instantTween || !NGUITools.GetActive(this))
				{
					activeSprite.alpha = (invertSpriteState ? ((!mIsActive) ? 1f : 0f) : ((!mIsActive) ? 0f : 1f));
				}
				else
				{
					TweenAlpha.Begin(activeSprite.gameObject, 0.15f, invertSpriteState ? ((!mIsActive) ? 1f : 0f) : ((!mIsActive) ? 0f : 1f));
				}
			}
			if (notify && current == null)
			{
				UIToggle uIToggle2 = current;
				current = this;
				if (EventDelegate.IsValid(onChange))
				{
					EventDelegate.Execute(onChange);
				}
				else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
				{
					eventReceiver.SendMessage(functionName, mIsActive, SendMessageOptions.DontRequireReceiver);
				}
				current = uIToggle2;
			}
			if (animator != null)
			{
				ActiveAnimation activeAnimation = ActiveAnimation.Play(animator, null, state ? Direction.Forward : Direction.Reverse, EnableCondition.IgnoreDisabledState, DisableCondition.DoNotDisable);
				if (activeAnimation != null && (instantTween || !NGUITools.GetActive(this)))
				{
					activeAnimation.Finish();
				}
			}
			else if (activeAnimation != null)
			{
				ActiveAnimation activeAnimation2 = ActiveAnimation.Play(this.activeAnimation, null, state ? Direction.Forward : Direction.Reverse, EnableCondition.IgnoreDisabledState, DisableCondition.DoNotDisable);
				if (activeAnimation2 != null && (instantTween || !NGUITools.GetActive(this)))
				{
					activeAnimation2.Finish();
				}
			}
			else
			{
				if (!(tween != null))
				{
					return;
				}
				bool flag = NGUITools.GetActive(this);
				if (tween.tweenGroup != 0)
				{
					UITweener[] componentsInChildren = tween.GetComponentsInChildren<UITweener>(true);
					int i = 0;
					for (int num2 = componentsInChildren.Length; i < num2; i++)
					{
						UITweener uITweener = componentsInChildren[i];
						if (uITweener.tweenGroup == tween.tweenGroup)
						{
							uITweener.Play(state);
							if (instantTween || !flag)
							{
								uITweener.tweenFactor = ((!state) ? 0f : 1f);
							}
						}
					}
				}
				else
				{
					tween.Play(state);
					if (instantTween || !flag)
					{
						tween.tweenFactor = ((!state) ? 0f : 1f);
					}
				}
			}
		}
	}
}
