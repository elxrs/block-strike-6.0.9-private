using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag and Drop Root")]
public class UIDragDropRoot : MonoBehaviour
{
	public static Transform root;

	private void OnEnable()
	{
		root = transform;
	}

	private void OnDisable()
	{
		if (root == transform)
		{
			root = null;
		}
	}
}
