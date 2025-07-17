using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(MonoBehaviour), true)]
public class SpawnerEditor : Editor
{
    private int selectedIndex = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target is ISpawner spawner)
        {
            var spawnSettings = spawner.GetSpawnSettings();

            if (spawnSettings != null && spawnSettings.Count > 0)
            {
                string[] options = spawnSettings
                    .Select(settings => settings.Name)
                    .ToArray();

                if (selectedIndex < 0 || selectedIndex >= options.Length)
                {
                    selectedIndex = 0;
                }

                selectedIndex = EditorGUILayout.Popup("Select Object", selectedIndex, options);
            }
            else
            {
                EditorGUILayout.LabelField("No spawn settings available.");
            }
        }
    }

    private void OnSceneGUI()
    {
        if (target is ISpawner spawner)
        {
            Handles.color = Color.green;
            var spawnSettings = spawner.GetSpawnSettings();

            // Alt + clic izquierdo
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt)
            {
                // Obtener punto del mouse en mundo (plano XY)
                Vector2 worldPoint = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;

                if (spawnSettings != null && spawnSettings.Count > 0)
                {
                    spawnSettings[selectedIndex].spawnPositions.Add(worldPoint);
                    EditorUtility.SetDirty(target); // Marcar como modificado
                }

                Event.current.Use(); // Consumir evento
            }

            // Dibujar los puntos
            foreach (var settings in spawnSettings)
            {
                if (settings.spawnPositions != null)
                {
                    foreach (var pos in settings.spawnPositions)
                    {
                        Handles.DrawSolidDisc(pos, Vector3.forward, 0.2f); // Vector3.forward = eje Z
                    }
                }
            }
        }
    }
}
