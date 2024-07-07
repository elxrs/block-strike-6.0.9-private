using UnityEngine;

namespace BSCM.Game.Modes.BunnyHop
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
#endif
	public class FinishTrigger : MonoBehaviour
	{
#if UNITY_EDITOR
		private BoxCollider boxCollider;
		
		void Update()
		{
			if (boxCollider == null)
			{
				boxCollider = GetComponent<BoxCollider>();
				if (boxCollider == null)
					boxCollider = gameObject.AddComponent<BoxCollider>();
			}
			boxCollider.isTrigger = true;
		}
#endif
		private void Start()
		{
			if (!Application.isPlaying)
				return;
			if (PhotonNetwork.room.GetGameMode() == global::GameMode.BunnyHop)
			{
				gameObject.AddComponent<BunnySpawn>().FinishSpawn = true;
			}
		}
	}
}
