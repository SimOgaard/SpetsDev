using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColossalPlains))]
public class CustomEditorForInspector : Editor
{
    ColossalPlains colossal_plains;
    Editor ground_editor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                colossal_plains.CreateGroundMesh();
            }
        }

        DrawSettingsEditor(colossal_plains.ground_noise_settings, colossal_plains.CreateGroundMesh, ref colossal_plains.ground_noise_settings_foldout, ref ground_editor);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
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
        colossal_plains = (ColossalPlains)target;
    }
}
