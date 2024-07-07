using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	private Transform mTransform;

	public Transform cachedTransform
	{
		get
		{
			if (mTransform == null)
			{
				mTransform = transform;
			}
			return mTransform;
		}
	}

	public Vector3 spawnPosition
	{
		get
		{
			Vector3 position = cachedTransform.position;
			position.x += Random.Range((0f - spawnScale.x) / 2f, spawnScale.x / 2f);
			position.z += Random.Range((0f - spawnScale.z) / 2f, spawnScale.z / 2f);
			return position;
		}
		set
		{
			cachedTransform.position = value;
		}
	}

	public Vector3 spawnRotation
	{
		get
		{
			return cachedTransform.eulerAngles;
		}
		set
		{
			cachedTransform.eulerAngles = value;
		}
	}

	public Vector3 spawnScale
	{
		get
		{
			return cachedTransform.localScale;
		}
		set
		{
			cachedTransform.localScale = value;
		}
	}
	
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (gameObject.name == "Red")
        {
            Gizmos.color = Color.red - new Color(0f, 0f, 0f, 0.2f);
        }
		if (gameObject.name == "Blue")
		{
            Gizmos.color = Color.blue - new Color(0f, 0f, 0f, 0.2f);
        }
		if (gameObject.name == "StaticPoint")
        {
			Gizmos.color = new Color(0.78f, 0f, 1f, 1f);
		}
		bool objGreen = false;
		foreach (char c in gameObject.name)
		{
			if (char.IsDigit(c))
			{
				objGreen = true;
				break;
			}
		}
		if (objGreen)
        {
            Gizmos.color = Color.green - new Color(0f, 0f, 0f, 0.3f);
        }
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
        Gizmos.DrawCube(transform.position, spawnScale);
    }
#endif
}
