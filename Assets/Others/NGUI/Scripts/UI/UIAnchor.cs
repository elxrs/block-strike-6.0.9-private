﻿using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
	[DoNotObfuscateNGUI]
	public enum Side
	{
		BottomLeft,
		Left,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		Center
	}

	public Camera uiCamera;

	public GameObject container;

	public Side side = Side.Center;

	public bool runOnlyOnce = true;

	public Vector2 relativeOffset = Vector2.zero;

	public Vector2 pixelOffset = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private UIWidget widgetContainer;

	private Transform mTrans;

	private Animation mAnim;

	private Rect mRect = default(Rect);

	private UIRoot mRoot;

	private bool mStarted;

	private void OnEnable()
	{
		mTrans = transform;
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		mAnim = animation;
#else
		mAnim = GetComponent<Animation>();
#endif
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void OnDisable()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void ScreenSizeChanged()
	{
		if (mStarted && runOnlyOnce)
		{
			Update();
		}
	}

	private void Start()
	{
		if (container == null && widgetContainer != null)
		{
			container = widgetContainer.gameObject;
			widgetContainer = null;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
		mRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
		}
		Update();
		mStarted = true;
	}

	private void Update()
	{
		if ((mAnim != null && mAnim.enabled && mAnim.isPlaying) || mTrans == null)
		{
			return;
		}
		bool flag = false;
		UIWidget uIWidget = ((!(container == null)) ? container.GetComponent<UIWidget>() : null);
		UIPanel uIPanel = ((!(container == null) || !(uIWidget == null)) ? container.GetComponent<UIPanel>() : null);
		if (uIWidget != null)
		{
			Bounds bounds = uIWidget.CalculateBounds(container.transform.parent);
			mRect.x = bounds.min.x;
			mRect.y = bounds.min.y;
			mRect.width = bounds.size.x;
			mRect.height = bounds.size.y;
		}
		else if (uIPanel != null)
		{
			if (uIPanel.clipping == UIDrawCall.Clipping.None)
			{
				float num = ((!(mRoot != null)) ? 0.5f : ((float)mRoot.activeHeight / (float)Screen.height * 0.5f));
				mRect.xMin = (float)(-Screen.width) * num;
				mRect.yMin = (float)(-Screen.height) * num;
				mRect.xMax = 0f - mRect.xMin;
				mRect.yMax = 0f - mRect.yMin;
			}
			else
			{
				Vector4 finalClipRegion = uIPanel.finalClipRegion;
				mRect.x = finalClipRegion.x - finalClipRegion.z * 0.5f;
				mRect.y = finalClipRegion.y - finalClipRegion.w * 0.5f;
				mRect.width = finalClipRegion.z;
				mRect.height = finalClipRegion.w;
			}
		}
		else if (container != null)
		{
			Transform parent = container.transform.parent;
			Bounds bounds2 = ((!(parent != null)) ? NGUIMath.CalculateRelativeWidgetBounds(container.transform) : NGUIMath.CalculateRelativeWidgetBounds(parent, container.transform));
			mRect.x = bounds2.min.x;
			mRect.y = bounds2.min.y;
			mRect.width = bounds2.size.x;
			mRect.height = bounds2.size.y;
		}
		else
		{
			if (!(uiCamera != null))
			{
				return;
			}
			flag = true;
			mRect = uiCamera.pixelRect;
		}
		float x = (mRect.xMin + mRect.xMax) * 0.5f;
		float y = (mRect.yMin + mRect.yMax) * 0.5f;
		Vector3 vector = new Vector3(x, y, 0f);
		if (side != Side.Center)
		{
			if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
			{
				vector.x = mRect.xMax;
			}
			else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
			{
				vector.x = x;
			}
			else
			{
				vector.x = mRect.xMin;
			}
			if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
			{
				vector.y = mRect.yMax;
			}
			else if (side == Side.Left || side == Side.Center || side == Side.Right)
			{
				vector.y = y;
			}
			else
			{
				vector.y = mRect.yMin;
			}
		}
		float width = mRect.width;
		float height = mRect.height;
		vector.x += pixelOffset.x + relativeOffset.x * width;
		vector.y += pixelOffset.y + relativeOffset.y * height;
		if (flag)
		{
			if (uiCamera.orthographic)
			{
				vector.x = Mathf.Round(vector.x);
				vector.y = Mathf.Round(vector.y);
			}
			vector.z = uiCamera.WorldToScreenPoint(mTrans.position).z;
			vector = uiCamera.ScreenToWorldPoint(vector);
		}
		else
		{
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			if (uIPanel != null)
			{
				vector = uIPanel.cachedTransform.TransformPoint(vector);
			}
			else if (container != null)
			{
				Transform parent2 = container.transform.parent;
				if (parent2 != null)
				{
					vector = parent2.TransformPoint(vector);
				}
			}
			vector.z = mTrans.position.z;
		}
		// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		if (flag && uiCamera.isOrthoGraphic && mTrans.parent != null)
#else
		if (flag && uiCamera.orthographic && mTrans.parent != null)
#endif
		{
			vector = mTrans.parent.InverseTransformPoint(vector);
			vector.x = Mathf.RoundToInt(vector.x);
			vector.y = Mathf.RoundToInt(vector.y);
			if (mTrans.localPosition != vector)
			{
				mTrans.localPosition = vector;
			}
		}
		else if (mTrans.position != vector)
		{
			mTrans.position = vector;
		}
		if (runOnlyOnce && Application.isPlaying)
		{
			enabled = false;
		}
	}
}
