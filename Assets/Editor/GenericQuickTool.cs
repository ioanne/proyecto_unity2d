using UnityEngine;
using UnityEditor;
using System.IO;

public class GenericQuickTool : EditorWindow
{
    private string prefix = "GEN_";
    private bool applySpriteRenderer = true;
    private Sprite spriteToApply;
    private string customSavePath = "";

    [MenuItem("Tools/Generic Quick Tool")]
    public static void ShowWindow()
    {
        GetWindow<GenericQuickTool>("Generic Quick Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generic Quick Tool", EditorStyles.boldLabel);

        prefix = EditorGUILayout.TextField("Prefix for Renaming", prefix);

        GUILayout.Space(5);
        applySpriteRenderer = EditorGUILayout.Toggle("Add SpriteRenderer", applySpriteRenderer);
        if (applySpriteRenderer)
        {
            spriteToApply = (Sprite)EditorGUILayout.ObjectField("Sprite to Apply", spriteToApply, typeof(Sprite), false);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Select Save Folder"))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Folder to Save Generated Files", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    customSavePath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a folder inside the Assets directory.", "OK");
                }
            }
        }

        if (!string.IsNullOrEmpty(customSavePath))
        {
            EditorGUILayout.HelpBox($"Save Path: {customSavePath}", MessageType.Info);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Process Selected Objects"))
        {
            if (string.IsNullOrEmpty(customSavePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a folder to save before processing.", "OK");
                return;
            }
            ProcessSelectedObjects();
        }

        if (GUILayout.Button("Log Selected Object Names"))
        {
            LogSelectedObjectNames();
        }
    }

    private void ProcessSelectedObjects()
    {
        var selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", "Select at least one GameObject in the scene.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Confirm Processing",
            $"Objects will be renamed and modified.\nAssets will be saved in:\n{customSavePath}",
            "Proceed",
            "Cancel"
        );

        if (!confirm)
            return;

        foreach (GameObject obj in selectedObjects)
        {
            Undo.RecordObject(obj, "Generic Quick Tool Changes");

            // Rename with prefix
            obj.name = prefix + obj.name;

            // Add SpriteRenderer if needed
            if (applySpriteRenderer && obj.GetComponent<SpriteRenderer>() == null)
            {
                var sr = obj.AddComponent<SpriteRenderer>();
                if (spriteToApply != null)
                {
                    sr.sprite = spriteToApply;
                }
            }

            // Snap Z position to 0
            var pos = obj.transform.position;
            obj.transform.position = new Vector3(pos.x, pos.y, 0);

            // Optionally save prefab (extendable)
            string localPath = Path.Combine(customSavePath, obj.name + ".prefab").Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, localPath, InteractionMode.UserAction);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", "Selected objects processed and saved as prefabs successfully!", "OK");
    }

    private void LogSelectedObjectNames()
    {
        var selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.Log("No objects selected.");
            return;
        }

        Debug.Log("Selected Objects:");
        foreach (var obj in selectedObjects)
        {
            Debug.Log(obj.name);
        }
    }
}
