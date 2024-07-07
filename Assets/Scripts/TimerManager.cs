using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
	private class Event
	{
		public int ID;

		public string tag;

		public Callback Function;

		public int Iterations = 1;

		public float Interval = -1f;

		public float StartTime;

		public float DueTime;

		public bool CancelOnLoad = true;

		public void Execute()
		{
			if (ID == 0 || DueTime == 0f)
			{
				Recycle();
			}
			else if (Function != null)
			{
				try
				{
					Function();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				if (Iterations > 0)
				{
					Iterations--;
					if (Iterations < 1)
					{
						Recycle();
						return;
					}
				}
				DueTime = Time.time + Interval;
			}
			else
			{
				Recycle();
			}
		}

		public void Recycle()
		{
			ID = 0;
			DueTime = 0f;
			StartTime = 0f;
			CancelOnLoad = true;
			Function = null;
			if (instance.list.Remove(this))
			{
				instance.pool.Add(this);
			}
		}
	}

	public delegate void Callback();

	public static float time;

	private BetterList<Event> list = new BetterList<Event>();

	private BetterList<Event> pool = new BetterList<Event>();

	public static TimerDelegate OnUpdate;

	public static bool stopOnUpdate;

	public int eventCount;

	private static TimerManager instance;

	private void Awake()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	private static void Init()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif
		if (instance == null)
		{
			GameObject gameObject = new GameObject("TimerManager");
			gameObject.AddComponent<TimerManager>();
		}
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif
		stopOnUpdate = false;
		time = Time.time;
		for (int i = 0; i < list.size; i++)
		{
			if (list.size > i && list.buffer[i] != null && (time >= list.buffer[i].DueTime || list.buffer[i].ID == 0))
			{
				list.buffer[i].Execute();
			}
		}
		if (OnUpdate != null)
		{
			OnUpdate();
		}
	}

	public static int Start()
	{
		return Start(true);
	}

	public static int Start(bool cancelOnLoad)
	{
		return In(315360000f, cancelOnLoad, null);
	}

	public static float GetDuration(int id)
	{
		for (int i = 0; i < instance.list.size; i++)
		{
			if (instance.list.buffer[i].ID == id)
			{
				return Time.time - instance.list.buffer[i].StartTime;
			}
		}
		return 0f;
	}

	public static int In(float delay, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		return In(string.Empty, delay, true, 1, 0f, callback);
	}

	public static int In(string tag, float delay, Callback callback)
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return 0;
        }
#endif
		return In(tag, delay, true, 1, 0f, callback);
	}

	public static int In(float delay, bool cancelOnLoad, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		return In(string.Empty, delay, cancelOnLoad, 1, 0f, callback);
	}

	public static int In(string tag, float delay, bool cancelOnLoad, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		return In(tag, delay, cancelOnLoad, 1, 0f, callback);
	}

	public static int In(float delay, int iterations, float interval, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		return In(string.Empty, delay, true, iterations, interval, callback);
	}

	public static int In(string tag, float delay, int iterations, float interval, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		return In(tag, delay, true, iterations, interval, callback);
	}

	public static int In(float delay, bool cancelOnLoad, int iterations, float interval, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		return In(string.Empty, delay, true, iterations, interval, callback);
	}

	public static int In(string tag, float delay, bool cancelOnLoad, int iterations, float interval, Callback callback)
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return 0;
		}
#endif
		Init();
		delay = Mathf.Max(0f, delay);
		iterations = Mathf.Max(0, iterations);
		interval = Mathf.Max(0f, interval);
		Event @event = null;
		if (instance.pool.size > 0)
		{
			@event = instance.pool.buffer[0];
			instance.pool.Remove(@event);
		}
		else
		{
			@event = new Event();
		}
		instance.eventCount++;
		@event.ID = instance.eventCount;
		@event.tag = tag;
		@event.Function = callback;
		@event.Iterations = iterations;
		@event.Interval = interval;
		@event.StartTime = Time.time;
		@event.DueTime = Time.time + delay;
		@event.CancelOnLoad = cancelOnLoad;
		instance.list.Add(@event);
		return @event.ID;
	}

	public static void Cancel(int id)
	{
		if (0 >= id)
		{
			return;
		}
		for (int num = instance.list.size - 1; num > -1; num--)
		{
			if (instance.list.buffer[num].ID == id)
			{
				instance.list.buffer[num].ID = 0;
				break;
			}
		}
	}

	public static void Cancel(params int[] ids)
	{
		if (ids == null || ids.Length <= 0)
		{
			return;
		}
		for (int num = instance.list.size - 1; num > -1; num--)
		{
			if (ids.Contains(instance.list.buffer[num].ID))
			{
				instance.list.buffer[num].ID = 0;
			}
		}
	}

	public static void Cancel(string tag)
	{
		if (string.IsNullOrEmpty(tag))
		{
			return;
		}
		for (int num = instance.list.size - 1; num > -1; num--)
		{
			if (instance.list.buffer[num].tag == tag)
			{
				instance.list.buffer[num].ID = 0;
			}
		}
	}

	public static void CancelAll()
	{
		for (int num = instance.list.size - 1; num > -1; num--)
		{
			instance.list.buffer[num].ID = 0;
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		for (int num = list.size - 1; num > -1; num--)
		{
			if (list.buffer[num].CancelOnLoad)
			{
				list.buffer[num].ID = 0;
			}
		}
	}

	public static bool IsActive(int id)
	{
		for (int num = instance.list.size - 1; num > -1; num--)
		{
			if (instance.list.buffer[num].ID == id)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsActive(string tag)
	{
		for (int num = instance.list.size - 1; num > -1; num--)
		{
			if (instance.list.buffer[num].tag == tag)
			{
				return true;
			}
		}
		return false;
	}
}

