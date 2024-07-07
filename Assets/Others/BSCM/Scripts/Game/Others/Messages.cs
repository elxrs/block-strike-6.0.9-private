using UnityEngine;

namespace BSCM.Game.Others
{
	public class Messages : MonoBehaviour 
	{
		public string[] messages;

		[Range(5,60)]
		public int[] delay;
	}
}
