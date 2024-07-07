using UnityEngine;

public class InputManager : MonoBehaviour
{
	public struct ButtonData
	{
		public string name;

		public KeyCode key;
	}

	public struct AxisData
	{
		public string name;

		public bool raw;
	}

	public delegate void ButtonDelegate(string key);

	public delegate void AxisDelegate(string key, float value);

	private static BetterList<ButtonData> buttons = new BetterList<ButtonData>();

	private static BetterList<AxisData> axis = new BetterList<AxisData>();

	public static ButtonDelegate GetButtonDownEvent;

	public static ButtonDelegate GetButtonEvent;

	public static ButtonDelegate GetButtonUpEvent;

	public static AxisDelegate GetAxisEvent;

	private static InputManager instance;

	private void Start()
	{
		instance = this;
	    DontDestroyOnLoad(gameObject);
	}

	private void AddButton(string name, KeyCode key)
	{
		ButtonData item = default(ButtonData);
		item.name = name;
		item.key = key;
		buttons.Add(item);
	}

	private void AddAxis(string name, bool raw)
	{
		AxisData item = default(AxisData);
		item.name = name;
		item.raw = raw;
		axis.Add(item);
	}

	public static void Init()
	{
		if (instance == null)
		{
			GameObject gameObject = new GameObject("InputManager");
			gameObject.AddComponent<InputManager>();
		}
	}
	
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private void Update()
    {
        if (GameObject.Find("Display") != null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                SetButtonDown("Fire");
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                SetButtonUp("Fire");
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                SetButtonDown("Aim");
                UIRoot.uiroot.gameObject.SetActive(false);
                UIRoot.uiroot.gameObject.SetActive(true);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                SetButtonUp("Aim");
                UIRoot.uiroot.gameObject.SetActive(false);
                UIRoot.uiroot.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                SetButtonDown("Chat");
            }
            if (Input.GetKeyUp(KeyCode.T))
            {
                SetButtonUp("Chat");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                SetButtonDown("Use");
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                SetButtonUp("Use");
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                SetButtonDown("Store");
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                SetButtonUp("Store");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                SetButtonDown("Bomb");
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                SetButtonUp("Bomb");
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SetButtonDown("Reload");
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            SetButtonUp("Reload");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetButtonDown("Jump");
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            SetButtonUp("Jump");
        }

        SetAxis("Mouse X", Input.GetAxis("Mouse X"));
        SetAxis("Mouse Y", Input.GetAxis("Mouse Y"));
    }
#endif

	public static void SetButtonDown(string name)
	{
		if (GetButtonDownEvent != null)
		{
			GetButtonDownEvent(name);
		}
	}

	public static void SetButtonUp(string name)
	{
		if (GetButtonUpEvent != null)
		{
			GetButtonUpEvent(name);
		}
	}

	public static void SetAxis(string name, float value)
	{
		if (GetAxisEvent != null)
		{
			GetAxisEvent(name, value);
		}
	}
}
