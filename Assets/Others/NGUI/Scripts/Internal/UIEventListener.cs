using System;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	private GameObject mGameObject;

	public object parameter;

	public VoidDelegate onSubmit;

	public VoidDelegate onClick;

	public VoidDelegate onDoubleClick;

	public BoolDelegate onHover;

	public BoolDelegate onPress;

	public BoolDelegate onSelect;

	public FloatDelegate onScroll;

	public VoidDelegate onDragStart;

	public VectorDelegate onDrag;

	public VoidDelegate onDragOver;

	public VoidDelegate onDragOut;

	public VoidDelegate onDragEnd;

	public ObjectDelegate onDrop;

	public KeyCodeDelegate onKey;

	public BoolDelegate onTooltip;

	public bool needsActiveCollider = true;

	private bool isColliderEnabled
	{
		get
		{
			if (!needsActiveCollider)
			{
				return true;
			}
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				return component.enabled;
			}
			Collider2D component2 = GetComponent<Collider2D>();
			return component2 != null && component2.enabled;
		}
	}

	private void Start()
	{
		mGameObject = gameObject;
	}

	private void OnEnable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnClick));
		UICamera.onDoubleClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onDoubleClick, new UICamera.VoidDelegate(OnDoubleClick));
		UICamera.onHover = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onHover, new UICamera.BoolDelegate(OnHover));
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
		UICamera.onSelect = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onSelect, new UICamera.BoolDelegate(OnSelect));
		UICamera.onScroll = (UICamera.FloatDelegate)Delegate.Combine(UICamera.onScroll, new UICamera.FloatDelegate(OnScroll));
		UICamera.onDragStart = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onDragStart, new UICamera.VoidDelegate(OnDragStart));
		UICamera.onDrag = (UICamera.VectorDelegate)Delegate.Combine(UICamera.onDrag, new UICamera.VectorDelegate(OnDrag));
		UICamera.onDragOver = (UICamera.ObjectDelegate)Delegate.Combine(UICamera.onDragOver, new UICamera.ObjectDelegate(OnDragOver));
		UICamera.onDragOut = (UICamera.ObjectDelegate)Delegate.Combine(UICamera.onDragOut, new UICamera.ObjectDelegate(OnDragOut));
		UICamera.onDragEnd = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onDragEnd, new UICamera.VoidDelegate(OnDragEnd));
		UICamera.onDrop = (UICamera.ObjectDelegate)Delegate.Combine(UICamera.onDrop, new UICamera.ObjectDelegate(OnDrop));
		UICamera.onKey = (UICamera.KeyCodeDelegate)Delegate.Combine(UICamera.onKey, new UICamera.KeyCodeDelegate(OnKey));
		UICamera.onTooltip = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onTooltip, new UICamera.BoolDelegate(OnTooltip));
	}

	private void OnDisable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onClick, new UICamera.VoidDelegate(OnClick));
		UICamera.onDoubleClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onDoubleClick, new UICamera.VoidDelegate(OnDoubleClick));
		UICamera.onHover = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onHover, new UICamera.BoolDelegate(OnHover));
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
		UICamera.onSelect = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onSelect, new UICamera.BoolDelegate(OnSelect));
		UICamera.onScroll = (UICamera.FloatDelegate)Delegate.Remove(UICamera.onScroll, new UICamera.FloatDelegate(OnScroll));
		UICamera.onDragStart = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onDragStart, new UICamera.VoidDelegate(OnDragStart));
		UICamera.onDrag = (UICamera.VectorDelegate)Delegate.Remove(UICamera.onDrag, new UICamera.VectorDelegate(OnDrag));
		UICamera.onDragOver = (UICamera.ObjectDelegate)Delegate.Remove(UICamera.onDragOver, new UICamera.ObjectDelegate(OnDragOver));
		UICamera.onDragOut = (UICamera.ObjectDelegate)Delegate.Remove(UICamera.onDragOut, new UICamera.ObjectDelegate(OnDragOut));
		UICamera.onDragEnd = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onDragEnd, new UICamera.VoidDelegate(OnDragEnd));
		UICamera.onDrop = (UICamera.ObjectDelegate)Delegate.Remove(UICamera.onDrop, new UICamera.ObjectDelegate(OnDrop));
		UICamera.onKey = (UICamera.KeyCodeDelegate)Delegate.Remove(UICamera.onKey, new UICamera.KeyCodeDelegate(OnKey));
		UICamera.onTooltip = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onTooltip, new UICamera.BoolDelegate(OnTooltip));
	}

	private void OnSubmit()
	{
		if (isColliderEnabled && onSubmit != null)
		{
			onSubmit(mGameObject);
		}
	}

	private void OnClick(GameObject go)
	{
		if (!(mGameObject != go) && isColliderEnabled && onClick != null)
		{
			onClick(mGameObject);
		}
	}

	private void OnDoubleClick(GameObject go)
	{
		if (!(mGameObject != go) && isColliderEnabled && onDoubleClick != null)
		{
			onDoubleClick(mGameObject);
		}
	}

	private void OnHover(GameObject go, bool isOver)
	{
		if (!(mGameObject != go) && isColliderEnabled && onHover != null)
		{
			onHover(mGameObject, isOver);
		}
	}

	private void OnPress(GameObject go, bool isPressed)
	{
		if (!(mGameObject != go) && isColliderEnabled && onPress != null)
		{
			onPress(mGameObject, isPressed);
		}
	}

	private void OnSelect(GameObject go, bool selected)
	{
		if (!(mGameObject != go) && isColliderEnabled && onSelect != null)
		{
			onSelect(mGameObject, selected);
		}
	}

	private void OnScroll(GameObject go, float delta)
	{
		if (!(mGameObject != go) && isColliderEnabled && onScroll != null)
		{
			onScroll(mGameObject, delta);
		}
	}

	private void OnDragStart(GameObject go)
	{
		if (!(mGameObject != go) && onDragStart != null)
		{
			onDragStart(mGameObject);
		}
	}

	private void OnDrag(GameObject go, Vector2 delta)
	{
		if (!(mGameObject != go) && onDrag != null)
		{
			onDrag(mGameObject, delta);
		}
	}

	private void OnDragOver(GameObject go, GameObject obj)
	{
		if (!(mGameObject != go) && isColliderEnabled && onDragOver != null)
		{
			onDragOver(mGameObject);
		}
	}

	private void OnDragOut(GameObject go, GameObject obj)
	{
		if (!(mGameObject != go) && isColliderEnabled && onDragOut != null)
		{
			onDragOut(mGameObject);
		}
	}

	private void OnDragEnd(GameObject go)
	{
		if (!(mGameObject != go) && onDragEnd != null)
		{
			onDragEnd(mGameObject);
		}
	}

	private void OnDrop(GameObject go, GameObject obj)
	{
		if (!(mGameObject != go) && isColliderEnabled && onDrop != null)
		{
			onDrop(mGameObject, obj);
		}
	}

	private void OnKey(GameObject go, KeyCode key)
	{
		if (!(mGameObject != go) && isColliderEnabled && onKey != null)
		{
			onKey(mGameObject, key);
		}
	}

	private void OnTooltip(GameObject go, bool show)
	{
		if (!(mGameObject != go) && isColliderEnabled && onTooltip != null)
		{
			onTooltip(mGameObject, show);
		}
	}

	public void Clear()
	{
		onSubmit = null;
		onClick = null;
		onDoubleClick = null;
		onHover = null;
		onPress = null;
		onSelect = null;
		onScroll = null;
		onDragStart = null;
		onDrag = null;
		onDragOver = null;
		onDragOut = null;
		onDragEnd = null;
		onDrop = null;
		onKey = null;
		onTooltip = null;
	}

	public static UIEventListener Get(GameObject go)
	{
		UIEventListener uIEventListener = go.GetComponent<UIEventListener>();
		if (uIEventListener == null)
		{
			uIEventListener = go.AddComponent<UIEventListener>();
		}
		return uIEventListener;
	}
}
