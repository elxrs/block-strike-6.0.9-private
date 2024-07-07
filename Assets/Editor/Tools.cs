using UnityEngine;
using UnityEditor;

public class Tools : MonoBehaviour
{
    [MenuItem("Tools/PlayerPrefs/DeleteAll")]
    static void PFD(MenuCommand command)
    {
        PlayerPrefs.DeleteAll();
    }
}
