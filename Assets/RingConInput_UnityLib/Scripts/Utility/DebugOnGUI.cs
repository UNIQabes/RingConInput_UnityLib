using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOnGUI : MonoBehaviour
{
    [HideInInspector] public static object lockObject = new object();
    static DebugOnGUI singleton=null;
    static Dictionary<string,object> LogMessage=new Dictionary<string, object>();
    private static bool dictKeysDirtyFlag = false;
    
    //各ボタン入力を表示するかどうか
    /*
    [SerializeField] private bool onAButtonPushed_IsDisp;
    [SerializeField] private bool onRButtonPushed_IsDisp;
    [SerializeField] private bool onZRButtonPushed_IsDisp;
    [SerializeField] private bool ringconStrain_IsDisp;
    */
    
    // Start is called before the first frame update
    void Start()
    {
        
        singleton = this;
        
    }
    

    public static void Log(object message,string key)
    {
        lock (lockObject)
        {
            
            if (LogMessage!=null)
            {
                if (LogMessage.ContainsKey(key))
                {
                    LogMessage[key] = message;
                }
                else
                {
                    LogMessage.Add(key, message);
                    dictKeysDirtyFlag = true;
                }
            }
        }
        
        
    }
    private void OnDestroy()
    {
        LogMessage = new Dictionary<string, object>();
    }

    void OnGUI()
    {
        lock (lockObject)
        {
            if (singleton)
            {
                if (dictKeysDirtyFlag)
                {
                    if (Event.current.type != EventType.Layout)
                    {
                        return;
                    }
                    else
                    {
                        dictKeysDirtyFlag = false;
                    }

                }
                //GUILayout.Label($"ddddd");
                GUIStyle style = GUI.skin.GetStyle("label");
                style.fontSize = 36;
                style.padding = new RectOffset(0, 0, 0, 0);
                GUILayout.BeginHorizontal(GUILayout.Width(960));
                GUILayout.BeginVertical(GUILayout.Width(960));
                //GUILayout.Label($"ddddd");
                foreach (object aMessage in LogMessage.Values)
                {
                    //Debug.Log("dd");
                    GUILayout.Label($"{aMessage}");
                }


                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        
        
    }
}
