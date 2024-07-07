using UnityEngine;

namespace BSCM.Game.Others
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
#endif
	public class Water : MonoBehaviour
	{
		public bool free;

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
			gameObject.AddComponent<WaterSystem>().freeGravity = free;
		}
	}
}
