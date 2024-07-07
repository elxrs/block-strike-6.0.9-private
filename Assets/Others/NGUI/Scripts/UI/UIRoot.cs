﻿using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Root")]
[RequireComponent(typeof(UIUpdateManager))]
[ExecuteInEditMode]
public class UIRoot : MonoBehaviour
{
	[DoNotObfuscateNGUI]
	public enum Scaling
	{
		Flexible,
		Constrained,
		ConstrainedOnMobiles
	}

	[DoNotObfuscateNGUI]
	public enum Constraint
	{
		Fit,
		Fill,
		FitWidth,
		FitHeight
	}

	public static List<UIRoot> list = new List<UIRoot>();

	public Scaling scalingStyle;

	public int manualWidth = 1280;

	public int manualHeight = 720;

	public int minimumHeight = 320;

	public int maximumHeight = 1536;

	public bool fitWidth;

	public bool fitHeight = true;

	public bool adjustByDPI;

	public bool shrinkPortraitUI;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	public static UIRoot uiroot;
#endif

	public static UIRoot instance;

	private Transform mTrans;

	public Constraint constraint
	{
		get
		{
			if (fitWidth)
			{
				if (fitHeight)
				{
					return Constraint.Fit;
				}
				return Constraint.FitWidth;
			}
			if (fitHeight)
			{
				return Constraint.FitHeight;
			}
			return Constraint.Fill;
		}
	}

	public Scaling activeScaling
	{
		get
		{
			Scaling scaling = scalingStyle;

			if (scaling == Scaling.ConstrainedOnMobiles)
				return Scaling.Constrained;
			return scaling;
		}
	}

	public int activeHeight
	{
		get
		{
			if (activeScaling == Scaling.Flexible)
			{
				Vector2 screenSize = NGUITools.screenSize;
				float num = screenSize.x / screenSize.y;
				if (screenSize.y < (float)minimumHeight)
				{
					screenSize.y = minimumHeight;
					screenSize.x = screenSize.y * num;
				}
				else if (screenSize.y > (float)maximumHeight)
				{
					screenSize.y = maximumHeight;
					screenSize.x = screenSize.y * num;
				}
				int num2 = Mathf.RoundToInt((!shrinkPortraitUI || !(screenSize.y > screenSize.x)) ? screenSize.y : (screenSize.y / num));
				return (!adjustByDPI) ? num2 : NGUIMath.AdjustByDPI(num2);
			}
			Constraint constraint = this.constraint;
			if (constraint == Constraint.FitHeight)
			{
				return manualHeight;
			}
			Vector2 screenSize2 = NGUITools.screenSize;
			float num3 = screenSize2.x / screenSize2.y;
			float num4 = (float)manualWidth / (float)manualHeight;
			switch (constraint)
			{
				case Constraint.FitWidth:
					return Mathf.RoundToInt((float)manualWidth / num3);
				case Constraint.Fit:
					return (!(num4 > num3)) ? manualHeight : Mathf.RoundToInt((float)manualWidth / num3);
				case Constraint.Fill:
					return (!(num4 < num3)) ? manualHeight : Mathf.RoundToInt((float)manualWidth / num3);
				default:
					return manualHeight;
			}
		}
	}

	public float pixelSizeAdjustment
	{
		get
		{
			int num = Mathf.RoundToInt(NGUITools.screenSize.y);
			return (num != -1) ? GetPixelSizeAdjustment(num) : 1f;
		}
	}

	public static float GetPixelSizeAdjustment(GameObject go)
	{
		UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(go);
		return (!(uIRoot != null)) ? 1f : uIRoot.pixelSizeAdjustment;
	}

	public float GetPixelSizeAdjustment(int height)
	{
		height = Mathf.Max(2, height);
		if (activeScaling == Scaling.Constrained)
		{
			return (float)activeHeight / (float)height;
		}
		if (height < minimumHeight)
		{
			return (float)minimumHeight / (float)height;
		}
		if (height > maximumHeight)
		{
			return (float)maximumHeight / (float)height;
		}
		return 1f;
	}

	protected virtual void Awake()
	{
		mTrans = transform;
	}

	protected virtual void OnEnable()
	{
		list.Add(this);
		instance = this;
#if UNITY_EDITOR
		uiroot = this;
#endif
	}

	protected virtual void OnDisable()
	{
		list.Remove(this);
	}

	protected virtual void Start()
	{
		UIOrthoCamera componentInChildren = GetComponentInChildren<UIOrthoCamera>();
		if (componentInChildren != null)
		{
			Debug.LogWarning("UIRoot should not be active at the same time as UIOrthoCamera. Disabling UIOrthoCamera.", componentInChildren);
			Camera component = componentInChildren.gameObject.GetComponent<Camera>();
			componentInChildren.enabled = false;
			if (component != null)
			{
				component.orthographicSize = 1f;
			}
		}
		else
		{
			UpdateScale(false);
			Invoke("UpdateScale", 0.5f);
		}
	}

	void Update()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying && gameObject.layer != 0)
			UnityEditor.EditorPrefs.SetInt("NGUI Layer", gameObject.layer);
#endif
		UpdateScale();
	}

	public void UpdateScale()
	{
		UpdateScale(true);
	}

	public void UpdateScale(bool updateAnchors)
	{
		if (!(mTrans != null))
		{
			return;
		}
		float num = activeHeight;
		if (!(num > 0f))
		{
			return;
		}
		float num2 = 2f / num;
		Vector3 localScale = mTrans.localScale;
		if (!(Mathf.Abs(localScale.x - num2) <= float.Epsilon) || !(Mathf.Abs(localScale.y - num2) <= float.Epsilon) || !(Mathf.Abs(localScale.z - num2) <= float.Epsilon))
		{
			mTrans.localScale = new Vector3(num2, num2, num2);
			if (updateAnchors)
			{
				BroadcastMessage("UpdateAnchors", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void Broadcast(string funcName)
	{
		int i = 0;
#if UNITY_EDITOR
		if (Application.isPlaying)
#endif
		{
			for (int count = list.Count; i < count; i++)
			{
				UIRoot uIRoot = list[i];
				if (uIRoot != null)
				{
					uIRoot.BroadcastMessage(funcName, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	public static void Broadcast(string funcName, object param)
	{
		if (param == null)
		{
			Debug.LogError("SendMessage is bugged when you try to pass 'null' in the parameter field. It behaves as if no parameter was specified.");
			return;
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIRoot uIRoot = list[i];
			if (uIRoot != null)
			{
				uIRoot.BroadcastMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	[ContextMenu("Fix Anchor")]
	private void FixAnchor()
	{
		UIAnchor[] componentsInChildren = GetComponentsInChildren<UIAnchor>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].uiCamera == null)
			{
				componentsInChildren[i].uiCamera = componentsInChildren[i].GetComponentInParent<Camera>();
				Debug.Log(componentsInChildren[i].name, componentsInChildren[i].gameObject);
			}
		}
	}
}
