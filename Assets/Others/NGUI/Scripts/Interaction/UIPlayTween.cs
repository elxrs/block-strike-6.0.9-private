using System;
using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Play Tween")]
public class UIPlayTween : MonoBehaviour
{
	public static UIPlayTween current;

	public GameObject tweenTarget;

	public int tweenGroup;

	public Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public bool resetIfDisabled;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public bool includeChildren;

	public List<EventDelegate> onFinished = new List<EventDelegate>();

	[SerializeField]
	[HideInInspector]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string callWhenFinished;

	private UITweener[] mTweens;

	private bool mStarted;

	private int mActive;

	private bool mActivated;

	private GameObject mGameObject;

	private void Awake()
	{
		mGameObject = gameObject;
		if (eventReceiver != null && EventDelegate.IsValid(onFinished))
		{
			eventReceiver = null;
			callWhenFinished = null;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
	}

	private void Start()
	{
		mStarted = true;
		if (tweenTarget == null)
		{
			tweenTarget = gameObject;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
	}

	private void OnEnable()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (mStarted)
		{
			OnHover(mGameObject, UICamera.IsHighlighted(mGameObject));
		}
		if (UICamera.currentTouch != null)
		{
			if (trigger == Trigger.OnPress || trigger == Trigger.OnPressTrue)
			{
				mActivated = UICamera.currentTouch.pressed == gameObject;
			}
			if (trigger == Trigger.OnHover || trigger == Trigger.OnHoverTrue)
			{
				mActivated = UICamera.currentTouch.current == gameObject;
			}
		}
		UIToggle component = GetComponent<UIToggle>();
		if (component != null)
		{
			EventDelegate.Add(component.onChange, OnToggle);
		}
		UICamera.onDragOver = (UICamera.ObjectDelegate)Delegate.Combine(UICamera.onDragOver, new UICamera.ObjectDelegate(OnDragOver));
		UICamera.onHover = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onHover, new UICamera.BoolDelegate(OnHover));
		UICamera.onDragOut = (UICamera.ObjectDelegate)Delegate.Combine(UICamera.onDragOut, new UICamera.ObjectDelegate(OnDragOut));
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
		UICamera.onDoubleClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onDoubleClick, new UICamera.VoidDelegate(OnDoubleClick));
		UICamera.onSelect = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onSelect, new UICamera.BoolDelegate(OnSelect));
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnClick));
	}

	private void OnDisable()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		UIToggle component = GetComponent<UIToggle>();
		if (component != null)
		{
			EventDelegate.Remove(component.onChange, OnToggle);
		}
	}

	private void OnDragOver(GameObject go, GameObject obj)
	{
		if (!(go != mGameObject) && trigger == Trigger.OnHover)
		{
			OnHover(go, true);
		}
	}

	private void OnHover(GameObject go, bool isOver)
	{
		if (go != mGameObject || !enabled || (trigger != Trigger.OnHover && (trigger != Trigger.OnHoverTrue || !isOver) && (trigger != Trigger.OnHoverFalse || isOver)) || isOver == mActivated)
		{
			return;
		}
		if (!isOver && UICamera.hoveredObject != null && UICamera.hoveredObject.transform.IsChildOf(transform))
		{
			UICamera.onHover = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onHover, new UICamera.BoolDelegate(CustomHoverListener));
			isOver = true;
			if (mActivated)
			{
				return;
			}
		}
		mActivated = isOver && trigger == Trigger.OnHover;
		Play(isOver);
	}

	private void CustomHoverListener(GameObject go, bool isOver)
	{
		if ((bool)this)
		{
			GameObject gameObject = base.gameObject;
			if (!gameObject || !go || (!(go == gameObject) && !go.transform.IsChildOf(transform)))
			{
				OnHover(mGameObject, false);
				UICamera.onHover = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onHover, new UICamera.BoolDelegate(CustomHoverListener));
			}
		}
	}

	private void OnDragOut(GameObject go, GameObject obj)
	{
		if (!(go != mGameObject) && enabled && mActivated)
		{
			mActivated = false;
			Play(false);
		}
	}

	private void OnPress(GameObject go, bool isPressed)
	{
		if (!(go != mGameObject) && enabled && (trigger == Trigger.OnPress || (trigger == Trigger.OnPressTrue && isPressed) || (trigger == Trigger.OnPressFalse && !isPressed)))
		{
			mActivated = isPressed && trigger == Trigger.OnPress;
			Play(isPressed);
		}
	}

	private void OnClick(GameObject go)
	{
		if (!(go != mGameObject) && enabled && trigger == Trigger.OnClick)
		{
			Play(true);
		}
	}

	private void OnDoubleClick(GameObject go)
	{
		if (!(go != mGameObject) && enabled && trigger == Trigger.OnDoubleClick)
		{
			Play(true);
		}
	}

	private void OnSelect(GameObject go, bool isSelected)
	{
		if (!(go != mGameObject) && enabled && (trigger == Trigger.OnSelect || (trigger == Trigger.OnSelectTrue && isSelected) || (trigger == Trigger.OnSelectFalse && !isSelected)))
		{
			mActivated = isSelected && trigger == Trigger.OnSelect;
			Play(isSelected);
		}
	}

	private void OnToggle()
	{
		if (enabled && !(UIToggle.current == null) && (trigger == Trigger.OnActivate || (trigger == Trigger.OnActivateTrue && UIToggle.current.value) || (trigger == Trigger.OnActivateFalse && !UIToggle.current.value)))
		{
			Play(UIToggle.current.value);
		}
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (disableWhenFinished == DisableCondition.DoNotDisable || mTweens == null)
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (uITweener.enabled)
				{
					flag = false;
					break;
				}
				if (uITweener.direction != (Direction)disableWhenFinished)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			if (flag2)
			{
				NGUITools.SetActive(tweenTarget, false);
			}
			mTweens = null;
		}
	}

	public void Play()
	{
		Play(true);
	}

	public void Play(bool forward)
	{
		mActive = 0;
		GameObject gameObject = ((!(tweenTarget == null)) ? tweenTarget : base.gameObject);
		if (!NGUITools.GetActive(gameObject))
		{
			if (ifDisabledOnPlay != EnableCondition.EnableThenPlay)
			{
				return;
			}
			NGUITools.SetActive(gameObject, true);
		}
		mTweens = ((!includeChildren) ? gameObject.GetComponents<UITweener>() : gameObject.GetComponentsInChildren<UITweener>());
		if (mTweens.Length == 0)
		{
			if (disableWhenFinished != 0)
			{
				NGUITools.SetActive(tweenTarget, false);
			}
			return;
		}
		bool flag = false;
		if (playDirection == Direction.Reverse)
		{
			forward = !forward;
		}
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup != tweenGroup)
			{
				continue;
			}
			if (!flag && !NGUITools.GetActive(gameObject))
			{
				flag = true;
				NGUITools.SetActive(gameObject, true);
			}
			mActive++;
			if (playDirection == Direction.Toggle)
			{
				EventDelegate.Add(uITweener.onFinished, OnFinished, true);
				uITweener.Toggle();
				continue;
			}
			if (resetOnPlay || (resetIfDisabled && !uITweener.enabled))
			{
				uITweener.Play(forward);
				uITweener.ResetToBeginning();
			}
			EventDelegate.Add(uITweener.onFinished, OnFinished, true);
			uITweener.Play(forward);
		}
	}

	private void OnFinished()
	{
		if (--mActive == 0 && current == null)
		{
			current = this;
			EventDelegate.Execute(onFinished);
			if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
			{
				eventReceiver.SendMessage(callWhenFinished, SendMessageOptions.DontRequireReceiver);
			}
			eventReceiver = null;
			current = null;
		}
	}
}
