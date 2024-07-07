using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag Camera")]
[ExecuteInEditMode]
public class UIDragCamera : MonoBehaviour
{
	public UIDraggableCamera draggableCamera;

	private void Awake()
	{
		if (draggableCamera == null)
		{
			draggableCamera = NGUITools.FindInParents<UIDraggableCamera>(gameObject);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (enabled && NGUITools.GetActive(gameObject) && draggableCamera != null && draggableCamera.enabled)
		{
			draggableCamera.Press(isPressed);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (enabled && NGUITools.GetActive(gameObject) && draggableCamera != null && draggableCamera.enabled)
		{
			draggableCamera.Drag(delta);
		}
	}

	private void OnScroll(float delta)
	{
		if (enabled && NGUITools.GetActive(gameObject) && draggableCamera != null && draggableCamera.enabled)
		{
			draggableCamera.Scroll(delta);
		}
	}
}
