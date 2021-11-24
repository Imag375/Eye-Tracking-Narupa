using UnityEngine;
using System.Runtime.InteropServices;
using ViveSR.anipal.Eye;

public class SRanipalBase : MonoBehaviour {

    protected static FocusInfo focusInfo;
    protected readonly float maxDistance = 20;
    protected readonly GazeIndex[] gazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
    protected static EyeData eyeData = new EyeData();
    protected bool eyeCallbackRegistered = false;

    protected static bool eyeFocus = false;
    protected static GameObject focusObject;
    protected static Ray gazeRay;

    protected static Vector3 originRay;
    protected static Vector3 originDirection;

    protected static int layerId = -1;

    private void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eyeCallbackRegistered == false)
        {
            SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eyeCallbackRegistered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eyeCallbackRegistered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eyeCallbackRegistered = false;
        }

        FocusEye();
        FocusRay(out originRay, out originDirection);
    }

    private void Release()
    {
        if (eyeCallbackRegistered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eyeCallbackRegistered = false;
        }
    }

    private static void EyeCallback(ref EyeData eye_data)
    {
        eyeData = eye_data;
    }


    private void FocusEye()
    {
        foreach (GazeIndex index in gazePriority)
        {
            if (layerId != -1)
            {
                if (eyeCallbackRegistered)
                    eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out focusInfo, 0, maxDistance, (1 << layerId), eyeData);
                else
                    eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out focusInfo, 0, maxDistance, (1 << layerId));
            }
            else
            {
                if (eyeCallbackRegistered)
                    eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out focusInfo, 0, maxDistance, eyeData);
                else
                    eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out focusInfo, 0, maxDistance);
            }
        }
    }


    private void FocusRay(out Vector3 originLocal, out Vector3 directionLocal)
    {
        if (eyeCallbackRegistered)
        {
            if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out originLocal, out directionLocal, eyeData)) { }
            else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out originLocal, out directionLocal, eyeData)) { }
            else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out originLocal, out directionLocal, eyeData)) { }
        }
        else
        {
            if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out originLocal, out directionLocal)) { }
            else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out originLocal, out directionLocal)) { }
            else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out originLocal, out directionLocal)) { }
        }
    }
}
