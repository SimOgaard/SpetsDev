using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Changes Unity Inspector window to show Scriptable Object fields and allow for drop down view
/// </summary>
[CustomEditor(typeof(WorldGenerationManager))]
public class CustomEditorForInspector : Editor
{
    private WorldGenerationManager world;
    private Editor editor;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        Texture2D[] noise_images = world.world_generation.GetNoiseTextures();
        for (int i = 0; i < noise_images.Length; i++)
        {
            if (i % 2 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }

            TextureField("Noise_" + i, noise_images[i]);
        }
        EditorGUILayout.EndHorizontal();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                world.world_generation.UpdateWorld();
            }
        }

        NoiseLayerSettings settings = world.world_generation.GetNoiseSettings();
        DrawSettingsEditor(settings, world.world_generation.UpdateWorld, ref world.foldout, ref editor);
    }

    private static Texture2D TextureField(string name, Texture2D texture)
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 224;
        GUILayout.Label(name, style);
        var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(224), GUILayout.Height(224));
        GUILayout.EndVertical();
        return result;
    }

    private void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        world = (WorldGenerationManager)target;
    }
}
