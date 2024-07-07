﻿using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class ClearSceneEditor 
{
	static ClearSceneEditor()
	{
		EditorSceneManager.newSceneCreated += SceneOpen;
	}

	private static void SceneOpen(Scene scene, NewSceneSetup setup, NewSceneMode mode)
	{
		RenderSettings.skybox = null;
		GameObject go = GameObject.Find("Directional Light");
		if (go != null)
		{
            Object.DestroyImmediate(go);
		}
        go = GameObject.Find("Main Camera");
        if (go != null)
        {
			Object.DestroyImmediate(go);
        }
    }
}
