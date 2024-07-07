using UnityEngine;

public class TrapAutoRotation : MonoBehaviour
{
	public Vector3 rotate;

	private Transform mTransform;

	private void Start()
	{
		mTransform = transform;
	}

	private void Update()
	{
		mTransform.Rotate(rotate);
	}
}
