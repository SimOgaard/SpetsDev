using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
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

	private void LateUpdate()
    {
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(MainCamera.mCamera.projectionMatrix, true);
		Matrix4x4 projectionMatrixInverse = projectionMatrix.inverse;

		Shader.SetGlobalMatrix("projectionMatrix", projectionMatrix);
		Shader.SetGlobalMatrix("projectionMatrixInverse", projectionMatrixInverse);

		Matrix4x4 worldToCameraMatrix = GL.GetGPUProjectionMatrix(MainCamera.mCamera.transform.worldToLocalMatrix, false);
		Matrix4x4 worldToCameraMatrixInverse = projectionMatrix.inverse;

		Shader.SetGlobalMatrix("worldToCameraMatrix", worldToCameraMatrix);
		Shader.SetGlobalMatrix("worldToCameraMatrixInverse", worldToCameraMatrixInverse);
	}
}
