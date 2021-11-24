using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class SRanipal : SRanipalBase {
    [Header("Слой фокуса глаз")]
    public string layer;

    private void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        if (layer.Trim().Length > 0)
            layerId = LayerMask.NameToLayer(layer);
    }

    public static bool Focus()
    {
        return eyeFocus;
    }

    public static bool Focus(out FocusInfo focusInfo)
    {
        focusInfo = SRanipal.focusInfo;
        return eyeFocus;
    }

    public static EyeData GetEyeData()
    {
        return eyeData;
    }

    public static Ray GetRay()
    {
        return gazeRay;
    }

    public static Ray GetRay(out Vector3 originLocal, out Vector3 directionLocal)
    {
        originLocal = originRay;
        directionLocal = originDirection;
        return gazeRay;
    }
}
