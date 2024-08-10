using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeLightOnPushed2 : MonoBehaviour
{
    public Button ThisButton;
    // Start is called before the first frame update
    void Start()
    {
        if (ThisButton)
        {
            ThisButton.onClick.AddListener(OnPushed);
        }
    }
    public void OnPushed()
    {
        MainJoyconInput.SetHomeLight(0xF,0x0, 0xF,
            new (byte,byte,byte)[]{(0xF,0x3,0x3),(0x0,0x3,0x3)});
    }

    private void Reset()
    {
        ThisButton = GetComponent<Button>();
    }
}
