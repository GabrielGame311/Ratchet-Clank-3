using UnityEngine;
using UnityEditor;

public class AutoAssignTextures : EditorWindow
{
    [MenuItem("Tools/Koppla Texturer På Markerade")]
    public static void Assign()
    {
        // Hämtar ENDAST de material som du har markerat i projektfönstret
        Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.Assets);
        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            Material mat = obj as Material;
            if (mat != null && mat.name.Contains("_"))
            {
                // Plockar ut numret (t.ex. "746" från "Material_25_746")
                int lastUnderscore = mat.name.LastIndexOf('_');
                string texNumber = mat.name.Substring(lastUnderscore + 1);

                // Söker efter texturen i projektet
                string[] texGuids = AssetDatabase.FindAssets(texNumber + " t:Texture");

                if (texGuids.Length > 0)
                {
                    string texPath = AssetDatabase.GUIDToAssetPath(texGuids[0]);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

                    if (tex != null)
                    {
                        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                        else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);

                        EditorUtility.SetDirty(mat);
                        count++;
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Klart! {count} markerade material har kopplats.");
    }
}