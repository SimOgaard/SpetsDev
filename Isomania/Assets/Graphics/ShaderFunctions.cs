using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ShaderFunctions : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("Tools/Clear shader cache")]
	static public void ClearShaderCache_Command()
	{
		var shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache");
		Directory.Delete(shaderCachePath, true);
	}
#endif
}
