using UnityEngine;

namespace BSCM.Game.Modes.DeathRun
{
	public class Button : MonoBehaviour 
	{
		public global::Team team = global::Team.Red;

		[Range(1,50)]
		public int key;

		private void Start()
		{
			if (!Application.isPlaying)
				return;
			if (PhotonNetwork.room.GetGameMode() != global::GameMode.DeathRun)
			{
				return;
			}
			TrapButton button = gameObject.AddComponent<TrapButton>();
			button.PlayerTeam = team;
			button.Key = key;
		}
	}
}
