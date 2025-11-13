using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class _4D_Object : MonoBehaviour
{
    public float rotationSpeed;

    public float angle;

    protected virtual void Update()
    {

        angle += Time.deltaTime * rotationSpeed * Mathf.Rad2Deg;

    }

    public void ChangeSpeed(float value)
    {
        // rotationSpeed = Mathf.Clamp(rotationSpeed + rotationSpeedFactor, 0, 0.05f);
        rotationSpeed = value;
    }



}

