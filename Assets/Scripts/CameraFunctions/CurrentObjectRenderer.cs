using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class CurrentObjectRenderer : MonoBehaviour
{
    public List<GameObject> _4D_Objects;

    public static GameObject currentObject;

    private int minObjectCount, maxObjectCount;

    private int index = 0;

    private void Start()
    {
        minObjectCount = 0;
        maxObjectCount = _4D_Objects.Count - 1;
        MoveToNextObject(0);
    }

    public void OnClickNext()
    {
        MoveToNextObject(1);
    }

    public void OnClickPrevious()
    {
        MoveToNextObject(-1);
    }


    private void MoveToNextObject(int nextPosition)
    {
        _4D_Objects[index].SetActive(false);

        index += nextPosition;

        if (index < 0)
            index = maxObjectCount;
        else if (index > maxObjectCount)
            index = minObjectCount;

        MoveAroundObject.SetTarget(_4D_Objects[index].transform);
        _4D_Objects[index].SetActive(true);

        currentObject = _4D_Objects[index];
    }

    public static GameObject GetCurrentRenderedObject() => currentObject;

}

