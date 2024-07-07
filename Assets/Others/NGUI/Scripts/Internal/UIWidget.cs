﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Invisible Widget")]
[ExecuteInEditMode]
public class UIWidget : UIRect
{
	[DoNotObfuscateNGUI]
	public enum Pivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight
	}

	[DoNotObfuscateNGUI]
	public enum AspectRatioSource
	{
		Free,
		BasedOnWidth,
		BasedOnHeight
	}

	public delegate void OnDimensionsChanged();

	public delegate void OnPostFillCallback(UIWidget widget, int bufferOffset, List<Vector3> verts, List<Vector2> uvs, List<Color> cols);

	public delegate bool HitCheck(Vector3 worldPos);

	public bool isCalculateFinalAlpha;

	[HideInInspector]
	[SerializeField]
	protected Color mColor = Color.white;

	[SerializeField]
	[HideInInspector]
	protected Pivot mPivot = Pivot.Center;

	[SerializeField]
	[HideInInspector]
	protected int mWidth = 100;

	[HideInInspector]
	[SerializeField]
	protected int mHeight = 100;

	[SerializeField]
	[HideInInspector]
	protected int mDepth;

	[SerializeField]
	[Tooltip("Custom material, if desired")]
	[HideInInspector]
	protected Material mMat;

	public OnDimensionsChanged onChange;

	public OnPostFillCallback onPostFill;

	public UIDrawCall.OnRenderCallback mOnRender;

	public bool autoResizeBoxCollider;

	public bool hideIfOffScreen;

	public AspectRatioSource keepAspectRatio;

	public float aspectRatio = 1f;

	public HitCheck hitCheck;

	[NonSerialized]
	public UIPanel panel;

	[NonSerialized]
	public UIGeometry geometry = new UIGeometry();

	[NonSerialized]
	public bool fillGeometry = true;

	[NonSerialized]
	protected bool mPlayMode = true;

	[NonSerialized]
	protected Vector4 mDrawRegion = new Vector4(0f, 0f, 1f, 1f);

	[NonSerialized]
	private Matrix4x4 mLocalToPanel;

	[NonSerialized]
	private bool mIsVisibleByAlpha = true;

	[NonSerialized]
	private bool mIsVisibleByPanel = true;

	[NonSerialized]
	private bool mIsInFront = true;

	[NonSerialized]
	private float mLastAlpha;

	[NonSerialized]
	private bool mMoved;

	[NonSerialized]
	public UIDrawCall drawCall;

	[NonSerialized]
	protected Vector3[] mCorners = new Vector3[4];

	[NonSerialized]
	private int mAlphaFrameID = -1;

	private int mMatrixFrame = -1;

	private Vector3 mOldV0;

	private Vector3 mOldV1;

	public UIDrawCall.OnRenderCallback onRender
	{
		get
		{
			return mOnRender;
		}
		set
		{
			if (mOnRender != value)
			{
				if (drawCall != null && drawCall.onRender != null && mOnRender != null)
				{
					UIDrawCall uIDrawCall = drawCall;
					uIDrawCall.onRender = (UIDrawCall.OnRenderCallback)Delegate.Remove(uIDrawCall.onRender, mOnRender);
				}
				mOnRender = value;
				if (drawCall != null)
				{
					UIDrawCall uIDrawCall2 = drawCall;
					uIDrawCall2.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(uIDrawCall2.onRender, value);
				}
			}
		}
	}

	public Vector4 drawRegion
	{
		get
		{
			return mDrawRegion;
		}
		set
		{
			if (mDrawRegion != value)
			{
				mDrawRegion = value;
				if (autoResizeBoxCollider)
				{
					ResizeCollider();
				}
				MarkAsChanged();
			}
		}
	}

	public Vector2 pivotOffset
	{
		get
		{
			return NGUIMath.GetPivotOffset(pivot);
		}
	}

	public int width
	{
		get
		{
			return mWidth;
		}
		set
		{
			int num = minWidth;
			if (value < num)
			{
				value = num;
			}
			if (mWidth != value && keepAspectRatio != AspectRatioSource.BasedOnHeight)
			{
				if (isAnchoredHorizontally)
				{
					if (leftAnchor.target != null && rightAnchor.target != null)
					{
						if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Left || mPivot == Pivot.TopLeft)
						{
							NGUIMath.AdjustWidget(this, 0f, 0f, value - mWidth, 0f);
						}
						else if (mPivot == Pivot.BottomRight || mPivot == Pivot.Right || mPivot == Pivot.TopRight)
						{
							NGUIMath.AdjustWidget(this, mWidth - value, 0f, 0f, 0f);
						}
						else
						{
							int num2 = value - mWidth;
							num2 -= num2 & 1;
							if (num2 != 0)
							{
								NGUIMath.AdjustWidget(this, (float)(-num2) * 0.5f, 0f, (float)num2 * 0.5f, 0f);
							}
						}
					}
					else if (leftAnchor.target != null)
					{
						NGUIMath.AdjustWidget(this, 0f, 0f, value - mWidth, 0f);
					}
					else
					{
						NGUIMath.AdjustWidget(this, mWidth - value, 0f, 0f, 0f);
					}
				}
				else
				{
					SetDimensions(value, mHeight);
				}
			}
			UpdateWidget();
		}
	}

	public int height
	{
		get
		{
			return mHeight;
		}
		set
		{
			int num = minHeight;
			if (value < num)
			{
				value = num;
			}
			if (mHeight != value && keepAspectRatio != AspectRatioSource.BasedOnWidth)
			{
				if (isAnchoredVertically)
				{
					if (bottomAnchor.target != null && topAnchor.target != null)
					{
						if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Bottom || mPivot == Pivot.BottomRight)
						{
							NGUIMath.AdjustWidget(this, 0f, 0f, 0f, value - mHeight);
						}
						else if (mPivot == Pivot.TopLeft || mPivot == Pivot.Top || mPivot == Pivot.TopRight)
						{
							NGUIMath.AdjustWidget(this, 0f, mHeight - value, 0f, 0f);
						}
						else
						{
							int num2 = value - mHeight;
							num2 -= num2 & 1;
							if (num2 != 0)
							{
								NGUIMath.AdjustWidget(this, 0f, (float)(-num2) * 0.5f, 0f, (float)num2 * 0.5f);
							}
						}
					}
					else if (bottomAnchor.target != null)
					{
						NGUIMath.AdjustWidget(this, 0f, 0f, 0f, value - mHeight);
					}
					else
					{
						NGUIMath.AdjustWidget(this, 0f, mHeight - value, 0f, 0f);
					}
				}
				else
				{
					SetDimensions(mWidth, value);
				}
			}
			UpdateWidget();
		}
	}

	public Color color
	{
		get
		{
			return mColor;
		}
		set
		{
			if (mColor != value)
			{
				bool includeChildren = mColor.a != value.a;
				mColor = value;
				Invalidate(includeChildren);
				if (widgetAreStatic)
				{
					UpdateWidget();
				}
			}
		}
	}

	public override float alpha
	{
		get
		{
			return mColor.a;
		}
		set
		{
			if (mColor.a != value)
			{
				mColor.a = value;
				Invalidate(true);
				if (widgetAreStatic)
				{
					UpdateWidget();
				}
			}
		}
	}

	public bool isVisible
	{
		get
		{
			return mIsVisibleByPanel && mIsVisibleByAlpha && mIsInFront && finalAlpha > 0.001f && NGUITools.GetActive(this);
		}
	}

	public bool hasVertices
	{
		get
		{
			return geometry != null && geometry.hasVertices;
		}
	}

	public Pivot rawPivot
	{
		get
		{
			return mPivot;
		}
		set
		{
			if (mPivot != value)
			{
				mPivot = value;
				if (autoResizeBoxCollider)
				{
					ResizeCollider();
				}
				MarkAsChanged();
			}
		}
	}

	public Pivot pivot
	{
		get
		{
			return mPivot;
		}
		set
		{
			if (mPivot != value)
			{
				Vector3 vector = worldCorners[0];
				mPivot = value;
				mChanged = true;
				Vector3 vector2 = worldCorners[0];
				Transform transform = cachedTransform;
				Vector3 position = transform.position;
				float z = transform.localPosition.z;
				position.x += vector.x - vector2.x;
				position.y += vector.y - vector2.y;
				cachedTransform.position = position;
				position = cachedTransform.localPosition;
				position.x = Mathf.Round(position.x);
				position.y = Mathf.Round(position.y);
				position.z = z;
				cachedTransform.localPosition = position;
			}
		}
	}

	public int depth
	{
		get
		{
			return mDepth;
		}
		set
		{
			if (mDepth == value)
			{
				return;
			}
			if (panel != null)
			{
				panel.RemoveWidget(this);
			}
			mDepth = value;
			if (panel != null)
			{
				panel.AddWidget(this);
				if (!Application.isPlaying)
				{
					panel.SortWidgets();
					panel.RebuildAllDrawCalls();
				}
			}
			cachedTransform.localPosition = new Vector3(cachedTransform.localPosition.x, cachedTransform.localPosition.y, (float)mDepth * -0.01f);
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
	}

	public int raycastDepth
	{
		get
		{
			if (panel == null)
			{
				CreatePanel();
			}
			return (!(panel != null)) ? mDepth : (mDepth + panel.depth * 1000);
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			Vector2 vector = pivotOffset;
			float num = (0f - vector.x) * (float)mWidth;
			float num2 = (0f - vector.y) * (float)mHeight;
			float x = num + (float)mWidth;
			float y = num2 + (float)mHeight;
			mCorners[0] = new Vector3(num, num2);
			mCorners[1] = new Vector3(num, y);
			mCorners[2] = new Vector3(x, y);
			mCorners[3] = new Vector3(x, num2);
			return mCorners;
		}
	}

	public virtual Vector2 localSize
	{
		get
		{
			Vector3[] array = localCorners;
			return array[2] - array[0];
		}
	}

	public Vector3 localCenter
	{
		get
		{
			Vector3[] array = localCorners;
			return Vector3.Lerp(array[0], array[2], 0.5f);
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			Vector2 vector = pivotOffset;
			float num = (0f - vector.x) * (float)mWidth;
			float num2 = (0f - vector.y) * (float)mHeight;
			float x = num + (float)mWidth;
			float y = num2 + (float)mHeight;
			Transform transform = cachedTransform;
			mCorners[0] = transform.TransformPoint(num, num2, 0f);
			mCorners[1] = transform.TransformPoint(num, y, 0f);
			mCorners[2] = transform.TransformPoint(x, y, 0f);
			mCorners[3] = transform.TransformPoint(x, num2, 0f);
			return mCorners;
		}
	}

	public Vector3 worldCenter
	{
		get
		{
			return cachedTransform.TransformPoint(localCenter);
		}
	}

	public virtual Vector4 drawingDimensions
	{
		get
		{
			Vector2 vector = pivotOffset;
			float num = (0f - vector.x) * (float)mWidth;
			float num2 = (0f - vector.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			return new Vector4((mDrawRegion.x != 0f) ? Mathf.Lerp(num, num3, mDrawRegion.x) : num, (mDrawRegion.y != 0f) ? Mathf.Lerp(num2, num4, mDrawRegion.y) : num2, (mDrawRegion.z != 1f) ? Mathf.Lerp(num, num3, mDrawRegion.z) : num3, (mDrawRegion.w != 1f) ? Mathf.Lerp(num2, num4, mDrawRegion.w) : num4);
		}
	}

	public virtual Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				RemoveFromPanel();
				mMat = value;
				MarkAsChanged();
			}
		}
	}

	public virtual Texture mainTexture
	{
		get
		{
			Material material = this.material;
			return (!(material != null)) ? null : material.mainTexture;
		}
		set
		{
			throw new NotImplementedException(string.Concat(GetType(), " has no mainTexture setter"));
		}
	}

	public virtual Shader shader
	{
		get
		{
			Material material = this.material;
			return (!(material != null)) ? null : material.shader;
		}
		set
		{
			throw new NotImplementedException(string.Concat(GetType(), " has no shader setter"));
		}
	}

	[Obsolete("There is no relative scale anymore. Widgets now have width and height instead")]
	public Vector2 relativeSize
	{
		get
		{
			return Vector2.one;
		}
	}

	public bool hasBoxCollider
	{
		get
		{
			BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
			if (boxCollider != null)
			{
				return true;
			}
			return GetComponent<BoxCollider2D>() != null;
		}
	}

	public virtual int minWidth
	{
		get
		{
			return 2;
		}
	}

	public virtual int minHeight
	{
		get
		{
			return 2;
		}
	}

	public virtual Vector4 border
	{
		get
		{
			return Vector4.zero;
		}
		set
		{
		}
	}

	public void SetColorNoAlpha(Color c)
	{
		if (mColor.r != c.r || mColor.g != c.g || mColor.b != c.b)
		{
			mColor.r = c.r;
			mColor.g = c.g;
			mColor.b = c.b;
			Invalidate(false);
		}
	}

	public void SetDimensions(int w, int h)
	{
		if (mWidth != w || mHeight != h)
		{
			mWidth = w;
			mHeight = h;
			if (keepAspectRatio == AspectRatioSource.BasedOnWidth)
			{
				mHeight = Mathf.RoundToInt((float)mWidth / aspectRatio);
			}
			else if (keepAspectRatio == AspectRatioSource.BasedOnHeight)
			{
				mWidth = Mathf.RoundToInt((float)mHeight * aspectRatio);
			}
			else if (keepAspectRatio == AspectRatioSource.Free)
			{
				aspectRatio = (float)mWidth / (float)mHeight;
			}
			mMoved = true;
			if (autoResizeBoxCollider)
			{
				ResizeCollider();
			}
			MarkAsChanged();
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		Vector2 vector = pivotOffset;
		float num = (0f - vector.x) * (float)mWidth;
		float num2 = (0f - vector.y) * (float)mHeight;
		float num3 = num + (float)mWidth;
		float num4 = num2 + (float)mHeight;
		float x = (num + num3) * 0.5f;
		float y = (num2 + num4) * 0.5f;
		Transform transform = cachedTransform;
		mCorners[0] = transform.TransformPoint(num, y, 0f);
		mCorners[1] = transform.TransformPoint(x, num4, 0f);
		mCorners[2] = transform.TransformPoint(num3, y, 0f);
		mCorners[3] = transform.TransformPoint(x, num2, 0f);
		if (relativeTo != null)
		{
			for (int i = 0; i < 4; i++)
			{
				mCorners[i] = relativeTo.InverseTransformPoint(mCorners[i]);
			}
		}
		return mCorners;
	}

	public override float CalculateFinalAlpha(int frameID)
	{
#if UNITY_EDITOR
		if (mAlphaFrameID != frameID || !Application.isPlaying)
#else
		if (mAlphaFrameID != frameID)
#endif
		{
			mAlphaFrameID = frameID;
			UpdateFinalAlpha(frameID);
		}
		return finalAlpha;
	}

	protected void UpdateFinalAlpha(int frameID)
	{
		if (!mIsVisibleByAlpha || !mIsInFront)
		{
			finalAlpha = 0f;
		}
		else if (isCalculateFinalAlpha)
		{
			finalAlpha = ((!(parent != null)) ? mColor.a : (parent.CalculateFinalAlpha(frameID) * mColor.a));
		}
		else
		{
			finalAlpha = mColor.a;
		}
	}

	public override void Invalidate(bool includeChildren)
	{
		mChanged = true;
		mAlphaFrameID = -1;
		if (panel != null)
		{
			bool visibleByPanel = (!hideIfOffScreen && !panel.hasCumulativeClipping) || panel.IsVisible(this);
			UpdateVisibility(CalculateCumulativeAlpha(Time.frameCount) > 0.001f, visibleByPanel);
			UpdateFinalAlpha(Time.frameCount);
			if (includeChildren)
			{
				base.Invalidate(true);
			}
		}
	}

	public float CalculateCumulativeAlpha(int frameID)
	{
		UIRect uIRect = parent;
		return (!(uIRect != null)) ? mColor.a : (uIRect.CalculateFinalAlpha(frameID) * mColor.a);
	}

	public override void SetRect(float x, float y, float width, float height)
	{
		Vector2 vector = pivotOffset;
		float num = Mathf.Lerp(x, x + width, vector.x);
		float num2 = Mathf.Lerp(y, y + height, vector.y);
		int num3 = Mathf.FloorToInt(width + 0.5f);
		int num4 = Mathf.FloorToInt(height + 0.5f);
		if (vector.x == 0.5f)
		{
			num3 = num3 >> 1 << 1;
		}
		if (vector.y == 0.5f)
		{
			num4 = num4 >> 1 << 1;
		}
		Transform transform = cachedTransform;
		Vector3 localPosition = transform.localPosition;
		localPosition.x = Mathf.Floor(num + 0.5f);
		localPosition.y = Mathf.Floor(num2 + 0.5f);
		if (num3 < minWidth)
		{
			num3 = minWidth;
		}
		if (num4 < minHeight)
		{
			num4 = minHeight;
		}
		transform.localPosition = localPosition;
		this.width = num3;
		this.height = num4;
		if (isAnchored)
		{
			transform = transform.parent;
			if ((bool)leftAnchor.target)
			{
				leftAnchor.SetHorizontal(transform, x);
			}
			if ((bool)rightAnchor.target)
			{
				rightAnchor.SetHorizontal(transform, x + width);
			}
			if ((bool)bottomAnchor.target)
			{
				bottomAnchor.SetVertical(transform, y);
			}
			if ((bool)topAnchor.target)
			{
				topAnchor.SetVertical(transform, y + height);
			}
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
	}

	public void ResizeCollider()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null)
		{
			NGUITools.UpdateWidgetCollider(this, component);
		}
		else
		{
			NGUITools.UpdateWidgetCollider(this, GetComponent<BoxCollider2D>());
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static int FullCompareFunc(UIWidget left, UIWidget right)
	{
		int num = UIPanel.CompareFunc(left.panel, right.panel);
		return (num != 0) ? num : PanelCompareFunc(left, right);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static int PanelCompareFunc(UIWidget left, UIWidget right)
	{
		if (left.mDepth < right.mDepth)
		{
			return -1;
		}
		if (left.mDepth > right.mDepth)
		{
			return 1;
		}
		Material material = left.material;
		Material material2 = right.material;
		if (material == material2)
		{
			return 0;
		}
		if (material == null)
		{
			return 1;
		}
		if (material2 == null)
		{
			return -1;
		}
		return (material.GetInstanceID() >= material2.GetInstanceID()) ? 1 : (-1);
	}

	public Bounds CalculateBounds()
	{
		return CalculateBounds(null);
	}

	public Bounds CalculateBounds(Transform relativeParent)
	{
		if (relativeParent == null)
		{
			Vector3[] array = localCorners;
			Bounds result = new Bounds(array[0], Vector3.zero);
			for (int i = 1; i < 4; i++)
			{
				result.Encapsulate(array[i]);
			}
			return result;
		}
		Matrix4x4 worldToLocalMatrix = relativeParent.worldToLocalMatrix;
		Vector3[] array2 = worldCorners;
		Bounds result2 = new Bounds(worldToLocalMatrix.MultiplyPoint3x4(array2[0]), Vector3.zero);
		for (int j = 1; j < 4; j++)
		{
			result2.Encapsulate(worldToLocalMatrix.MultiplyPoint3x4(array2[j]));
		}
		return result2;
	}

	public void SetDirty()
	{
		if (drawCall != null)
		{
			drawCall.isDirty = true;
		}
		else if (isVisible && hasVertices)
		{
			CreatePanel();
		}
	}

	public void RemoveFromPanel()
	{
		if (panel != null)
		{
			panel.RemoveWidget(this);
			panel = null;
		}
		drawCall = null;
#if UNITY_EDITOR
		mOldTex = null;
		mOldShader = null;
#endif
	}

#if UNITY_EDITOR
	[System.NonSerialized] Texture mOldTex;
	[System.NonSerialized] Shader mOldShader;

	/// <summary>
	/// This callback is sent inside the editor notifying us that some property has changed.
	/// </summary>

	protected override void OnValidate()
	{
		if (NGUITools.GetActive(this))
		{
			base.OnValidate();

			// Prior to NGUI 2.7.0 width and height was specified as transform's local scale
			if ((mWidth == 100 || mWidth == minWidth) &&
				(mHeight == 100 || mHeight == minHeight) && cachedTransform.localScale.magnitude > 8f)
			{
				UpgradeFrom265();
				cachedTransform.localScale = Vector3.one;
			}

			if (mWidth < minWidth) mWidth = minWidth;
			if (mHeight < minHeight) mHeight = minHeight;
			if (autoResizeBoxCollider) ResizeCollider();

			// If the texture is changing, we need to make sure to rebuild the draw calls
			if (mOldTex != mainTexture || mOldShader != shader)
			{
				mOldTex = mainTexture;
				mOldShader = shader;
			}

			aspectRatio = (keepAspectRatio == AspectRatioSource.Free) ?
				(float)mWidth / mHeight : Mathf.Max(0.01f, aspectRatio);

			if (keepAspectRatio == AspectRatioSource.BasedOnHeight)
			{
				mWidth = Mathf.RoundToInt(mHeight * aspectRatio);
			}
			else if (keepAspectRatio == AspectRatioSource.BasedOnWidth)
			{
				mHeight = Mathf.RoundToInt(mWidth / aspectRatio);
			}

			if (!Application.isPlaying)
			{
				if (panel != null)
				{
					panel.RemoveWidget(this);
					panel = null;
				}
				CreatePanel();
			}
		}
		else
		{
			if (mWidth < minWidth) mWidth = minWidth;
			if (mHeight < minHeight) mHeight = minHeight;
		}
	}
#endif

	public virtual void MarkAsChanged()
	{
		if (NGUITools.GetActive(this))
		{
			mChanged = true;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
			if (panel != null && enabled && NGUITools.GetActive(gameObject) && !mPlayMode)
			{
				SetDirty();
				CheckLayer();
#if UNITY_EDITOR
				// Mark the panel as dirty so it gets updated
				if (material != null) NGUITools.SetDirty(panel.gameObject);
#endif
			}
		}
	}

	public UIPanel CreatePanel()
	{
		if (mStarted && panel == null && enabled && NGUITools.GetActive(cachedGameObject))
		{
			panel = UIPanel.Find(cachedTransform, true, cachedGameObject.layer);
			if (panel != null)
			{
				mParentFound = false;
				panel.AddWidget(this);
				CheckLayer();
				Invalidate(true);
			}
		}
		return panel;
	}

	public void CheckLayer()
	{
		if (panel != null && panel.cachedGameObject.layer != cachedGameObject.layer)
		{
			UnityEngine.Debug.LogWarning("You can't place widgets on a layer different than the UIPanel that manages them.\nIf you want to move widgets to a different layer, parent them to a new panel instead.", this);
			cachedGameObject.layer = panel.cachedGameObject.layer;
		}
	}

	public override void ParentHasChanged()
	{
		base.ParentHasChanged();
		if (panel != null)
		{
			UIPanel uIPanel = UIPanel.Find(cachedTransform, true, cachedGameObject.layer);
			if (panel != uIPanel)
			{
				RemoveFromPanel();
				CreatePanel();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mPlayMode = Application.isPlaying;
	}

	protected override void OnInit()
	{
		base.OnInit();
		RemoveFromPanel();
		mMoved = true;
		nUpdate(Time.frameCount);
#if UNITY_EDITOR
		NGUITools.SetDirty(this);
#endif
	}

	protected virtual void UpgradeFrom265()
	{
		Vector3 localScale = cachedTransform.localScale;
		mWidth = Mathf.Abs(Mathf.RoundToInt(localScale.x));
		mHeight = Mathf.Abs(Mathf.RoundToInt(localScale.y));
		NGUITools.UpdateWidgetCollider(gameObject, true);
	}

	protected override void OnStart()
	{
#if UNITY_EDITOR
		if (GetComponent<UIPanel>() != null)
		{
			UnityEngine.Debug.LogError("Widgets and panels should not be on the same object! Widget must be a child of the panel.", this);
		}
		else if (!Application.isPlaying && GetComponents<UIWidget>().Length > 1)
		{
			UnityEngine.Debug.LogError("You should not place more than one widget on the same object. Weird stuff will happen!", this);
		}
#endif
		cachedTransform.localPosition = new Vector3(cachedTransform.localPosition.x, cachedTransform.localPosition.y, (float)mDepth * -0.01f);
		CreatePanel();
	}

	protected override void OnAnchor()
	{
		nProfiler.BeginSample("UIWidget.OnAnchor");
		Transform transform = cachedTransform;
		Transform transform2 = transform.parent;
		Vector3 localPosition = transform.localPosition;
		Vector2 vector = pivotOffset;
		nProfiler.BeginSample("1");
		float num;
		float num2;
		float num3;
		float num4;
		if (leftAnchor.target == bottomAnchor.target && leftAnchor.target == rightAnchor.target && leftAnchor.target == topAnchor.target)
		{
			nProfiler.BeginSample("1");
			nProfiler.BeginSample("1");
			Vector3[] sides = leftAnchor.GetSides(transform2);
			nProfiler.EndSample();
			if (sides != null)
			{
				nProfiler.BeginSample("2");
				num = NGUIMath.Lerp(sides[0].x, sides[2].x, leftAnchor.relative) + (float)leftAnchor.absolute;
				num2 = NGUIMath.Lerp(sides[0].x, sides[2].x, rightAnchor.relative) + (float)rightAnchor.absolute;
				num3 = NGUIMath.Lerp(sides[3].y, sides[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute;
				num4 = NGUIMath.Lerp(sides[3].y, sides[1].y, topAnchor.relative) + (float)topAnchor.absolute;
				mIsInFront = true;
				nProfiler.EndSample();
			}
			else
			{
				nProfiler.BeginSample("3");
				Vector3 localPos = GetLocalPos(leftAnchor, transform2);
				num = localPos.x + (float)leftAnchor.absolute;
				num3 = localPos.y + (float)bottomAnchor.absolute;
				num2 = localPos.x + (float)rightAnchor.absolute;
				num4 = localPos.y + (float)topAnchor.absolute;
				mIsInFront = !hideIfOffScreen || localPos.z >= 0f;
				nProfiler.EndSample();
			}
			nProfiler.EndSample();
		}
		else
		{
			mIsInFront = true;
			if ((bool)leftAnchor.target)
			{
				Vector3[] sides2 = leftAnchor.GetSides(transform2);
				num = ((sides2 == null) ? (GetLocalPos(leftAnchor, transform2).x + (float)leftAnchor.absolute) : (NGUIMath.Lerp(sides2[0].x, sides2[2].x, leftAnchor.relative) + (float)leftAnchor.absolute));
			}
			else
			{
				num = localPosition.x - vector.x * (float)mWidth;
			}
			if ((bool)rightAnchor.target)
			{
				Vector3[] sides3 = rightAnchor.GetSides(transform2);
				num2 = ((sides3 == null) ? (GetLocalPos(rightAnchor, transform2).x + (float)rightAnchor.absolute) : (NGUIMath.Lerp(sides3[0].x, sides3[2].x, rightAnchor.relative) + (float)rightAnchor.absolute));
			}
			else
			{
				num2 = localPosition.x - vector.x * (float)mWidth + (float)mWidth;
			}
			if ((bool)bottomAnchor.target)
			{
				Vector3[] sides4 = bottomAnchor.GetSides(transform2);
				num3 = ((sides4 == null) ? (GetLocalPos(bottomAnchor, transform2).y + (float)bottomAnchor.absolute) : (NGUIMath.Lerp(sides4[3].y, sides4[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute));
			}
			else
			{
				num3 = localPosition.y - vector.y * (float)mHeight;
			}
			if ((bool)topAnchor.target)
			{
				Vector3[] sides5 = topAnchor.GetSides(transform2);
				num4 = ((sides5 == null) ? (GetLocalPos(topAnchor, transform2).y + (float)topAnchor.absolute) : (NGUIMath.Lerp(sides5[3].y, sides5[1].y, topAnchor.relative) + (float)topAnchor.absolute));
			}
			else
			{
				num4 = localPosition.y - vector.y * (float)mHeight + (float)mHeight;
			}
		}
		nProfiler.EndSample();
		nProfiler.BeginSample("2");
		Vector3 vector2 = new Vector3(Mathf.Lerp(num, num2, vector.x), Mathf.Lerp(num3, num4, vector.y), localPosition.z);
		vector2.x = Mathf.Round(vector2.x);
		vector2.y = Mathf.Round(vector2.y);
		int num5 = Mathf.FloorToInt(num2 - num + 0.5f);
		int num6 = Mathf.FloorToInt(num4 - num3 + 0.5f);
		if (keepAspectRatio != 0 && aspectRatio != 0f)
		{
			if (keepAspectRatio == AspectRatioSource.BasedOnHeight)
			{
				num5 = Mathf.RoundToInt((float)num6 * aspectRatio);
			}
			else
			{
				num6 = Mathf.RoundToInt((float)num5 / aspectRatio);
			}
		}
		if (num5 < minWidth)
		{
			num5 = minWidth;
		}
		if (num6 < minHeight)
		{
			num6 = minHeight;
		}
		if (Vector3.SqrMagnitude(localPosition - vector2) > 0.001f)
		{
			cachedTransform.localPosition = vector2;
			if (mIsInFront)
			{
				mChanged = true;
			}
		}
		if (mWidth != num5 || mHeight != num6)
		{
			mWidth = num5;
			mHeight = num6;
			if (mIsInFront)
			{
				mChanged = true;
			}
			if (autoResizeBoxCollider)
			{
				ResizeCollider();
			}
		}
		nProfiler.EndSample();
		nProfiler.EndSample();
	}

	protected override void OnUpdate()
	{
		if (panel == null)
		{
			CreatePanel();
		}
#if UNITY_EDITOR
		else if (!mPlayMode) ParentHasChanged();
#endif
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			MarkAsChanged();
		}
	}

	protected override void OnDisable()
	{
		RemoveFromPanel();
		base.OnDisable();
	}

	private void OnDestroy()
	{
		RemoveFromPanel();
	}

#if UNITY_EDITOR
	static int mHandles = -1;

	/// <summary>
	/// Whether widgets will show handles with the Move Tool, or just the View Tool.
	/// </summary>

	static public bool showHandlesWithMoveTool
	{
		get
		{
			if (mHandles == -1)
			{
				mHandles = UnityEditor.EditorPrefs.GetInt("NGUI Handles", 1);
			}
			return (mHandles == 1);
		}
		set
		{
			int val = value ? 1 : 0;

			if (mHandles != val)
			{
				mHandles = val;
				UnityEditor.EditorPrefs.SetInt("NGUI Handles", mHandles);
			}
		}
	}

	/// <summary>
	/// Whether the widget should have some form of handles shown.
	/// </summary>

	static public bool showHandles
	{
		get
		{
#if UNITY_4_3 || UNITY_4_5
			if (showHandlesWithMoveTool)
			{
				return UnityEditor.Tools.current == UnityEditor.Tool.Move;
			}
			return UnityEditor.Tools.current == UnityEditor.Tool.View;
#else
			return UnityEditor.Tools.current == UnityEditor.Tool.Rect;
#endif
		}
	}

	/// <summary>
	/// Draw some selectable gizmos.
	/// </summary>

	void OnDrawGizmos()
	{
		if (isVisible && NGUITools.GetActive(this))
		{
			if (UnityEditor.Selection.activeGameObject == gameObject && showHandles) return;

			Color outline = new Color(1f, 1f, 1f, 0.2f);

			float adjustment = (root != null) ? 0.05f : 0.001f;
			Vector2 offset = pivotOffset;
			Vector3 center = new Vector3(mWidth * (0.5f - offset.x), mHeight * (0.5f - offset.y), -mDepth * adjustment);
			Vector3 size = new Vector3(mWidth, mHeight, 1f);

			// Draw the gizmo
			Gizmos.matrix = cachedTransform.localToWorldMatrix;
			Gizmos.color = (UnityEditor.Selection.activeGameObject == cachedTransform) ? Color.white : outline;
			Gizmos.DrawWireCube(center, size);

			// Make the widget selectable
			size.z = 0.01f;
			Gizmos.color = Color.clear;
			Gizmos.DrawCube(center, size);
		}
	}
#endif // UNITY_EDITOR

	public bool UpdateVisibility(bool visibleByAlpha, bool visibleByPanel)
	{
		if (mIsVisibleByAlpha != visibleByAlpha || mIsVisibleByPanel != visibleByPanel)
		{
			mChanged = true;
			mIsVisibleByAlpha = visibleByAlpha;
			mIsVisibleByPanel = visibleByPanel;
			return true;
		}
		return false;
	}

	[ContextMenu("Update Wigdet")]
	public void UpdateWidget()
	{
		if (!widgetUpdateFrame)
		{
			widgetUpdateFrame = true;
		}
		if (panel != null && panel.widgetAreStatic)
		{
			panel.widgetUpdateFrame = true;
		}
	}

	public bool UpdateTransform()
	{
		if (!isActiveAndEnabled)
		{
			return true;
		}
		mMoved = true;
		return UpdateTransform(Time.frameCount);
	}

	public bool UpdateTransform(int frame)
	{
		mPlayMode = Application.isPlaying;
#if UNITY_EDITOR
		if (mMoved || !mPlayMode)
#else
		if (mMoved)
#endif
		{
			mMoved = true;
			mMatrixFrame = -1;
			cachedTransform.hasChanged = false;
			Vector2 vector = pivotOffset;
			float num = (0f - vector.x) * (float)mWidth;
			float num2 = (0f - vector.y) * (float)mHeight;
			float x = num + (float)mWidth;
			float y = num2 + (float)mHeight;
			mOldV0 = panel.worldToLocal.MultiplyPoint3x4(cachedTransform.TransformPoint(num, num2, 0f));
			mOldV1 = panel.worldToLocal.MultiplyPoint3x4(cachedTransform.TransformPoint(x, y, 0f));
		}
		else if (cachedTransform.hasChanged)
		{
			mMatrixFrame = -1;
			cachedTransform.hasChanged = false;
			Vector2 vector2 = pivotOffset;
			float num3 = (0f - vector2.x) * (float)mWidth;
			float num4 = (0f - vector2.y) * (float)mHeight;
			float x2 = num3 + (float)mWidth;
			float y2 = num4 + (float)mHeight;
			Vector3 vector3 = panel.worldToLocal.MultiplyPoint3x4(cachedTransform.TransformPoint(num3, num4, 0f));
			Vector3 vector4 = panel.worldToLocal.MultiplyPoint3x4(cachedTransform.TransformPoint(x2, y2, 0f));
			if (Vector3.SqrMagnitude(mOldV0 - vector3) > 1E-06f || Vector3.SqrMagnitude(mOldV1 - vector4) > 1E-06f)
			{
				mMoved = true;
				mOldV0 = vector3;
				mOldV1 = vector4;
			}
		}
		if (mMoved && onChange != null)
		{
			onChange();
		}
		return mMoved || mChanged;
	}

	public bool UpdateGeometry(int frame)
	{
		float num = CalculateFinalAlpha(frame);
		if (mIsVisibleByAlpha && mLastAlpha != num)
		{
			mChanged = true;
		}
		mLastAlpha = num;
		if (mChanged)
		{
			if (mIsVisibleByAlpha && num > 0.001f && shader != null)
			{
				bool result = geometry.hasVertices;
				if (fillGeometry)
				{
					geometry.Clear();
					OnFill(geometry.verts, geometry.uvs, geometry.cols);
				}
				if (geometry.hasVertices)
				{
					if (mMatrixFrame != frame)
					{
						mLocalToPanel = panel.worldToLocal * cachedTransform.localToWorldMatrix;
						mMatrixFrame = frame;
					}
					geometry.ApplyTransform(mLocalToPanel, panel.generateNormals);
					mMoved = false;
					mChanged = false;
					return true;
				}
				mChanged = false;
				return result;
			}
			if (geometry.hasVertices)
			{
				if (fillGeometry)
				{
					geometry.Clear();
				}
				mMoved = false;
				mChanged = false;
				return true;
			}
		}
		else if (mMoved && geometry.hasVertices)
		{
			if (mMatrixFrame != frame)
			{
				mLocalToPanel = panel.worldToLocal * cachedTransform.localToWorldMatrix;
				mMatrixFrame = frame;
			}
			geometry.ApplyTransform(mLocalToPanel, panel.generateNormals);
			mMoved = false;
			mChanged = false;
			return true;
		}
		mMoved = false;
		mChanged = false;
		return false;
	}

	public void WriteToBuffers(List<Vector3> v, List<Vector2> u, List<Color> c, List<Vector3> n, List<Vector4> t, List<Vector4> u2)
	{
		geometry.WriteToBuffers(v, u, c, n, t, u2);
	}

	public virtual void MakePixelPerfect()
	{
		Vector3 localPosition = cachedTransform.localPosition;
		localPosition.z = Mathf.Round(localPosition.z);
		localPosition.x = Mathf.Round(localPosition.x);
		localPosition.y = Mathf.Round(localPosition.y);
		cachedTransform.localPosition = localPosition;
		Vector3 localScale = cachedTransform.localScale;
		cachedTransform.localScale = new Vector3(Mathf.Sign(localScale.x), Mathf.Sign(localScale.y), 1f);
	}

	public virtual void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
	}
}
