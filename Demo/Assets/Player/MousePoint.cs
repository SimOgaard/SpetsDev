using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour
{
    private Vector3 MousePos;
    private float rot;
    private Vector3 currentEulerAngles;
    private Quaternion finalRot;
    public GameObject camObj;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = camObj.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        rotToTarget();
    }

    public Vector3 getScreenPosFromCentre() //get position of mouse in 2D space from centre
    {
        Vector3 mousePos = Input.mousePosition; 
        mousePos.x -= Screen.width / 2;
        mousePos.y -= (Screen.height / 2);
        return mousePos;
    }

    public void rotToMouse2D() //rotates GameObject towards mouse using characters, does not work vey well for aiming projectiles in an isometric view, leaving it in because it may be useful
    {
        Vector3 relativeMousePos = getScreenPosFromPlayer();
        rot = Mathf.Atan(relativeMousePos.y / relativeMousePos.x) * 180 / Mathf.PI;
        if (relativeMousePos.x <= 0)
        {
            currentEulerAngles = new Vector3(0, -90 - rot, 0);
        }
        else
        {
            currentEulerAngles = new Vector3(0, 90 - rot, 0);
        }
        finalRot.eulerAngles = currentEulerAngles;
        transform.rotation = finalRot;
    }

    public Vector3 getScreenPosFromPlayer() //get position of mouse in 2D space relative to player attaching reference point
    {
        return Input.mousePosition -  cam.WorldToScreenPoint(transform.position);
    }

    public Vector3 getTargetMousePos() //gets the target position of the mouse calculated to be on the same plane as the player character

        //<summary>
        //Calculates the difference between the y distance between the camera and player as well as the camera and target mouse position
        //that difference is then divided by the distance between target and camera to get a factor that will multiply with the vector
        //that describes the distance between the camera and the target to extend that vector to end at the players y position. This endpoint
        //is then used as the target.
        //<summary>
    {
        float RayYDiff;
        float PlayerYDiff;
        float PlayerCamYDiff;
        float factor;
        Vector3 targetPos = new Vector3(0,0,0);
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit camRayHit;
        if (Physics.Raycast(camRay, out camRayHit))
        {
            RayYDiff = cam.transform.position.y - camRayHit.point.y;
            PlayerYDiff = gameObject.transform.position.y - camRayHit.point.y;
            PlayerCamYDiff = (RayYDiff - PlayerYDiff);
            factor = PlayerCamYDiff / RayYDiff;
            targetPos = cam.transform.position + ((camRayHit.point- cam.transform.position) * factor);
        }
        return targetPos;
    }

    public Vector3 getTargetHitPoint () //get world position of object mouse is pointing at
    {
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }

    public void rotToTarget() //rotate towards target point
    {
        transform.LookAt(getTargetMousePos());
    }
}
