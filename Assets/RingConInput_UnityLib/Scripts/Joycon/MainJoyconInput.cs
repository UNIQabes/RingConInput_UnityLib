using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;



public class MainJoyconInput : Joycon_obs
{
    private static MainJoyconInput instance=null;
    private static MainJoyconInput getInstance
    {
        get
        {
            if (instance==null)
            {
                instance = new MainJoyconInput();
            }
            return instance;
        }
    }
    private static CancellationTokenSource cancellationTokenSourceOnAppQuit;
    private static CancellationToken cancellationTokenOnAppQuit;//Application終了時にCancellされるToken
    
    //JoyconRについての変数
    //MainのJoyConのJoyConConnection nullでないなら、このJoyConConnectionに登録している
    private static JoyConConnection _joyconConnection_R;
    public static JoyConConnectInfo ConnectInfo_JoyconR = JoyConConnectInfo.JoyConIsNotFound;
    //MainのJoyConのシリアルナンバー 接続しているJoyCon、もしくは接続していないときは優先して登録するJoyCon。空の文字列の時は好きに登録すれば良い。
    public static string SerialNumber_R { get; private set; } = "";

    private static Quaternion JoyconR_InitJoyconCoordPose = Quaternion.identity;
    private static Vector3 JoyconR_DefaultJoyconCoordDownVec = Vector3.up;
    public static Quaternion JoyconR_JoyconCoordPose { get; private set; }=Quaternion.identity;
    public static Quaternion JoyconR_JoyconCoordSmoothedPose { get; private set; }
    public static float RingconStrain { get; private set; }
    public static bool RButton_JoyconRInput { get; private set; }
    public static bool ZRButton_JoyconRInput { get; private set; }
    public static bool AButton_JoyconRInput { get; private set; }
    private static List<byte[]> _inputReportsInThisFrame_JoyconR = null;
    public static float JoyconR_GyroXCalibration=0;
    public static float JoyconR_GyroYCalibration=0;
    public static float JoyconR_GyroZCalibration=0;
    
    
    //JoyconLについての変数
    private static JoyConConnection _joyconConnection_L;
    public static JoyConConnectInfo ConnectInfo_JoyconL = JoyConConnectInfo.JoyConIsNotFound;
    //MainのJoyConのシリアルナンバー 接続しているJoyCon、もしくは接続していないときは優先して登録するJoyCon。空の文字列の時は好きに登録すれば良い。
    public static string SerialNumber_L { get; private set; } = "";

