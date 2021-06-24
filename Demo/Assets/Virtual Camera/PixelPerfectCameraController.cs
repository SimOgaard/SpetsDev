using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// testa även:
// material på render texturen som tar in x och y movement och samplar ofsett "tex2D(_MainTex, pos);" || FÖR ATT FÅ BORT JITTER
// https://www.youtube.com/watch?v=2JbhkZe22bE&ab_channel=HeartBeast || FÖR ATT FÅ BORT JITTER

// pixel snaps and glides
[RequireComponent(typeof(Camera))]
public class PixelPerfectCameraController : MonoBehaviour
{
    //[SerializeField]
    //private float mouse_sensitivity = 2f;
    [SerializeField]
    private float move_sensitivity = 1f;

    private float units_per_pixel = 40f / 216f;
    private Vector3 local_rotation;

    private Vector3 offset;

    private Camera m_camera;
    private Transform camera_pivot_transform;
    private Transform virtual_screen_transform;

    [SerializeField]
    private RenderTexture render_texture;
    [SerializeField]
    private Material virtual_screen_glide;

    private Vector3 RoundToPixel(Vector3 position)
    {
        if (units_per_pixel == 0.0f)
            return position;

        Vector3 result;
        result.x = Mathf.Round(position.x / units_per_pixel) * units_per_pixel;
        result.y = Mathf.Round(position.y / units_per_pixel) * units_per_pixel;
        result.z = Mathf.Round(position.z / units_per_pixel) * units_per_pixel;

        return result;
    }

    private void PixelSnap()
    {
        Vector3 camera_position = m_camera.transform.rotation * m_camera.transform.position;
        Vector3 rounded_camera_position = RoundToPixel(camera_position);
        offset = rounded_camera_position - camera_position;
        offset.z = -offset.z;
        Matrix4x4 offset_matrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_camera.worldToCameraMatrix = offset_matrix * m_camera.transform.worldToLocalMatrix;
    }

    private void UpdateCameraRotation(bool right_click, bool left_click)
    {
        local_rotation.x += System.Convert.ToInt32(right_click) * 45f - System.Convert.ToInt32(left_click) * 45f;
        camera_pivot_transform.rotation = Quaternion.Euler(local_rotation.y, local_rotation.x, 0);
    }

    private void MoveCamera()
    {
        Vector3 movement_normal = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
        Vector3 movement_translated_normal = Quaternion.AngleAxis(local_rotation.x, Vector3.up) * movement_normal;
        Vector3 movement = movement_translated_normal * move_sensitivity * Time.deltaTime;

        camera_pivot_transform.position += movement;
    }

    private void MoveVirtualCamera()
    {
        Vector3 offset_new = new Vector3(offset.x, offset.y, 0f);

        Debug.Log(offset.x);
        virtual_screen_transform.position = offset_new * 108f/20f;
    }

    private void Awake()
    {
        camera_pivot_transform = transform.parent;
        virtual_screen_transform = camera_pivot_transform.parent.GetChild(1).GetChild(0);
        m_camera = GetComponent<Camera>();
    }

    private void Start()
    {
        local_rotation.y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad));
        UpdateCameraRotation(false, false);
    }
    
    private void Update()
    {
        // change variables in render_texture_glide_material, 

        //Graphics.Blit(render_texture, null, virtual_screen_glide, -1);
    }

    RenderTexture mainSceneRT;
    RenderTexture rayMarchRT;
    void OnPreRender()
    {
        //m_camera.targetTexture = mainSceneRT;
        // this ensures that w/e the camera sees is rendered to the above RT
    }

    void OnPostrender()
    {
        // render to secondary render target
        //Graphics.Blit(rayMarchRT, mainSceneRT, virtual_screen_glide);
        // You have to set target texture to null for the Blit below to work
        //m_camera.targetTexture = null;

        //Graphics.Blit(mainSceneRT, null as RenderTexture);
        //Graphics.Blit(render_texture, null, virtual_screen_glide, -1);
    }

    private void LateUpdate()
    {
        //UpdateCameraRotation(Input.GetButtonDown("Fire1"), Input.GetButtonDown("Fire2"));
        MoveCamera();
        MoveVirtualCamera();
    }

    private void OnPreCull()
    {
        PixelSnap();
    }
}
