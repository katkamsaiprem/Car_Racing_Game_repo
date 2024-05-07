using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public Image SpeedRing;
    public float DisplaySpeed;

    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI GearText;
    // Start is called before the first frame update
    void Start()
    {
        SpeedRing.fillAmount = 0;
        SpeedText.text = "0";
        GearText.text = "1";
    }

    // Update is called once per frame
    void Update()
    {//convert speed into %
        DisplaySpeed = SaveScript.Speed / SaveScript.TopSpeed;//give decimal number btw 0&1
        SpeedRing.fillAmount = DisplaySpeed;
        SpeedText.text = (Mathf.Round(SaveScript.Speed).ToString());
        GearText.text = (SaveScript.Gear+1).ToString();


    }
}
