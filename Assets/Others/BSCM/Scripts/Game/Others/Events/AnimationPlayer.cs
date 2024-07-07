using UnityEngine;

namespace BSCM.Game.Others
{
	public class AnimationPlayer : MonoBehaviour 
	{
		public Animation target;

		public void Play(string clip)
		{
#if UNITY_EDITOR
			if (target != null)
				target.Play(clip);
#endif
		}

		public void PlayQueued(string clip)
		{
#if UNITY_EDITOR
			if (target != null)
				target.PlayQueued(clip);
#endif
		}

		public void Stop()
		{
#if UNITY_EDITOR
			if (target != null)
				target.Stop();
#endif
		}
	}
}
