using UnityEngine;

namespace BSCM.Game.Modes.DeathRun
{
	public class SetActive : MonoBehaviour 
	{
		[Range(1,50)]
		public int key = 1;

		public GameObject target;

		public bool value;

		public float delayIn;

		public float delayOut = 3;

		private void Start()
		{
			if (!Application.isPlaying)
				return;
			if (PhotonNetwork.room.GetGameMode() != global::GameMode.DeathRun)
			{
				return;
			}
			TrapEnable trap = gameObject.AddComponent<TrapEnable>();
			trap.Key = key;
			trap.Target = target;
			trap.Value = value;
			trap.DelayIn = delayIn;
			trap.DelayOut = delayOut;
		}
	}
}
