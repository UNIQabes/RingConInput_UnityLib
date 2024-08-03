using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;


public static class MyMarshal 
{
    public static string bytesToStr(byte[] bytes)
    {
        string retV = "";
        foreach (byte aByte in bytes)
        {
            retV += aByte.ToString() + " ";
        }
        return retV;
    }
    public static string bytesToStrUTF32(byte[] bytes)
    {
        string retV = "";
        int lastCode = -1;
        for (int i = 0; i <= bytes.Length - 4; i += 4)
        {

            int utf32Code = BitConverter.ToInt32(bytes, i);
            //0x000000 and 0x10ffff
            if (0x000000 <= utf32Code & utf32Code <= 0x10ffff & utf32Code != 0)
            {
                lastCode = utf32Code;
                retV += Char.ConvertFromUtf32(utf32Code);
            }
            else
            {
                break;
            }
        }
        //Debug.Log(lastCode);
        //Debug.Log(Char.ConvertFromUtf32(lastCode));
        return retV;
    }

    public static string intPtrToStrUtf32(IntPtr intPtr, int maxlen)
    {

        string retV = "";
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        byte[] wcharBuf = new byte[4];
        IntPtr curPtr = intPtr;
        while (true)
        {
            int counter = 0;
            Marshal.Copy(curPtr, wcharBuf, 0, wcharBuf.Length);
            int utf32Code = BitConverter.ToInt32(wcharBuf, 0);
            //UTF32の文字コードの取り得る値の範囲に収まっているか? & utf32Codeがnull文字じゃないか? & 文字数の最大値に達していないか?
            if (0x000000 <= utf32Code & utf32Code <= 0x10ffff & utf32Code != 0 & counter <= maxlen)
            {
                retV += Char.ConvertFromUtf32(utf32Code);
                counter++;
            }
            else
            {
                break;
            }
            curPtr = IntPtr.Add(curPtr, 4);
        }
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        retV = Marshal.PtrToStringAnsi(intPtr);
#endif

        return retV;
    }


}
