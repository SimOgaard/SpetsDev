using UnityEngine;
using UnityEngine.Rendering;

// Run in edit mode so we can see in the scene view
[RequireComponent(typeof(Light)), ExecuteInEditMode]
public class CopyShadowmap : MonoBehaviour
{
    private Light m_Light;
    private CommandBuffer m_Buffer;

    private void OnEnable()
    {
        m_Light = GetComponent<Light>();

        // Only want to support directional lights
        if (m_Light.type == LightType.Directional)
        {
            // Create a new command buffer
            m_Buffer = new CommandBuffer() { name = "Copy Shadowmap" };
            // Just expose the shadowmap to all shaders globally
            m_Buffer.SetGlobalTexture("_DirectionalShadowmap", BuiltinRenderTextureType.CurrentActive);

            // Add just after the shadowmap is rendered so it is set to BuiltinRenderTextureType.CurrentActive
            m_Light.AddCommandBuffer(LightEvent.AfterShadowMap, m_Buffer);
        }
    }

    private void OnDisable()
    {
        // Cleanup light & buffer
        if (m_Buffer != null)
        {
            if (m_Light != null)
            {
                m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, m_Buffer);
            }

            m_Buffer.Release();
            m_Buffer = null;
        }
    }
}