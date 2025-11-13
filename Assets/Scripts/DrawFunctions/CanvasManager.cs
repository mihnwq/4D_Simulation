using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class CanvasManager : MonoBehaviour
{

    private string name = " ";

    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    Scrollbar rotationSpeed;

    private GameObject currentObject;
    public _4D_Object current4D_Object;

    public float speed = 0f;         
    private float lastValue = 0f;      
    private float speedMultiplier = 0.03f;

    private void Start()
    {

        rotationSpeed.value = 0f;

        rotationSpeed.value = 0.5f;

        rotationSpeed.onValueChanged.AddListener(OnScroll);

        currentObject = CurrentObjectRenderer.GetCurrentRenderedObject();
        current4D_Object = currentObject.GetComponent<_4D_Object>();
        current4D_Object.ChangeSpeed(0.5f * 0.03f);
    }

    public void OnScroll(float value)
    {
        float delta = value - lastValue; 

        if (Mathf.Abs(delta) > 0.0001f) 
        {
  
            speed += delta * speedMultiplier;

            
            speed = Mathf.Clamp(speed, 0.0f, 0.1f);


            current4D_Object.ChangeSpeed(speed);
           /* // Update UI text
            if (speedText != null)
                speedText.text = $"Speed: {speed:F2}";*/
        }

        lastValue = value;
    }

    private void Update()
    {
        CheckObjectChanged();

        currentObject = CurrentObjectRenderer.GetCurrentRenderedObject();

        name = currentObject.name;
    }

    private void CheckObjectChanged()
    {
        if (!text.text.Equals(name))
        {
            text.text = name;



            current4D_Object = currentObject.GetComponent<_4D_Object>();
            current4D_Object.ChangeSpeed(speed);
        }
            
    }



}

