using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joycon_obs
{
    //1フレーム間にJoycon_subjが受け取ったInputReportがまとめて渡される
    public virtual void OnReadReport(string serealNumber,List<byte[]> reports)
    {

    }
    //接続解除時に呼ばれる
    public virtual void OnDisConnect()
    {

    }
}
