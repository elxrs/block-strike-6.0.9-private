using UnityEngine;
using UnityEngine.Events;

namespace CodeStage.AntiCheat.Detectors
{
	[AddComponentMenu("")]
	public abstract class ActDetectorBase : MonoBehaviour
	{
		protected const string CONTAINER_NAME = "Anti-Cheat Toolkit Detectors";

		protected const string MENU_PATH = "Code Stage/Anti-Cheat Toolkit/";

		protected const string GAME_OBJECT_MENU_PATH = "GameObject/Create Other/Code Stage/Anti-Cheat Toolkit/";

		protected static GameObject detectorsContainer;

		[Tooltip("Automatically start detector. Detection Event will be called on detection.")]
		public bool autoStart = true;

		[Tooltip("Detector will survive new level (scene) load if checked.")]
		public bool keepAlive = true;

		[Tooltip("Automatically dispose Detector after firing callback.")]
		public bool autoDispose = true;

		[SerializeField]
		protected UnityEvent detectionEvent;

		protected UnityAction detectionAction;

		[SerializeField]
		protected bool detectionEventHasListener;

		protected bool isRunning;

		protected bool started;

		private void Start()
		{
			if (detectorsContainer == null && gameObject.name == "Anti-Cheat Toolkit Detectors")
			{
				detectorsContainer = gameObject;
			}
			if (autoStart && !started)
			{
				StartDetectionAutomatically();
			}
		}

		private void OnEnable()
		{
			if (started && (detectionEventHasListener || detectionAction != null || DetectorHasAdditionalCallbacks()))
			{
				ResumeDetector();
			}
		}

		private void OnDisable()
		{
			if (started)
			{
				PauseDetector();
			}
		}

		private void OnApplicationQuit()
		{
			DisposeInternal();
		}

		protected virtual void OnDestroy()
		{
			StopDetectionInternal();
			if (transform.childCount == 0 && GetComponentsInChildren<Component>().Length <= 2)
			{
				Destroy(gameObject);
			}
			else if (name == "Anti-Cheat Toolkit Detectors" && GetComponentsInChildren<ActDetectorBase>().Length <= 1)
			{
				Destroy(gameObject);
			}
		}

		protected virtual bool Init(ActDetectorBase instance, string detectorName)
		{
			if (instance != null && instance != this && instance.keepAlive)
			{
				Debug.LogWarning("[ACTk] " + name + ": self-destroying, other instance already exists & only one instance allowed!", gameObject);
				Destroy(this);
				return false;
			}
			DontDestroyOnLoad(gameObject);
			return true;
		}

		protected virtual void DisposeInternal()
		{
			Destroy(this);
		}

		protected virtual bool DetectorHasAdditionalCallbacks()
		{
			return false;
		}

		internal virtual void OnCheatingDetected()
		{
			if (detectionAction != null)
			{
				detectionAction();
			}
			if (detectionEventHasListener)
			{
				detectionEvent.Invoke();
			}
			if (autoDispose)
			{
				DisposeInternal();
			}
			else
			{
				StopDetectionInternal();
			}
		}

		protected abstract void StartDetectionAutomatically();

		protected abstract void StopDetectionInternal();

		protected abstract void PauseDetector();

		protected abstract void ResumeDetector();
	}
}
