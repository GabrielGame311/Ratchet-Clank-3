#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class OpenSaveFolder
{
    [MenuItem("Tools/Öppna Sparfilsmapp")]
    public static void OpenFolder()
    {
        // Öppnar utforskaren direkt där Application.persistentDataPath pekar
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
#endif