using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RTransform_MyUtil
{
    public static (float minx, float miny,float maxx,float maxy) GetCanvasXYRange(RectTransform canvasRect)
    {
        Vector3[] corners = new Vector3[4];
        canvasRect.GetWorldCorners(corners);
        return (corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }

    //RenderModeがOverlayの時に使える。(他は知らん)
    public static Vector3 OverLayRectToScreen(Vector3 WorldPoint, RectTransform canvasRect)
    {
        Vector3[] corners = new Vector3[4];
        canvasRect.GetWorldCorners(corners);
        float minx ,maxx , miny , maxy;
        (minx,miny,maxx,maxy)=GetCanvasXYRange(canvasRect);

        float retx = (WorldPoint.x - minx) / (maxx - minx) * Screen.width;
        float rety = (WorldPoint.y - miny) / (maxy - miny) * Screen.height;
        return new Vector3(retx, rety, 0);
    }
    public static Vector3 OverLayRectToWorld(Vector3 WorldPoint, RectTransform canvasRect,Camera camera,float cameraDist)
    {
        Vector3 screenPos= OverLayRectToScreen(WorldPoint, canvasRect);
        
        return camera.ScreenToWorldPoint(new Vector3(screenPos.x,screenPos.y, cameraDist));
    }
}
