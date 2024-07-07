using System.Collections;
using UnityEngine;

public class UIFontControl : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(GenerateFont());
	}

	private IEnumerator GenerateFont()
	{
        LevelManager.LoadLevel("Logo");
		yield break;
	}
}
