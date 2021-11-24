using NarupaIMD;
using NarupaIMD.Interaction;
using UnityEngine;
using ViveSR.anipal.Eye;
using NarupaXR.Interaction;
using Valve.VR;


public class EyeDirection : MonoBehaviour
{
    [Header("Цвет окрашивания")]
    public Color focusColor = Color.red;
    [Space]
    [Header("Отображения взгляда")]
    public bool isDrawRay = false;
    public LineRenderer lineRenderer;
    public float lenght = 25;

    public EyeLogger logger;


    private GameObject focusObject;
    private Color oldColor;
    private FocusInfo focusInfo;

    private void Awake()
    {
        logger = new EyeLogger();
    }

    private void Update()
    {
        EyeFocus();
        if (isDrawRay)
        {
            DrawRay();
        }
    }

    private void EyeFocus()
    {
        if (SRanipal.Focus(out focusInfo))
        {
            GameObject obj = focusInfo.transform.gameObject;

            logger.AddInfo(focusInfo.point.ToString() + " " + obj.GetComponent<AtomInfo>().Index);
            
            // focusObject - предыдущий объект
            if (focusObject != null && obj != null && !GameObject.ReferenceEquals(obj, focusObject))
            {
                // так как ссылки не равны, значит новый объект не равен старому
                // и старому нужно вернуть старый цвет
                Renderer rend = focusObject.GetComponent<Renderer>();

                // вернуть рендеру старого объекта стандартный цвет
                if (rend != null)
                {
                    rend.material.color = oldColor;
                }

                rend = obj.GetComponent<Renderer>();

                if (rend != null)
                {
                    // сохранить старое значение цвета
                    // перед тем, как окрашивать его в другой цвет
                    oldColor = obj.GetComponent<Renderer>().material.color;
                }
            }


            // окрашивание объекта на который смотрим
            if (obj != null)
            {
                focusObject = obj;
                Renderer rend = focusObject.GetComponent<Renderer>();

                if (rend != null)
                {
                    rend.material.color = SetColorAlpha(focusColor, 1f);
                }
            }
        }
        else
        {
            if (focusObject != null)
            {
                // если в поле зрения ничего нет, то вернуть цвет обратно
                Renderer rend = focusObject.GetComponent<Renderer>();

                if (rend != null)
                {
                    rend.material.color = SetColorAlpha(Color.white, 0f);
                }
            }
        }
    }

    private void DrawRay()
    {
        Vector3 origin, direction;

        Ray ray = SRanipal.GetRay(out origin, out direction);

        Vector3 directionCombined = Camera.main.transform.TransformDirection(direction);
        lineRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
        lineRenderer.SetPosition(1, Camera.main.transform.position + directionCombined * lenght);
    }

    private Color SetColorAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}

