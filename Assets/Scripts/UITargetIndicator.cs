using System;
using UnityEngine;

public class UITargetIndicator : MonoBehaviour
{
	public Transform Target;

	public UISprite Indicator;

	public Camera MainCamera;

	public float Limit = 0.9f;

	public float MaxDistance = 5f;

	public float MinDistance = 2f;

	private Vector2 center;

	private Vector2 LimitRect;

	private Vector2 LimitRect2;

	private Transform MainCameraTransform;

	private static UITargetIndicator instance;

	private void Start()
	{
		instance = this;
		if (MainCamera != null)
		{
			MainCameraTransform = MainCamera.transform;
		}
		center = new Vector2(400f, 240f);
		LimitRect = new Vector2((1f - Limit) * 400f, (1f - Limit) * 240f);
		LimitRect2 = new Vector2(800f - LimitRect.x, 480f - LimitRect.y);
	}

	public static void SetTarget(Transform target)
	{
		instance.Target = target;
	}

	public static void SetCamera(Camera cam)
	{
		instance.MainCamera = cam;
		instance.MainCameraTransform = cam.transform;
	}

	private void LateUpdate()
	{
		float num = Vector3.Distance(MainCameraTransform.position, Target.position);
		if (MaxDistance + MinDistance > num)
		{
			Indicator.alpha = (num - MinDistance) / MaxDistance;
			if (Indicator.alpha == 0f)
			{
				return;
			}
		}
		Vector3 vector = MainCamera.WorldToScreenPoint(Target.position);
		if (vector.z > 0f && vector.x > LimitRect.x && vector.x < LimitRect2.x && vector.y > LimitRect.y && vector.y < LimitRect2.y)
		{
			Indicator.cachedTransform.localPosition = new Vector3(vector.x - center.x, vector.y - center.y, 0f);
			return;
		}
		if (vector.z < 0f)
		{
			vector.z *= -1f;
		}
		vector.x -= center.x;
		vector.y -= center.y;
		float num2 = Mathf.Atan2(vector.y, vector.x);
		num2 -= (float)Math.PI / 2f;
		float num3 = Mathf.Cos(num2);
		float num4 = 0f - Mathf.Sin(num2);
		vector = center + new Vector2(num4 * 150f, num3 * 150f);
		float num5 = num3 / num4;
		Vector3 vector2 = center * Limit;
		vector = ((!(num3 > 0f)) ? new Vector3((0f - vector2.y) / num5, 0f - vector2.y, 0f) : new Vector3(vector2.y / num5, vector2.y, 0f));
		if (vector.x > vector2.x)
		{
			vector = new Vector3(vector2.x, vector2.x * num5, 0f);
		}
		else if (vector.x < 0f - vector2.x)
		{
			vector = new Vector3(0f - vector2.x, (0f - vector2.x) * num5, 0f);
		}
		Indicator.cachedTransform.localPosition = vector;
	}
}
