using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MoveAroundObject : MonoBehaviour
{
    [SerializeField]
    private float mouseSensitivity = 3.0f;

    private float rotationY;
    private float rotationX;

    private static Transform target;

    [SerializeField]
    public float distanceFromTarget = 5.0f;

    private Vector3 currentRoation;
    private Vector3 smoothVelocity = Vector3.zero;

    [SerializeField]
    private float maxDistance = 100;

    [SerializeField]
    private float minDistance = 3;

    [SerializeField]
    public float mapSmoothTime = 0.2f;


    public float smallestClampDown = 0;


    public void Start()
    {
       //Nothing here to see.
    }

    public void Update()
    {
        if(Input.GetMouseButton(1))
        {
            moveAroundObject(mapSmoothTime, smallestClampDown);
        }
        distanceFromTarget = zoom(distanceFromTarget);
        getToObjectPosition();

    }

    public float minClampDown = -20f;

    public void moveAroundObject(float smoothTime, float smallestClampDown)
    {

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;

        rotationX += mouseY;

        //rotationX = Mathf.Clamp(rotationX, getMin(rotationX, rotationY, minClampDown, smallestClampDown), 40);

        Vector3 nextRoation = new Vector3(rotationX, rotationY);

        currentRoation = Vector3.SmoothDamp(currentRoation, nextRoation, ref smoothVelocity, smoothTime);

        transform.localEulerAngles = currentRoation;

    }
   

    public void getToObjectPosition()
    {
        Vector3 nextPosition = target.position - transform.forward * distanceFromTarget;

        Vector3 offset = Vector3.up;

        offset.y -= offset.y / 2;

        Vector3 finalTarget = nextPosition;




        transform.position = finalTarget;

    }

    public float zoom(float distanceFromTarget)
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // forward
        {
            distanceFromTarget += 2;

            if (distanceFromTarget > maxDistance)
            {
                distanceFromTarget = maxDistance;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            distanceFromTarget -= 2;

            if (distanceFromTarget < minDistance)
            {
                distanceFromTarget = minDistance;
            }
        }

        return distanceFromTarget;
    }

    public static void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }


}

