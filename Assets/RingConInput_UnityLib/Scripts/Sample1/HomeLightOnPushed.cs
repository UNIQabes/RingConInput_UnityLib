using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeLightOnPushed : MonoBehaviour
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
        MainJoyconInput.SetHomeLight(0xF,0x0, 0x4,
            new (byte,byte,byte)[]{(0xF,0x2,0x2),(0x0,0x2,0x2),(0xF,0x2,0x2),(0x0,0x2,0x2)});
    }

    private void Reset()
    {
        ThisButton = GetComponent<Button>();
    }
}
