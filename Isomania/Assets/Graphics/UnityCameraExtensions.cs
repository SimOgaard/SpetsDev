using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region unity_camera_extensions
public static class UnityCameraExtensions
{
	/// <summary>
	/// View matrix of camera
	/// </summary>
	public static Matrix4x4 MATRIX_V(this Camera camera)
	{
		Matrix4x4 viewMat = camera.worldToCameraMatrix;
		return viewMat;
	}

	/// <summary>
	/// Projection matrix of camera
	/// </summary>
	public static Matrix4x4 MATRIX_P(this Camera camera)
	{
		Matrix4x4 projMat = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
		return projMat;
	}

	/// <summary>
	/// View-projection matrix of camera (MATRIX_P * MATRIX_V)
	/// </summary>
	public static Matrix4x4 MATRIX_VP(this Camera camera)
	{
		return (camera.MATRIX_P() * camera.MATRIX_V());
	}

	/// <summary>
	/// Inverse view-projection matrix of camera (MATRIX_P * MATRIX_V).inverse
	/// </summary>
	public static Matrix4x4 MATRIX_I_VP(this Camera camera)
	{
		return (camera.MATRIX_VP()).inverse;
	}
}
#endregion
