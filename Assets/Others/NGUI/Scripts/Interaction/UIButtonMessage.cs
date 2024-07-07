using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UIButtonMessage : MonoBehaviour
{
	[DoNotObfuscateNGUI]
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick
	}

	public GameObject target;

	public string functionName;

	public Trigger trigger;

	public bool includeChildren;

	private bool mStarted;

	private void Start()
	{
		mStarted = true;
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			OnHover(UICamera.IsHighlighted(gameObject));
		}
	}

	private void OnHover(bool isOver)
	{
		if (enabled && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
		{
			Send();
		}
	}

	private void OnPress(bool isPressed)
	{
		if (enabled && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
		{
			Send();
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
		{
			OnHover(isSelected);
		}
	}

	private void OnClick()
	{
		if (enabled && trigger == Trigger.OnClick)
		{
			Send();
		}
	}

	private void OnDoubleClick()
	{
		if (enabled && trigger == Trigger.OnDoubleClick)
		{
			Send();
		}
	}

	private void Send()
	{
		if (string.IsNullOrEmpty(functionName))
		{
			return;
		}
		if (target == null)
		{
			target = gameObject;
		}
		if (includeChildren)
		{
			Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				Transform transform = componentsInChildren[i];
				transform.gameObject.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}
