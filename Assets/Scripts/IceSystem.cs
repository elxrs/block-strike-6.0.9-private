using UnityEngine;

public class IceSystem : MonoBehaviour
{
	private BoxCollider boxCollider;

	private Bounds bounds;

	private bool secure;

	private void Start()
	{
		gameObject.layer = 2;
		boxCollider = GetComponent<BoxCollider>();
		if (!(boxCollider == null))
		{
			bounds = boxCollider.bounds;
			secure = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (component == null)
		{
			return;
		}
		if (secure)
		{
			if (bounds.Intersects(component.mCharacterController.bounds))
			{
				component.SetMoveIce(true);
			}
		}
		else
		{
			component.SetMoveIce(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			component.SetMoveIce(false);
		}
	}
}
