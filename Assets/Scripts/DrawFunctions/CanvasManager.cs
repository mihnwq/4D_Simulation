using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

class CanvasManager : MonoBehaviour
{

    private string name = " ";

    [SerializeField]
    TextMeshProUGUI text;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (text.text != name)
            text.text = name;

        name = CurrentObjectRenderer.GetCurrentRenderedObject().name;
    }




}

