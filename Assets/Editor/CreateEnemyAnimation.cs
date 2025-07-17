using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class EnemyAnimatorAutoGenerator : EditorWindow
{
    private Sprite spriteSheet;
    private string baseName = "Enemy";
    private float walkFrameRate = 8f;
    private float idleFrameRate = 1f;

    private int columns = 4;
    private int rowsToUse = 3;

    private string customSavePath = "";

    [MenuItem("Tools/Auto Generate Enemy Animator")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAnimatorAutoGenerator>("Auto Generate Enemy Animator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Generator for Enemy Animator", EditorStyles.boldLabel);
        spriteSheet = (Sprite)EditorGUILayout.ObjectField("Sprite Sheet (Sliced)", spriteSheet, typeof(Sprite), false);
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        walkFrameRate = EditorGUILayout.FloatField("Walk Animation FPS", walkFrameRate);
        idleFrameRate = EditorGUILayout.FloatField("Idle Animation FPS", idleFrameRate);

        GUILayout.Space(5);
        GUILayout.Label("Sprite Sheet Layout", EditorStyles.boldLabel);
        columns = EditorGUILayout.IntField("Columns (Horizontal)", columns);
        rowsToUse = EditorGUILayout.IntField("Rows to Use (Vertical)", rowsToUse);

        GUILayout.Space(10);

        if (GUILayout.Button("Select Save Folder"))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Folder to Save Animations and Controller", "Assets", "");
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

        if (GUILayout.Button("Generate All"))
        {
            if (spriteSheet == null)
            {
                EditorUtility.DisplayDialog("Error", "Assign a sliced sprite sheet.", "OK");
                return;
            }
            GenerateEverything();
        }
    }

    private void GenerateEverything()
    {
        string folderPath = !string.IsNullOrEmpty(customSavePath) ? customSavePath : GetSelectedPathOrFallback();
        string animFolder = Path.Combine(folderPath, baseName + "_Animations");
        Directory.CreateDirectory(animFolder);
        AssetDatabase.Refresh();

        bool confirm = EditorUtility.DisplayDialog(
            "Confirm",
            $"All animations and AnimatorController will be saved in:\n{animFolder}",
            "Proceed",
            "Cancel"
        );

        if (!confirm)
            return;

        string spritePath = AssetDatabase.GetAssetPath(spriteSheet);
        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name, new NaturalSortComparer())
            .ToArray();

        int expectedSprites = columns * rowsToUse;
        if (sprites.Length < expectedSprites)
        {
            EditorUtility.DisplayDialog("Error", $"Expected at least {expectedSprites} sprites (Columns x RowsToUse). Check slicing.", "OK");
            return;
        }

        // Step 1: Create and save all AnimationClips first
        Dictionary<string, string> idleClipPaths = new()
        {
            ["Down"] = $"{animFolder}/{baseName}_IdleDown.anim",
            ["Left"] = $"{animFolder}/{baseName}_IdleLeft.anim",
            ["Right"] = $"{animFolder}/{baseName}_IdleRight.anim",
            ["Up"] = $"{animFolder}/{baseName}_IdleUp.anim"
        };

        Dictionary<string, string> walkClipPaths = new()
        {
            ["Down"] = $"{animFolder}/{baseName}_WalkDown.anim",
            ["Left"] = $"{animFolder}/{baseName}_WalkLeft.anim",
            ["Right"] = $"{animFolder}/{baseName}_WalkRight.anim",
            ["Up"] = $"{animFolder}/{baseName}_WalkUp.anim"
        };

        CreateSingleFrameClip(sprites[0], idleClipPaths["Down"]);
        CreateSingleFrameClip(sprites[columns], idleClipPaths["Left"]);
        CreateSingleFrameClip(sprites[columns * 2], idleClipPaths["Right"]);
        CreateSingleFrameClip(rowsToUse >= 4 ? sprites[columns * 3] : sprites[0], idleClipPaths["Up"]);

        CreateWalkClip(GetRowSprites(sprites, 0), walkClipPaths["Down"]);
        CreateWalkClip(GetRowSprites(sprites, 1), walkClipPaths["Left"]);
        CreateWalkClip(GetRowSprites(sprites, 2), walkClipPaths["Right"]);
        CreateWalkClip(rowsToUse >= 4 ? GetRowSprites(sprites, 3) : GetRowSprites(sprites, 0), walkClipPaths["Up"]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Step 2: Load clips from disk to ensure correct references
        Dictionary<string, AnimationClip> idleClips = new()
        {
            ["Down"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPaths["Down"]),
            ["Left"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPaths["Left"]),
            ["Right"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPaths["Right"]),
            ["Up"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPaths["Up"])
        };

        Dictionary<string, AnimationClip> walkClips = new()
        {
            ["Down"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkClipPaths["Down"]),
            ["Left"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkClipPaths["Left"]),
            ["Right"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkClipPaths["Right"]),
            ["Up"] = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkClipPaths["Up"])
        };

        // Step 3: Create AnimatorController
        string controllerPath = Path.Combine(animFolder, $"{baseName}_Animator.controller").Replace("\\", "/");
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        controller.AddParameter("moveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("moveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("isMoving", AnimatorControllerParameterType.Bool);

        var rootStateMachine = controller.layers[0].stateMachine;

        AnimatorState idleTreeState = rootStateMachine.AddState("IdleTree");
        BlendTree idleTree = CreatePersistentBlendTree(controller, "IdleTree");
        idleTreeState.motion = idleTree;
        idleTree.blendType = BlendTreeType.SimpleDirectional2D;
        idleTree.blendParameter = "moveX";
        idleTree.blendParameterY = "moveY";
        idleTree.useAutomaticThresholds = false;

        idleTree.AddChild(idleClips["Up"], new Vector2(0, 1));
        idleTree.AddChild(idleClips["Right"], new Vector2(1, 0));
        idleTree.AddChild(idleClips["Down"], new Vector2(0, -1));
        idleTree.AddChild(idleClips["Left"], new Vector2(-1, 0));

        AnimatorState walkTreeState = rootStateMachine.AddState("WalkTree");
        BlendTree walkTree = CreatePersistentBlendTree(controller, "WalkTree");
        walkTreeState.motion = walkTree;
        walkTree.blendType = BlendTreeType.SimpleDirectional2D;
        walkTree.blendParameter = "moveX";
        walkTree.blendParameterY = "moveY";
        walkTree.useAutomaticThresholds = false;

        walkTree.AddChild(walkClips["Up"], new Vector2(0, 1));
        walkTree.AddChild(walkClips["Right"], new Vector2(1, 0));
        walkTree.AddChild(walkClips["Down"], new Vector2(0, -1));
        walkTree.AddChild(walkClips["Left"], new Vector2(-1, 0));

        var toWalk = idleTreeState.AddTransition(walkTreeState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
        toWalk.hasExitTime = false;

        var toIdle = walkTreeState.AddTransition(idleTreeState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
        toIdle.hasExitTime = false;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "AnimatorController and animations generated successfully!", "OK");
    }

    private Sprite[] GetRowSprites(Sprite[] sprites, int rowIndex)
    {
        Sprite[] row = new Sprite[3];
        for (int i = 0; i < 3; i++)
        {
            row[i] = sprites[rowIndex * columns + i];
        }
        return row;
    }

    private AnimationClip CreateSingleFrameClip(Sprite sprite, string path)
    {
        AnimationClip clip = new AnimationClip { frameRate = idleFrameRate };

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[1];
        keyFrames[0] = new ObjectReferenceKeyframe { time = 0, value = sprite };

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);

        EnableLoop(clip);
        AssetDatabase.CreateAsset(clip, path.Replace("\\", "/"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return clip;
    }

    private AnimationClip CreateWalkClip(Sprite[] walkSprites, string path)
    {
        AnimationClip clip = new AnimationClip { frameRate = walkFrameRate };

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[walkSprites.Length + 1];
        for (int i = 0; i < walkSprites.Length; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe { time = i / walkFrameRate, value = walkSprites[i] };
        }
        keyFrames[walkSprites.Length] = new ObjectReferenceKeyframe { time = walkSprites.Length / walkFrameRate, value = walkSprites[0] };

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);

        EnableLoop(clip);
        AssetDatabase.CreateAsset(clip, path.Replace("\\", "/"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return clip;
    }

    private void EnableLoop(AnimationClip clip)
    {
        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
        settings.FindPropertyRelative("m_LoopTime").boolValue = true;
        serializedClip.ApplyModifiedProperties();
    }

    private BlendTree CreatePersistentBlendTree(AnimatorController controller, string name)
    {
        var blendTree = new BlendTree { name = name };
        AssetDatabase.AddObjectToAsset(blendTree, controller);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return blendTree;
    }

    static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);
            break;
        }
        return path;
    }

    private class NaturalSortComparer : IComparer<string>
    {
        public int Compare(string a, string b) => EditorUtility.NaturalCompare(a, b);
    }
}
