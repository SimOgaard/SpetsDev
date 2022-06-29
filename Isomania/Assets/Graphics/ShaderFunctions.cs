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

    private void Update()
    {
		Shader.SetGlobalMatrix("UNITY_MATRIX_I_V", MainCamera.mCamera.projectionMatrix.inverse);

		Shader.SetGlobalMatrix(
			"Camera_ForwardMatrix",
			Matrix4x4.Rotate(Quaternion.LookRotation(MainCamera.mCamera.transform.forward, Vector3.up))
		);
		Shader.SetGlobalMatrix(
			"Camera_ForwardMatrixInverse",
			Matrix4x4.Rotate(Quaternion.LookRotation(MainCamera.mCamera.transform.forward, Vector3.up)).inverse
		);
	}
}
