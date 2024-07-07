using UnityEngine;

public class TrapEnable : MonoBehaviour
{
	[Range(1f, 30f)]
	public int Key = 1;

	public GameObject Target;

	public bool setStartPosition;

	public Vector3 startPosition;

	public bool Value;

	public float DelayIn;

	public float DelayOut = 3f;

	private bool Activated;

	private void Start()
	{
		if (Target == null)
		{
			Target = gameObject;
		}
		if (setStartPosition)
		{
			transform.position = startPosition;
		}
		EventManager.AddListener("StartRound", StartRound);
		EventManager.AddListener("WaitPlayer", StartRound);
		EventManager.AddListener("Button" + Key, ActiveTrap);
	}

	[ContextMenu("Get Start Position")]
	private void GetStart()
	{
		startPosition = transform.position;
	}

	[ContextMenu("Get Transform")]
	private void GetTransform()
	{
		Target = gameObject;
	}

	private void ActiveTrap()
	{
		if (Activated)
		{
			return;
		}
		Activated = true;
		TimerManager.In(DelayIn, delegate
		{
			Target.SetActive(Value);
			if (DelayOut != 0f)
			{
				TimerManager.In(DelayOut, delegate
				{
					if (Activated)
					{
						Target.SetActive(!Value);
					}
				});
			}
		});
	}

	private void StartRound()
	{
		Target.SetActive(!Value);
		Activated = false;
	}
}