    private static Quaternion JoyconL_InitJoyconCoordPose = Quaternion.identity;
    private static Vector3 JoyconL_DefaultJoyconCoordDownVec = Vector3.up;
    public static Quaternion JoyconL_JoyconCoordPose { get; private set; }=Quaternion.identity;
    public static Quaternion JoyconL_JoyconCoordSmoothedPose { get; private set; }
    public static bool RButton_JoyconLInput { get; private set; }
    public static bool ZRButton_JoyconLInput { get; private set; }
    public static bool AButton_JoyconLInput { get; private set; }
    private static List<byte[]> _inputReportsInThisFrame_JoyconL = null;
    public static float JoyconL_GyroXCalibration=0;
    public static float JoyconL_GyroYCalibration=0;
    public static float JoyconL_GyroZCalibration=0;
    
    
   
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Joycon_subj.RegisterAfterInitCallback(AfterSubjInitCallback);
    }

    static void AfterSubjInitCallback()
    {
        _joyconConnection_R = null;
        cancellationTokenSourceOnAppQuit = new CancellationTokenSource();
        cancellationTokenOnAppQuit = cancellationTokenSourceOnAppQuit.Token;
        Joycon_subj.UpdateJoyConConnection();
        IsTryingReconnectJoycon = true;


        RingconStrain = 0;
        Application.quitting += OnApplicatioQuitStatic;
        updatestatic().Forget();
        reconnectTask().Forget();
        fixedupdatestatic().Forget();
    }




    static async UniTaskVoid updatestatic()
    {
        while (!cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            
            DebugOnGUI.Log($"{SerialNumber_R} {ConnectInfo_JoyconR}", "ConnectInfo");
            if (_joyconConnection_R!=null&&!_joyconConnection_R.IsConnecting)
            {
                if (ConnectInfo_JoyconR != JoyConConnectInfo.JoyConIsNotFound)
                {
                    DebugOnGUI.Log("接続が切れたよ", "Joycon");
                    Debug.Log("接続が切れたよ!!!");
                }
                ConnectInfo_JoyconR = JoyConConnectInfo.JoyConIsNotFound;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate,cancellationTokenOnAppQuit);
        }
        
    }

    static async UniTaskVoid fixedupdatestatic()
    {
        while (!cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            //ResetYRot_xyOrder();
            JoyconR_JoyconCoordSmoothedPose=Quaternion.Slerp(JoyconR_JoyconCoordSmoothedPose, JoyconR_JoyconCoordPose, 0.05f);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationTokenOnAppQuit);
        }
    }

    public static bool IsTryingReconnectJoycon;
    static async UniTaskVoid reconnectTask()
    {
        while (!cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            if (ConnectInfo_JoyconR!=JoyConConnectInfo.JoyConIsReady & IsTryingReconnectJoycon)
            {
                Debug.Log("再接続を試行します");
                DebugOnGUI.Log("再接続を試行します", "Joycon");
                bool connectIsSuccess=await ReConnectJoyconAsync();
            }
            await UniTask.Delay(1000,false,PlayerLoopTiming.EarlyUpdate,cancellationTokenOnAppQuit);
        }
    }

    private static void OnApplicatioQuitStatic()
    {

        cancellationTokenSourceOnAppQuit.Cancel();
        
    }

    


    //JoyConを接続し直す
    public static async UniTask<bool> ReConnectJoyconAsync()
    {
        //以前登録していたJoyConConnectionへの登録を解除 二重にJoyconConnectionに登録するのを防ぐ
        if (_joyconConnection_R != null)
        {
            _joyconConnection_R.DelObserver(getInstance);
            _joyconConnection_R = null;
        }
        string newJoyConSerialNum = "";
        Joycon_subj.UpdateJoyConConnection();
        List<string> joyconRKeys = Joycon_subj.GetJoyConSerialNumbers_R();
        foreach (string aJoyconSerialNum in joyconRKeys)
        {
            newJoyConSerialNum = aJoyconSerialNum;
            //以前接続していたJoyConに優先的に繋ぐ
            if (newJoyConSerialNum == SerialNumber_R)
            {
                break;
            }
        }


        if (newJoyConSerialNum != "")
        {
            JoyConConnection newJoyConConnection = Joycon_subj.GetJoyConConnection(newJoyConSerialNum);
            //DebugOnGUI.Log(newJoyConSerialNum, "dedwedsfef");
            if (newJoyConConnection.ConnectToJoyCon())
            {
                //ConnectInfo = JoyConConnectInfo.SettingUpJoycon;
                newJoyConConnection.AddObserver(getInstance);
                _joyconConnection_R = newJoyConConnection;
                SerialNumber_R = newJoyConSerialNum;
                await joyConSetUp(cancellationTokenOnAppQuit);
            }
            else
            {
                Debug.Log("接続できなかった!!");
            }

        }

        JoyconR_JoyconCoordPose = JoyconR_InitJoyconCoordPose;
        JoyconR_JoyconCoordSmoothedPose = JoyconR_InitJoyconCoordPose;
        RingconStrain = 0;
        return true;
    }

    



    private static async UniTask joyConSetUp(CancellationToken cancellationToken)
    {


        //await UniTask.DelayFrame(100, cancellationToken: cancellationToken);
        //実行コンテクスト(?というらしい)をPreUpdateに切り替える
        await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationTokenOnAppQuit);

        byte[] ReplyBuf = new byte[50];

        
        try
        {
            ConnectInfo_JoyconR = JoyConConnectInfo.SettingUpJoycon;
            Debug.Log("セットアップします!");
            DebugOnGUI.Log("セットアップ開始", "Joycon");

            //セットアップを開始したら、4のランプを点灯させる
            _joyconConnection_R.SendSubCmd(new byte[] { 0x30, 0b00001000 }, cancellationTokenOnAppQuit).Forget();
            

            // Enable vibration
            DebugOnGUI.Log($"{SerialNumber_R}:Enable vibration", "Joycon");
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x48, 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Enable IMU data", "Joycon");
            // Enable IMU data
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x40, 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);
            
            DebugOnGUI.Log($"{SerialNumber_R}:Set IMU sensitivity 0x41", "Joycon");
            //Set IMU sensitivity 0x41
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x41, 0x03, 0x00, 0x01 , 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Set input report mode to 0x30", "Joycon");
            //Set input report mode to 0x30
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x03, 0x30 }, ReplyBuf, cancellationTokenOnAppQuit);

            //3/8終了　3のランプも点灯させる
            _joyconConnection_R.SendSubCmd(new byte[] { 0x30, 0b00001100 }, cancellationTokenOnAppQuit).Forget();

            DebugOnGUI.Log($"{SerialNumber_R}:Enable MCU data", "Joycon");
            // Enabling MCU data
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x22, 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Enable MCU data", "Joycon");
            //Enabling MCU data 21 21 1 1
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[39] { 0x21, 0x21, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Get external data", "Joycon");
            //Get ext data 59
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x59, 0x0 }, ReplyBuf, cancellationTokenOnAppQuit);

            //6/8終了　2のランプも点灯させる
            _joyconConnection_R.SendSubCmd(new byte[] { 0x30, 0b00001110 }, cancellationTokenOnAppQuit).Forget();

            DebugOnGUI.Log($"{SerialNumber_R}:Get external device in format config", "Joycon");
            //Get ext dev in format config 5C
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x5C, 0x06, 0x03, 0x25, 0x06, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x16, 0xED, 0x34, 0x36, 0x00, 0x00, 0x00, 0x0A, 0x64, 0x0B, 0xE6, 0xA9, 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xA8, 0xE1, 0x34, 0x36 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Start external polling", "Joycon");
            //Start external polling 5A
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x5A, 0x04, 0x01, 0x01, 0x02 }, ReplyBuf, cancellationTokenOnAppQuit);

            //セットアップが完了したら、全てのランプを光らせる
            await _joyconConnection_R.SendSubCmd(new byte[] { 0x30, 0b00001111 }, cancellationTokenOnAppQuit);

            _joyconConnection_R.SendRumble(1000,0.5f,400,0.5f);

            ConnectInfo_JoyconR = JoyConConnectInfo.JoyConIsReady;
            Debug.Log($"{SerialNumber_R}:Joyconのセットアップが完了しました");
            DebugOnGUI.Log("Joyconのセットアップが完了しました", "Joycon");
        }
        catch(OperationCanceledException e)
        {
            ConnectInfo_JoyconR = JoyConConnectInfo.JoyConIsNotFound;
            DebugOnGUI.Log("JoyConのセットアップに失敗しました", "Joycon");
            Debug.Log("JoyConのセットアップに失敗しました。");
        }

    }

    public static async UniTaskVoid SetupAgain()
    {
        if (ConnectInfo_JoyconR == JoyConConnectInfo.JoyConIsReady)
        {
            await joyConSetUp(cancellationTokenOnAppQuit);
        }
    }

    public override void OnReadReport(string serialNumver,List<byte[]> reports)
    {
        _inputReportsInThisFrame_JoyconR = reports;
        int x30ReportNum = 0;
        foreach (byte[] report in reports)
        {
            if (report != null && report.Length >= 37)
            {
                if (report[0] == 0x30)
                {
                    x30ReportNum++;
                }
            }
        }
        if (x30ReportNum == 0)
        {
            return;
        }

        foreach (byte[] report in reports)
        {
            if (report != null && report.Length >= 37)
            {
                if (report[0] == 0x30)
                {
                    float sec = Time.deltaTime / x30ReportNum;
                    RingconStrain = BitConverter.ToInt16(report, 39);
                    float gyro_x1 = 0.070f * (float)BitConverter.ToInt16(report, 19) ;
                    float gyro_y1 = 0.070f * (float)BitConverter.ToInt16(report, 21) ;
                    float gyro_z1 = 0.070f * (float)BitConverter.ToInt16(report, 23) ;
                    float acc_x1 = 0.000244f * (float)BitConverter.ToInt16(report, 13);
                    float acc_y1 = 0.000244f * (float)BitConverter.ToInt16(report, 15);
                    float acc_z1 = 0.000244f * (float)BitConverter.ToInt16(report, 17);
                    
                    JoyconR_JoyconCoordPose = CorrectPoseByAcc(JoyconR_JoyconCoordPose,new Vector3(acc_x1,acc_y1,acc_z1));
                    JoyconR_JoyconCoordPose = ApplyAngVToPose(JoyconR_JoyconCoordPose, new Vector3(gyro_x1, gyro_y1, gyro_z1), sec/2);
                    JoyconR_JoyconCoordSmoothedPose = ApplyAngVToPose(JoyconR_JoyconCoordSmoothedPose, new Vector3(gyro_x1, gyro_y1, gyro_z1), sec/2);
                    

                    float gyro_x2 = 0.070f * (float)BitConverter.ToInt16(report, 31);
                    float gyro_y2 = 0.070f * (float)BitConverter.ToInt16(report, 33);
                    float gyro_z2 = 0.070f * (float)BitConverter.ToInt16(report, 35);
                    float acc_x2 = 0.000244f * (float)BitConverter.ToInt16(report, 25);
                    float acc_y2 = 0.000244f * (float)BitConverter.ToInt16(report, 27);
                    float acc_z2 = 0.000244f * (float)BitConverter.ToInt16(report, 29);
                    JoyconR_JoyconCoordPose = CorrectPoseByAcc(JoyconR_JoyconCoordPose,new Vector3(acc_x2,acc_y2,acc_z2));
                    JoyconR_JoyconCoordPose = ApplyAngVToPose(JoyconR_JoyconCoordPose, new Vector3(gyro_x2, gyro_y2, gyro_z2), sec/2);
                    JoyconR_JoyconCoordSmoothedPose = ApplyAngVToPose(JoyconR_JoyconCoordSmoothedPose, new Vector3(gyro_x2, gyro_y2, gyro_z2), sec/2);
                    byte buttonStatus = report[3];
                    RButton_JoyconRInput = (buttonStatus & 0b01000000) > 0;
                    ZRButton_JoyconRInput = (buttonStatus & 0b10000000) > 0;
                    AButton_JoyconRInput = (buttonStatus & 0b00001000) > 0;
                }
            }
        }


    }
    
    //poseが角速度angV(単位:deg/sec)でsec秒回転した後の姿勢を返す
    public static Quaternion ApplyAngVToPose(Quaternion pose,Vector3 angV,float sec)
    {

        return (Quaternion.AngleAxis(angV.magnitude*sec*2.6f,pose*angV) * pose);
        //return (Quaternion.AngleAxis(angV.magnitude*sec,pose*angV) * pose);
        
    }
    
    //poseにかかる加速度acc(単位:g)が実際の重力の大きさに近かった場合、accが下方向となるように補正したposeを返す
    public static Quaternion CorrectPoseByAcc(Quaternion pose,Vector3 acc)
    {
        //blend
        float gravityAndAcc_magDiff=Mathf.Abs(1 - (acc.magnitude));
        if (gravityAndAcc_magDiff < 0.01f)
        {
            Vector3 gravityV = pose*(-acc);
            Quaternion switch_correctionRotation=V3_MyUtil.RotateV2V(gravityV,JoyconR_DefaultJoyconCoordDownVec);
            Quaternion switch_correctedPose=switch_correctionRotation*pose;
            Quaternion blend_correctedPose=Quaternion.Slerp(switch_correctedPose,pose,gravityAndAcc_magDiff/0.01f);
            return blend_correctedPose;
        }
        return pose;

        //Switch
        /*
        if (Mathf.Abs(1 - (acc.magnitude)) < 0.001f)
        {
            Vector3 gravityV = pose*(-acc);
            Quaternion correction_rotation=V3_MyUtil.RotateV2V(gravityV,JoyconR_DefaultJoyconCoordDownVec);
            return correction_rotation * pose;
        }
        return pose;
        */
    }

    

    public static async  UniTask SetCalibrationWhenStaticCondition()
    {
        Debug.Log("キャリブレーション開始");

        int counter = 0;
        Vector3 gyroVInStaticCondition = new Vector3(0,0,0);
        float threshold = 0.5f;
        while (counter <= 6)
        {
            if (ConnectInfo_JoyconR != JoyConConnectInfo.JoyConIsReady)
            {
                Debug.Log("キャリブレーション設定中止");
                return;
            }
            if (_inputReportsInThisFrame_JoyconR != null && _inputReportsInThisFrame_JoyconR.Count > 0)
            {

                foreach (byte[] aInputReport in _inputReportsInThisFrame_JoyconR)
                {
                    float gyro_x1 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 19);
                    float gyro_y1 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 21);
                    float gyro_z1 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 23);
                    Vector3 gyroV1 = new Vector3(gyro_x1, gyro_y1, gyro_z1);
                    if ((gyroVInStaticCondition - gyroV1).magnitude < threshold) { counter++; }
                    else { counter = 0; gyroVInStaticCondition = gyroV1; /*Debug.Log("リセット!");*/ }

                    float gyro_x2 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 31);
                    float gyro_y2 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 33);
                    float gyro_z2 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 35);
                    Vector3 gyroV2 = new Vector3(gyro_x2, gyro_y2, gyro_z2);
                    if ((gyroVInStaticCondition - gyroV2).magnitude < threshold) { counter++; }
                    else { counter = 0; gyroVInStaticCondition = gyroV2; /*Debug.Log("リセット!");*/ }
                }
            }
            else
            {
                //Debug.Log("ぬるぬる");
            }
            await UniTask.Yield(cancellationTokenOnAppQuit);
        }

        

        JoyconR_GyroXCalibration = -gyroVInStaticCondition.x;
        JoyconR_GyroYCalibration = -gyroVInStaticCondition.y;
        JoyconR_GyroZCalibration = -gyroVInStaticCondition.z;
        
        Debug.Log($"キャリブレーション設定完了:{new Vector3(JoyconR_GyroXCalibration, JoyconR_GyroYCalibration, JoyconR_GyroZCalibration)}(deg/s)") ;
    }
}

public enum JoyConConnectInfo
{
    JoyConIsNotFound,
    SettingUpJoycon,
    JoyConIsReady
}
