using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class CurrentObjectRenderer : MonoBehaviour
{
    public List<GameObject> _4D_Objects;

    CurrentObjectRenderer instance;

    public void Awake()
    {
        instance = this;
        
    }

    private int index = 0;

    private void Start()
    {
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

        //Looks like shit, gonna remake it.
        if (index + nextPosition == -1)
            index = _4D_Objects.Count - 1;
        else if (index == _4D_Objects.Count)
            index = 0;
        else index = index + nextPosition;

        MoveAroundObject.SetTarget(_4D_Objects[index].transform);
        _4D_Objects[index].SetActive(true);
    }

}

