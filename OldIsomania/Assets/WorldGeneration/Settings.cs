using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class Settings : ScriptableObject
{
#if UNITY_EDITOR
	public virtual void Rename()
	{
		var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
		var hierarchyWindow = EditorWindow.GetWindow(type);
		var rename = type.GetMethod("RenameGO", BindingFlags.Instance | BindingFlags.NonPublic);
		rename.Invoke(hierarchyWindow, null);
	}
#endif

	/// <summary>
	/// Updates all data associated with this object
	/// </summary>
	public virtual void Update()
    {
    }

	/// <summary>
	/// Clears all data associated with this object
	/// </summary>
	public virtual void Destroy()
	{
	}
}
