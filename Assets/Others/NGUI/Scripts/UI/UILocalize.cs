using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Localize")]
[RequireComponent(typeof(UIWidget))]
[ExecuteInEditMode]
public class UILocalize : MonoBehaviour
{
	public string key;

	public string addon;

	private UILabel lbl;

	private bool mStarted;

	public string value
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (lbl == null)
				{
					lbl = GetComponent<UILabel>();
				}
				lbl.text = value;
			}
		}
	}

	private void OnEnable()
	{
		Localization.onLocalize = (Localization.OnLocalizeNotification)Delegate.Combine(Localization.onLocalize, new Localization.OnLocalizeNotification(OnLocalize));
		if (mStarted)
		{
			OnLocalize();
		}
	}

	private void OnDisable()
	{
		Localization.onLocalize = (Localization.OnLocalizeNotification)Delegate.Remove(Localization.onLocalize, new Localization.OnLocalizeNotification(OnLocalize));
	}

	private void Start()
	{
		mStarted = true;
		OnLocalize();
	}

	private void OnLocalize()
	{
		if (string.IsNullOrEmpty(key))
		{
			UILabel component = GetComponent<UILabel>();
			if (component != null)
			{
				key = component.text;
			}
		}
		if (!string.IsNullOrEmpty(key))
		{
			value = Localization.Get(key) + addon;
		}
	}
}
