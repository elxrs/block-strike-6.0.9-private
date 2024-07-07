using UnityEngine;

public class sTweenRotation : MonoBehaviour
{
	public Vector3 speed = Vector3.forward;

	private Transform mTransform;

	private Vector3 startEulerAngles;

	public bool changePosition;

	public Vector3 position;

	private void Start()
	{
		if (changePosition)
		{
			transform.localPosition = position;
		}
		mTransform = transform;
		startEulerAngles = transform.localEulerAngles;
	}

	private void FixedUpdate()
	{
		mTransform.localEulerAngles = startEulerAngles + speed * Time.time * 10f;
	}

	[ContextMenu("Get Position")]
	private void GetValue()
	{
		position = transform.localPosition;
	}
}
