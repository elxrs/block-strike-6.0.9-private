using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Toggled Objects")]
public class UIToggledObjects : MonoBehaviour
{
	public List<GameObject> activate;

	public List<GameObject> deactivate;

	[HideInInspector]
	[SerializeField]
	private GameObject target;

	[SerializeField]
	[HideInInspector]
	private bool inverse;

	private void Awake()
	{
		if (target != null)
		{
			if (activate.Count == 0 && deactivate.Count == 0)
			{
				if (inverse)
				{
					deactivate.Add(target);
				}
				else
				{
					activate.Add(target);
				}
			}
			else
			{
				target = null;
			}
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		UIToggle component = GetComponent<UIToggle>();
		EventDelegate.Add(component.onChange, Toggle);
	}

	public void Toggle()
	{
		bool value = UIToggle.current.value;
		if (enabled)
		{
			for (int i = 0; i < activate.Count; i++)
			{
				Set(activate[i], value);
			}
			for (int j = 0; j < deactivate.Count; j++)
			{
				Set(deactivate[j], !value);
			}
		}
	}

	private void Set(GameObject go, bool state)
	{
		if (go != null)
		{
			NGUITools.SetActive(go, state);
		}
	}
}
