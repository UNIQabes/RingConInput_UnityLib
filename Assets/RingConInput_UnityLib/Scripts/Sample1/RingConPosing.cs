using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingConPosing : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        //JoyconPose_R_Ring
        //this.transform.rotation = V3_MyUtil.rotationWithMatrix(MainJoyconInput.JoyconPose_R_Ring,
        this.transform.rotation = V3_MyUtil.rotationWithMatrix(MainJoyconInput.JoyconR_JoyconCoordSmoothedPose,
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1));
        float strainDelta=MainJoyconInput.RingconStrain-4500;
        this.transform.localScale = new Vector3(1-strainDelta/5000, 1 + strainDelta / 5000, 1);
    }

}
