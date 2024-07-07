using UnityEngine;

public class PlayerSkinRagdoll : MonoBehaviour
{
	public static float force;

	public Rigidbody[] playerLimbs;

	public PlayerSkinAtlas playerAtlas;

	public Transform playerRightWeaponRoot;

	public Transform playerLeftWeaponRoot;

	[HideInInspector]
	public Vector3 defaultPosition;

	private Transform mTransform;

	private GameObject mGameObject;

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

	public GameObject cachedGameObject
	{
		get
		{
			if (mGameObject == null)
			{
				mGameObject = gameObject;
			}
			return mGameObject;
		}
	}

	public void SetSkin(UIAtlas atlas, string head, string body, string legs)
	{
		if (playerAtlas != null)
		{
			playerAtlas.atlas = atlas;
			playerAtlas.SetSprite("0-" + head, "1-" + body, "2-" + legs);
		}
	}

	public void Active(Vector3 f, Transform[] tr)
	{
		playerAtlas.skinnedMeshRenderer.enabled = true;
		for (int i = 0; i < tr.Length; i++)
		{
			playerLimbs[i].position = tr[i].position;
			playerLimbs[i].rotation = tr[i].rotation;
			playerLimbs[i].isKinematic = false;
			playerLimbs[i].velocity = Vector3.zero;
			playerLimbs[i].AddForce(f * force);
			try
			{
				BoxCollider collider = FindObjectOfType<PlayerTriggerDetector>().GetComponent<BoxCollider>();
				if (playerLimbs[i].gameObject.GetComponent<BoxCollider>() != null)
				{
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<BoxCollider>(), PlayerInput.instance.mCharacterController);
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<BoxCollider>(), collider);
				}
				if (playerLimbs[i].gameObject.GetComponent<CapsuleCollider>() != null)
				{
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<CapsuleCollider>(), PlayerInput.instance.mCharacterController);
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<CapsuleCollider>(), collider);
				}
				if (playerLimbs[i].gameObject.GetComponent<SphereCollider>() != null)
				{
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<SphereCollider>(), PlayerInput.instance.mCharacterController);
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<SphereCollider>(), collider);
				}
			}
			catch
			{

			}
		}
	}

	public void Deactive()
	{
		playerAtlas.skinnedMeshRenderer.enabled = false;
		for (int i = 0; i < playerLimbs.Length; i++)
		{
			playerLimbs[i].isKinematic = true;
		}
		playerLimbs[0].position = defaultPosition;
	}
}
