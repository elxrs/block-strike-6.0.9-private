using UnityEngine;
using UnityEngine.Events;

namespace BSCM.Game.Others
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
#endif
	public class EventsTrigger : MonoBehaviour
	{
		public UnityEvent TriggerEnter;

		public UnityEvent TriggerExit;

#if UNITY_EDITOR
		private BoxCollider boxCollider;

		[ContextMenu("Invoke Trigger Enter")]
		void InvokeTriggerEnter()
		{
			if (!Application.isPlaying)
				return;
			if (TriggerEnter != null)
				TriggerEnter.Invoke();
		}

		[ContextMenu("Invoke Trigger Exit")]
		void InvokeTriggerExit()
		{
			if (!Application.isPlaying)
				return;
			if (TriggerExit != null)
				TriggerExit.Invoke();
		}

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
			EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
			eventTrigger.TriggerEnter = TriggerEnter;
			eventTrigger.TriggerExit = TriggerExit;
		}
	}
}
