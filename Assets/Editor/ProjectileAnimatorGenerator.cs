using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

public class GenericProjectileAnimatorGenerator : EditorWindow
{
    private Sprite flightSpriteSheet;
    private Sprite explosionSpriteSheet;
    private string baseName = "Projectile";
    private float flightFrameRate = 16f;
    private float explosionFrameRate = 12f;

    private int flightFrameCount = 8;
    private int explosionFrameCount = 6;

    private string customSavePath = "Assets/";

    [MenuItem("Tools/Generate Generic Projectile Animator")]
    public static void ShowWindow()
    {
        GetWindow<GenericProjectileAnimatorGenerator>("Projectile Animator Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Projectile Animation Setup", EditorStyles.boldLabel);

        flightSpriteSheet = (Sprite)EditorGUILayout.ObjectField("Flight Sprite Sheet", flightSpriteSheet, typeof(Sprite), false);
        explosionSpriteSheet = (Sprite)EditorGUILayout.ObjectField("Explosion Sprite Sheet", explosionSpriteSheet, typeof(Sprite), false);
        
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        flightFrameRate = EditorGUILayout.FloatField("Flight FPS", flightFrameRate);
        explosionFrameRate = EditorGUILayout.FloatField("Explosion FPS", explosionFrameRate);

        GUILayout.Space(5);
        GUILayout.Label("Sprite Sheet Layout", EditorStyles.boldLabel);
        flightFrameCount = EditorGUILayout.IntField("Flight Frame Count", flightFrameCount);
        explosionFrameCount = EditorGUILayout.IntField("Explosion Frame Count", explosionFrameCount);

        if (GUILayout.Button("Select Save Folder"))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
            {
                customSavePath = "Assets" + selected.Substring(Application.dataPath.Length);
            }
            else if (!string.IsNullOrEmpty(selected))
            {
                EditorUtility.DisplayDialog("Invalid Path", "The selected folder must be inside the project's Assets folder.", "OK");
            }
        }

        if (!string.IsNullOrEmpty(customSavePath))
        {
            EditorGUILayout.HelpBox("Save Path: " + customSavePath, MessageType.Info);
        }

        if (GUILayout.Button("Generate Animator"))
        {
            if (flightSpriteSheet == null || explosionSpriteSheet == null)
            {
                EditorUtility.DisplayDialog("Missing Sprites", "Please assign both sprite sheets.", "OK");
                return;
            }
            GenerateProjectileAnimator();
        }
    }

    private void GenerateProjectileAnimator()
    {
        if (string.IsNullOrEmpty(customSavePath))
        {
            EditorUtility.DisplayDialog("Path Not Set", "Please select a save folder first.", "OK");
            return;
        }

        string animFolder = Path.Combine(customSavePath, baseName + "_Animations");
        if (!Directory.Exists(animFolder))
        {
            Directory.CreateDirectory(animFolder);
        }

        // --- INICIO DE LA SOLUCIÓN SECUENCIAL ROBUSTA ---

        // Paso 1: Crear y guardar la animación de vuelo (Flight).
        string flightClipPath = Path.Combine(animFolder, baseName + "_Flight.anim").Replace("\\", "/");
        string flightSheetPath = AssetDatabase.GetAssetPath(flightSpriteSheet);
        var flightSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(flightSheetPath).OfType<Sprite>().OrderBy(s => s.name).Take(flightFrameCount).ToArray();
        
        if (flightSprites.Length < flightFrameCount)
        {
            EditorUtility.DisplayDialog("Error", $"Could not load enough flight sprites. Found {flightSprites.Length}, expected {flightFrameCount}. Check sprite slicing and naming.", "OK");
            return;
        }
        CreateClip(flightSprites, flightFrameRate, flightClipPath, true);
        
        // Forzar al editor a reconocer el nuevo clip.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Paso 2: Crear y guardar la animación de explosión (Explosion).
        string explosionClipPath = Path.Combine(animFolder, baseName + "_Explosion.anim").Replace("\\", "/");
        string explosionSheetPath = AssetDatabase.GetAssetPath(explosionSpriteSheet);
        var explosionSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(explosionSheetPath).OfType<Sprite>().OrderBy(s => s.name).Take(explosionFrameCount).ToArray();
        
        if (explosionSprites.Length < explosionFrameCount)
        {
            EditorUtility.DisplayDialog("Error", $"Could not load enough explosion sprites. Found {explosionSprites.Length}, expected {explosionFrameCount}. Check sprite slicing and naming.", "OK");
            return;
        }
        CreateClip(explosionSprites, explosionFrameRate, explosionClipPath, false);

        // Forzar al editor a reconocer el segundo clip.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Paso 3: Crear el Animator Controller vacío.
        string controllerPath = Path.Combine(animFolder, baseName + "_Animator.controller").Replace("\\", "/");
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Forzar al editor a reconocer el controlador antes de modificarlo.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Paso 4: Cargar todos los assets que ahora existen de forma segura.
        AnimationClip flightClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(flightClipPath);
        AnimationClip explosionClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(explosionClipPath);
        
        if (flightClip == null || explosionClip == null)
        {
            Debug.LogError("Failed to load created animation clips after refresh.");
            EditorUtility.DisplayDialog("Error", "Failed to load created clips. Check the console for more details.", "OK");
            return;
        }

        // Paso 5: Ahora que todo está cargado y procesado, modificar la máquina de estados.
        var rootSM = controller.layers[0].stateMachine;

        controller.AddParameter("Explode", AnimatorControllerParameterType.Trigger);

        AnimatorState flightState = rootSM.AddState("Flight");
        flightState.motion = flightClip;
        
        AnimatorState explosionState = rootSM.AddState("Explosion");
        explosionState.motion = explosionClip;
        
        rootSM.defaultState = flightState;

        AnimatorStateTransition transition = flightState.AddTransition(explosionState);
        transition.hasExitTime = false;
        transition.duration = 0;
        transition.AddCondition(AnimatorConditionMode.If, 0, "Explode");

        // Paso 6: Guardar los cambios finales en el controlador.
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Success", baseName + " animations and animator generated successfully in " + animFolder, "OK");
    }

    private void CreateClip(Sprite[] sprites, float frameRate, string path, bool loop)
    {
        AnimationClip clip = new AnimationClip { frameRate = frameRate };

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        float time = 0f;
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = time,
                value = sprites[i]
            };
            time += 1f / frameRate;
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        AssetDatabase.CreateAsset(clip, path);
    }
}
