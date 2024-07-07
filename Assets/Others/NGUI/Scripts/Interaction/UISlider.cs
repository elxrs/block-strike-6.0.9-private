using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/NGUI Slider")]
[ExecuteInEditMode]
public class UISlider : UIProgressBar
{
	private enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded
	}

	[HideInInspector]
	[SerializeField]
	private Transform foreground;

	[SerializeField]
	[HideInInspector]
	private float rawValue = 1f;

	[HideInInspector]
	[SerializeField]
	private Direction direction = Direction.Upgraded;

	[SerializeField]
	[HideInInspector]
	protected bool mInverted;

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
	public float sliderValue
	{
		get
		{
			return value;
		}
		set
		{
			base.value = value;
		}
	}

	[Obsolete("Use 'fillDirection' instead")]
	public bool inverted
	{
		get
		{
			return isInverted;
		}
		set
		{
		}
	}

	protected override void Upgrade()
	{
		if (direction != Direction.Upgraded)
		{
			mValue = rawValue;
			if (foreground != null)
			{
				mFG = foreground.GetComponent<UIWidget>();
			}
			if (direction == Direction.Horizontal)
			{
				mFill = (mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight);
			}
			else
			{
				mFill = ((!mInverted) ? FillDirection.BottomToTop : FillDirection.TopToBottom);
			}
			direction = Direction.Upgraded;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
	}

	protected override void OnStart()
	{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		GameObject bg = (mBG != null && (mBG.collider != null || mBG.GetComponent<Collider2D>() != null)) ? mBG.gameObject : gameObject;
		UIEventListener bgl = UIEventListener.Get(bg);
		bgl.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(bgl.onPress, new UIEventListener.BoolDelegate(OnPressBackground));
		bgl.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(bgl.onDrag, new UIEventListener.VectorDelegate(OnDragBackground));

		if (thumb != null && (thumb.collider != null || thumb.GetComponent<Collider2D>() != null) && (mFG == null || thumb != mFG.cachedTransform))
#else
		GameObject bg = (mBG != null && (mBG.GetComponent<Collider>() != null || mBG.GetComponent<Collider2D>() != null)) ? mBG.gameObject : gameObject;
		UIEventListener bgl = UIEventListener.Get(bg);
		bgl.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(bgl.onPress, new UIEventListener.BoolDelegate(OnPressBackground));
		bgl.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(bgl.onDrag, new UIEventListener.VectorDelegate(OnDragBackground));

		if (thumb != null && (thumb.GetComponent<Collider>() != null || thumb.GetComponent<Collider2D>() != null) && (mFG == null || thumb != mFG.cachedTransform))
#endif
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(thumb.gameObject);
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnPressForeground));
			uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, new UIEventListener.VectorDelegate(OnDragForeground));
		}
	}

	protected void OnPressBackground(GameObject go, bool isPressed)
	{
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			value = ScreenToValue(UICamera.lastEventPosition);
			if (!isPressed && onDragFinished != null)
			{
				onDragFinished();
			}
		}
	}

	protected void OnDragBackground(GameObject go, Vector2 delta)
	{
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			value = ScreenToValue(UICamera.lastEventPosition);
		}
	}

	protected void OnPressForeground(GameObject go, bool isPressed)
	{
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			if (isPressed)
			{
				mOffset = ((!(mFG == null)) ? (value - ScreenToValue(UICamera.lastEventPosition)) : 0f);
			}
			else if (onDragFinished != null)
			{
				onDragFinished();
			}
		}
	}

	protected void OnDragForeground(GameObject go, Vector2 delta)
	{
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			value = mOffset + ScreenToValue(UICamera.lastEventPosition);
		}
	}

	public override void OnPan(Vector2 delta)
	{
		if (enabled && isColliderEnabled)
		{
			base.OnPan(delta);
		}
	}
}
