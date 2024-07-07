using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("NGUI/UI/Viewport Camera")]
public class UIViewport : MonoBehaviour
{
	public Camera sourceCamera;

	public Transform topLeft;

	public Transform bottomRight;

	public float fullSize = 1f;

	private Camera mCam;

	private void Start()
	{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		mCam = camera;
#else
		mCam = GetComponent<Camera>();
#endif
		if (sourceCamera == null)
		{
			sourceCamera = Camera.main;
		}
	}

	private void LateUpdate()
	{
		if (!(topLeft != null) || !(bottomRight != null))
		{
			return;
		}
		if (topLeft.gameObject.activeInHierarchy)
		{
			Vector3 vector = sourceCamera.WorldToScreenPoint(topLeft.position);
			Vector3 vector2 = sourceCamera.WorldToScreenPoint(bottomRight.position);
			Rect rect = new Rect(vector.x / (float)Screen.width, vector2.y / (float)Screen.height, (vector2.x - vector.x) / (float)Screen.width, (vector.y - vector2.y) / (float)Screen.height);
			float num = fullSize * rect.height;
			if (rect != mCam.rect)
			{
				mCam.rect = rect;
			}
			if (mCam.orthographicSize != num)
			{
				mCam.orthographicSize = num;
			}
			mCam.enabled = true;
		}
		else
		{
			mCam.enabled = false;
		}
	}
}
