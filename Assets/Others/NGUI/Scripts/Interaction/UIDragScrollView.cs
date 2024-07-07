using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag Scroll View")]
public class UIDragScrollView : MonoBehaviour
{
	public UIScrollView scrollView;

	[SerializeField]
	[HideInInspector]
	private UIScrollView draggablePanel;

	private Transform mTrans;

	private UIScrollView mScroll;

	private bool mAutoFind;

	private bool mStarted;

	private GameObject mGameObject;

	[NonSerialized]
	private bool mPressed;

	private void OnEnable()
	{
		mGameObject = gameObject;
		mTrans = transform;
		if (scrollView == null && draggablePanel != null)
		{
			scrollView = draggablePanel;
			draggablePanel = null;
		}
		if (mStarted && (mAutoFind || mScroll == null))
		{
			FindScrollView();
		}
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
		UICamera.onDrag = (UICamera.VectorDelegate)Delegate.Combine(UICamera.onDrag, new UICamera.VectorDelegate(OnDrag));
		UICamera.onScroll = (UICamera.FloatDelegate)Delegate.Combine(UICamera.onScroll, new UICamera.FloatDelegate(OnScroll));
		UICamera.onPan = (UICamera.VectorDelegate)Delegate.Combine(UICamera.onPan, new UICamera.VectorDelegate(OnPan));
	}

	private void Start()
	{
		mStarted = true;
		FindScrollView();
	}

	private void FindScrollView()
	{
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(mTrans);
		if (scrollView == null || (mAutoFind && uIScrollView != scrollView))
		{
			scrollView = uIScrollView;
			mAutoFind = true;
		}
		else if (scrollView == uIScrollView)
		{
			mAutoFind = true;
		}
		mScroll = scrollView;
	}

	private void OnDisable()
	{
		if (mPressed && mScroll != null && mScroll.GetComponentInChildren<UIWrapContent>() == null)
		{
			mScroll.Press(false);
			mScroll = null;
		}
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPress));
		UICamera.onDrag = (UICamera.VectorDelegate)Delegate.Remove(UICamera.onDrag, new UICamera.VectorDelegate(OnDrag));
		UICamera.onScroll = (UICamera.FloatDelegate)Delegate.Remove(UICamera.onScroll, new UICamera.FloatDelegate(OnScroll));
		UICamera.onPan = (UICamera.VectorDelegate)Delegate.Remove(UICamera.onPan, new UICamera.VectorDelegate(OnPan));
	}

	private void OnPress(GameObject go, bool pressed)
	{
		if (mGameObject != go)
		{
			return;
		}
		mPressed = pressed;
		if (mAutoFind && mScroll != scrollView)
		{
			mScroll = scrollView;
			mAutoFind = false;
		}
		if ((bool)scrollView && enabled && NGUITools.GetActive(gameObject))
		{
			scrollView.Press(pressed);
			if (!pressed && mAutoFind)
			{
				scrollView = NGUITools.FindInParents<UIScrollView>(mTrans);
				mScroll = scrollView;
			}
		}
	}

	private void OnDrag(GameObject go, Vector2 delta)
	{
		if (!(mGameObject != go) && (bool)scrollView && NGUITools.GetActive(this))
		{
			scrollView.Drag();
		}
	}

	private void OnScroll(GameObject go, float delta)
	{
		if (!(mGameObject != go) && (bool)scrollView && NGUITools.GetActive(this))
		{
			scrollView.Scroll(delta);
		}
	}

	public void OnPan(GameObject go, Vector2 delta)
	{
		if (!(mGameObject != go) && (bool)scrollView && NGUITools.GetActive(this))
		{
			scrollView.OnPan(delta);
		}
	}
}
