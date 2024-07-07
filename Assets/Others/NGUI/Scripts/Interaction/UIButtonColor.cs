using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Color")]
[ExecuteInEditMode]
public class UIButtonColor : UIWidgetContainer
{
	[DoNotObfuscateNGUI]
	public enum State
	{
		Normal,
		Hover,
		Pressed,
		Disabled
	}

	public GameObject tweenTarget;

	public Color hover = new Color(0.88235295f, 40f / 51f, 0.5882353f, 1f);

	public Color pressed = new Color(61f / 85f, 0.6392157f, 41f / 85f, 1f);

	public Color disabledColor = Color.grey;

	public float duration = 0.2f;

	[NonSerialized]
	protected Color mStartingColor;

	[NonSerialized]
	protected Color mDefaultColor;

	[NonSerialized]
	protected bool mInitDone;

	[NonSerialized]
	protected UIWidget mWidget;

	[NonSerialized]
	protected State mState;

	public State state
	{
		get
		{
			return mState;
		}
		set
		{
			SetState(value, false);
		}
	}

	public Color defaultColor
	{
		get
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return Color.white;
#endif
			if (!mInitDone)
			{
				OnInit();
			}
			return mDefaultColor;
		}
		set
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			if (!mInitDone)
			{
				OnInit();
			}
			mDefaultColor = value;
			State state = mState;
			mState = State.Disabled;
			SetState(state, false);
		}
	}

	public virtual bool isEnabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	public void ResetDefaultColor()
	{
		defaultColor = mStartingColor;
	}

	public void CacheDefaultColor()
	{
		if (!mInitDone)
		{
			OnInit();
		}
	}

	private void Start()
	{
		if (!mInitDone)
		{
			OnInit();
		}
		if (!isEnabled)
		{
			SetState(State.Disabled, true);
		}
	}

	protected virtual void OnInit()
	{
		mInitDone = true;
		if (tweenTarget == null && !Application.isPlaying)
		{
			tweenTarget = gameObject;
		}
		if (tweenTarget != null)
		{
			mWidget = tweenTarget.GetComponent<UIWidget>();
		}
		if (mWidget != null)
		{
			mDefaultColor = mWidget.color;
			mStartingColor = mDefaultColor;
		}
		else
		{
			if (!(tweenTarget != null))
			{
				return;
			}
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			Renderer renderer = tweenTarget.renderer;
#else
			Renderer renderer = tweenTarget.GetComponent<Renderer>();
#endif
			if (renderer != null)
			{
				mDefaultColor = ((!Application.isPlaying) ? renderer.sharedMaterial.color : renderer.material.color);
				mStartingColor = mDefaultColor;
				return;
			}
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			Light light = tweenTarget.light;
#else
			Light light = tweenTarget.GetComponent<Light>();
#endif
			if (light != null)
			{
				mDefaultColor = light.color;
				mStartingColor = mDefaultColor;
			}
			else
			{
				tweenTarget = null;
				mInitDone = false;
			}
		}
	}

	protected virtual void OnEnable()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			mInitDone = false;
			return;
		}
#endif
		if (mInitDone)
		{
			OnHover(UICamera.IsHighlighted(gameObject));
		}
		if (UICamera.currentTouch != null)
		{
			if (UICamera.currentTouch.pressed == gameObject)
			{
				OnPress(true);
			}
			else if (UICamera.currentTouch.current == gameObject)
			{
				OnHover(true);
			}
		}
	}

	protected virtual void OnDisable()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (!mInitDone || mState == State.Normal)
		{
			return;
		}
		SetState(State.Normal, true);
		if (tweenTarget != null)
		{
			TweenColor component = tweenTarget.GetComponent<TweenColor>();
			if (component != null)
			{
				component.value = mDefaultColor;
				component.enabled = false;
			}
		}
	}

	protected virtual void OnHover(bool isOver)
	{
		if (isEnabled)
		{
			if (!mInitDone)
			{
				OnInit();
			}
			if (tweenTarget != null)
			{
				SetState(isOver ? State.Hover : State.Normal, false);
			}
		}
	}

	protected virtual void OnPress(bool isPressed)
	{
		if (!isEnabled)
		{
			return;
		}
		if (!mInitDone)
		{
			OnInit();
		}
		if (!(tweenTarget != null))
		{
			return;
		}
		if (isPressed)
		{
			SetState(State.Pressed, false);
		}
		else if (UICamera.currentTouch != null && UICamera.currentTouch.current == gameObject)
		{
			if (UICamera.currentScheme == UICamera.ControlScheme.Controller)
			{
				SetState(State.Hover, false);
			}
			else if (UICamera.currentScheme == UICamera.ControlScheme.Mouse && UICamera.hoveredObject == gameObject)
			{
				SetState(State.Hover, false);
			}
			else
			{
				SetState(State.Normal, false);
			}
		}
		else
		{
			SetState(State.Normal, false);
		}
	}

	protected virtual void OnDragOver()
	{
		if (isEnabled)
		{
			if (!mInitDone)
			{
				OnInit();
			}
			if (tweenTarget != null)
			{
				SetState(State.Pressed, false);
			}
		}
	}

	protected virtual void OnDragOut()
	{
		if (isEnabled)
		{
			if (!mInitDone)
			{
				OnInit();
			}
			if (tweenTarget != null)
			{
				SetState(State.Normal, false);
			}
		}
	}

	public virtual void SetState(State state, bool instant)
	{
		if (!mInitDone)
		{
			mInitDone = true;
			OnInit();
		}
		if (mState != state)
		{
			mState = state;
			UpdateColor(instant);
		}
	}

	public void UpdateColor(bool instant)
	{
		if (mInitDone && tweenTarget != null)
		{
			TweenColor tweenColor;
			switch (mState)
			{
				case State.Hover:
					tweenColor = TweenColor.Begin(tweenTarget, duration, hover);
					break;
				case State.Pressed:
					tweenColor = TweenColor.Begin(tweenTarget, duration, pressed);
					break;
				case State.Disabled:
					tweenColor = TweenColor.Begin(tweenTarget, duration, disabledColor);
					break;
				default:
					tweenColor = TweenColor.Begin(tweenTarget, duration, mDefaultColor);
					break;
			}
			if (instant && tweenColor != null)
			{
				tweenColor.value = tweenColor.to;
				tweenColor.enabled = false;
			}
		}
	}
}
