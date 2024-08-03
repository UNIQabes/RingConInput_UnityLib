using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class V3_MyUtil
{
    public static Quaternion RotateV2V(Vector3 from, Vector3 to)
    {
        Vector3 f_norm = from.normalized;
        Vector3 t_norm = to.normalized;
        Vector3 cross_ft = Vector3.Cross(f_norm, t_norm);
        //Vector3 cross_ft = Vector3.Cross(t_norm, f_norm);
        
        return Quaternion.AngleAxis(Vector3.SignedAngle(f_norm, t_norm, cross_ft), cross_ft);
    }

    public static Vector3 GetHorizontalComp(Vector3 origVector, Vector3 parallelVector)
    {
        Vector3 parallelVector_norm=parallelVector.normalized;
        return parallelVector_norm*Vector3.Dot(origVector, parallelVector_norm);
    }
    public static Vector3 GetVerticalComp(Vector3 origVector, Vector3 parallelVector)
    {
        return origVector - GetHorizontalComp(origVector, parallelVector);
    }
    public static Vector3 rotationWithMatrix(Vector3 originalV,
    Vector3 xUnit_Converted,
    Vector3 yUnit_Converted,
    Vector3 zUnit_Converted)
    { return xUnit_Converted * originalV.x + yUnit_Converted * originalV.y + zUnit_Converted * originalV.z; }

    public static Quaternion rotationWithMatrix(Quaternion originalQuaternion,
        Vector3 xUnit_Converted,
        Vector3 yUnit_Converted,
        Vector3 zUnit_Converted)
    {
        Vector3 rotation_zxy= originalQuaternion.eulerAngles;
        return Quaternion.AngleAxis(rotation_zxy.y, yUnit_Converted)
            * Quaternion.AngleAxis(rotation_zxy.x, xUnit_Converted)
            * Quaternion.AngleAxis(rotation_zxy.z, zUnit_Converted);
    }
    
    
}
