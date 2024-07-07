using UnityEngine;

public class AutoRotate : MonoBehaviour
{
	public Vector3 Rotate;

	private Transform Target;

	private void Start()
	{
		Target = transform;
	}

	private void Update()
	{
		Target.Rotate(Rotate);
	}
}
