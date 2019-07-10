using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Values : MonoBehaviour
{
    public Text sliderValue;
    // Start is called before the first frame update
    void Start()
    {
        sliderValue = GetComponent<Text>();
    }

    // Update is called once per frame
    public void UpdateValue(float value)
    {
        sliderValue.text = value + "";     
    }
}
