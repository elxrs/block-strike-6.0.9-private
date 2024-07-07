using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/NGUI Scroll Bar")]
[ExecuteInEditMode]
public class UIScrollBar : UISlider
{
	private enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded
	}

	[SerializeField]
	[HideInInspector]
	protected float mSize = 1f;

	[SerializeField]
	[HideInInspector]
	private float mScroll;

	[HideInInspector]
	[SerializeField]
	private Direction mDir = Direction.Upgraded;

	[Obsolete("Use 'value' instead")]
	public float scrollValue
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

	public float barSize
	{
		get
		{
			return mSize;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mSize == num)
			{
				return;
			}
			mSize = num;
			mIsDirty = true;
			if (NGUITools.GetActive(this))
			{
				if (current == null && onChange != null)
				{
					current = this;
					EventDelegate.Execute(onChange);
					current = null;
				}
				ForceUpdate();
#if UNITY_EDITOR
				if (!Application.isPlaying)
					NGUITools.SetDirty(this);
#endif
			}
		}
	}

	protected override void Upgrade()
	{
		if (mDir != Direction.Upgraded)
		{
			mValue = mScroll;
			if (mDir == Direction.Horizontal)
			{
				mFill = (mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight);
			}
			else
			{
				mFill = ((!mInverted) ? FillDirection.TopToBottom : FillDirection.BottomToTop);
			}
			mDir = Direction.Upgraded;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (mFG != null && mFG.gameObject != gameObject)
		{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			bool hasCollider = (mFG.collider != null) || (mFG.GetComponent<Collider2D>() != null);
#else
			bool hasCollider = (mFG.GetComponent<Collider>() != null) || (mFG.GetComponent<Collider2D>() != null);
#endif
			if (!hasCollider) return;
			UIEventListener uIEventListener = UIEventListener.Get(mFG.gameObject);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressForeground));
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragForeground));
			mFG.autoResizeBoxCollider = true;
		}
	}

	protected override float LocalToValue(Vector2 localPos)
	{
		if (mFG != null)
		{
			float num = Mathf.Clamp01(mSize) * 0.5f;
			float t = num;
			float t2 = 1f - num;
			Vector3[] localCorners = mFG.localCorners;
			if (isHorizontal)
			{
				t = Mathf.Lerp(localCorners[0].x, localCorners[2].x, t);
				t2 = Mathf.Lerp(localCorners[0].x, localCorners[2].x, t2);
				float num2 = t2 - t;
				if (num2 == 0f)
				{
					return value;
				}
				return (!isInverted) ? ((localPos.x - t) / num2) : ((t2 - localPos.x) / num2);
			}
			t = Mathf.Lerp(localCorners[0].y, localCorners[1].y, t);
			t2 = Mathf.Lerp(localCorners[3].y, localCorners[2].y, t2);
			float num3 = t2 - t;
			if (num3 == 0f)
			{
				return value;
			}
			return (!isInverted) ? ((localPos.y - t) / num3) : ((t2 - localPos.y) / num3);
		}
		return base.LocalToValue(localPos);
	}

	public override void ForceUpdate()
	{
		if (mFG != null)
		{
			mIsDirty = false;
			float num = Mathf.Clamp01(mSize) * 0.5f;
			float num2 = Mathf.Lerp(num, 1f - num, value);
			float num3 = num2 - num;
			float num4 = num2 + num;
			if (isHorizontal)
			{
				mFG.drawRegion = ((!isInverted) ? new Vector4(num3, 0f, num4, 1f) : new Vector4(1f - num4, 0f, 1f - num3, 1f));
			}
			else
			{
				mFG.drawRegion = ((!isInverted) ? new Vector4(0f, num3, 1f, num4) : new Vector4(0f, 1f - num4, 1f, 1f - num3));
			}
			if (thumb != null)
			{
				Vector4 drawingDimensions = mFG.drawingDimensions;
				Vector3 position = new Vector3(Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, 0.5f), Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, 0.5f));
				SetThumbPosition(mFG.cachedTransform.TransformPoint(position));
			}
		}
		else
		{
			base.ForceUpdate();
		}
	}
}
