// ***********************************************************************
// Company          : 
// Author           : KimCh
// Copyright(c)     : KimCh
//
// Last Modified By : 
// Last Modified On : 
// ***********************************************************************
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Init The UI Root
/// </summary>
public class KUIRoot : MonoBehaviour
{
    private const int kScreenWidth = 1920;
    private const int kScreenHeight = 1080;

    #region Field

    /// <summary>
    /// 背景层
    /// </summary>
    [SerializeField]
    private Transform _backgroundLayer;
    /// <summary>
    /// 标准层
    /// </summary>
    [SerializeField]
    private Transform _normalLayer;
    /// <summary>
    /// 浮动层
    /// </summary>
    [SerializeField]
    private Transform _floatLayer;
    /// <summary>
    /// 弹框层
    /// </summary>
    [SerializeField]
    private Transform _popupLayer;

    #endregion

    #region Property  

    [SerializeField]
    private Camera _uiCamera;
    public Camera uiCamera
    {
        get
        {
            return _uiCamera;
        }
        private set
        {
            if (value)
            {
                _uiCamera = value;
            }
        }
    }

    [SerializeField]
    private Canvas _uiCanvas;
    public Canvas uiCanvas
    {
        get
        {
            return _uiCanvas;
        }
        private set
        {
            if (value)
            {
                _uiCanvas = value;
            }
        }
    }

    public static Camera uiRootCamera
    {
        get { return _Instance.uiCamera; }
    }

    public static Canvas uiRootCanvas
    {
        get { return _Instance.uiCanvas; }
    }

    #endregion

    #region Public API

    /// <summary>
    /// 屏幕坐标转换到UI坐标
    /// </summary>
    /// <param name="screenPoint"></param>
    /// <returns></returns>
    public static Vector2 ScreenPointToLocalPointInRectangle(Vector2 screenPoint)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)uiRootCanvas.transform, screenPoint, uiRootCamera, out localPoint);
        return localPoint;
    }

    /// <summary>
    /// 鼠标坐标转换的屏幕坐标
    /// </summary>
    /// <returns></returns>
    public static Vector2 MousePointToLocalPointInRectangle()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)uiRootCanvas.transform, Input.mousePosition, uiRootCamera, out localPoint);
        return localPoint;
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    internal void AnchorToBackground(GameObject window)
    {
        AnchorUI(window, _backgroundLayer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    internal void AnchorToNormal(GameObject window)
    {
        AnchorUI(window, _normalLayer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    internal void AnchorToPopup(GameObject window)
    {
        AnchorUI(window, _popupLayer);
    }

    internal void AnchorToFloat(GameObject window)
    {
        AnchorUI(window, _floatLayer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="parent"></param>
    private void AnchorUI(GameObject window, Transform parent)
    {
        var windowTransform = window.transform;
        var windowRectTransform = windowTransform as RectTransform;

        Vector3 anchoredPosition = Vector3.zero;
        Vector2 sizeDelta = Vector2.zero;
        Vector3 scale = Vector3.one;

        if (windowRectTransform)
        {
            anchoredPosition = windowRectTransform.anchoredPosition;
            sizeDelta = windowRectTransform.sizeDelta;
            scale = windowRectTransform.localScale;
        }
        else
        {
            anchoredPosition = windowTransform.localPosition;
            scale = windowTransform.localScale;
        }

        windowTransform.SetParent(parent, false);

        if (windowRectTransform)
        {
            windowRectTransform.anchoredPosition = anchoredPosition;
            windowRectTransform.sizeDelta = sizeDelta;
            windowRectTransform.localScale = scale;
        }
        else
        {
            windowTransform.localPosition = anchoredPosition;
            windowTransform.localScale = scale;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CreateRoot()
    {
        var go = this.gameObject;

        go.name = "UIRoot";
        go.layer = LayerMask.NameToLayer("UI");
        if (!go.GetComponent<RectTransform>())
        {
            go.AddComponent<RectTransform>();
        }

        if (!this.uiCamera)
        {
            ///Camera
            var cameraObj = new GameObject("UICamera")
            {
                layer = go.layer
            };
            cameraObj.transform.SetParent(go.transform, false);
            cameraObj.transform.localPosition = new Vector3(0, 0, -1000f);

            ////Camera
            var camera = cameraObj.AddComponent<Camera>();
            this.uiCamera = camera;

            camera.clearFlags = CameraClearFlags.Depth;
            camera.depth = 10;
            camera.orthographic = true;
            camera.cullingMask = 1 << go.layer;
            camera.nearClipPlane = 0f;
            camera.farClipPlane = 100f;
        }

        if (!this.uiCanvas)
        {
            ////Canvas
            var canvas = go.AddComponent<Canvas>();
            this.uiCanvas = canvas;

            canvas.pixelPerfect = false;
            canvas.worldCamera = this.uiCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            int sw = Screen.width;
            int sh = Screen.height;
            float sr = sw / (float)sh;
            int rw = (int)(sr * kScreenHeight);
            int rh = kScreenHeight;
            ////Canvas Scaler
            var canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referenceResolution = new Vector2(rw, rh);
        }

        ////Sub
        if (!_backgroundLayer)
        {
            _backgroundLayer = this.CreateSubRoot("Background", 100).transform;
        }
        if (!_normalLayer)
        {
            _normalLayer = this.CreateSubRoot("Normal", 200).transform;
        }
        if (!_floatLayer)
        {
            _floatLayer = this.CreateSubRoot("Float", 280).transform;
        }
        if (!_popupLayer)
        {
            _popupLayer = this.CreateSubRoot("Popup", 300).transform;
        }

        //Add Event System
        if (!EventSystem.current)
        {
            var eventObj = new GameObject(typeof(EventSystem).Name)
            {
                layer = go.layer
            };
            eventObj.transform.SetParent(go.transform, false);
            eventObj.AddComponent<EventSystem>();
            eventObj.AddComponent<StandaloneInputModule>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    private GameObject CreateSubRoot(string name, int sort)
    {
        var go = new GameObject(name)
        {
            layer = this.gameObject.layer
        };

        go.transform.SetParent(this.transform, false);
        go.transform.localPosition = new Vector3(0, 0, -sort);

        var canvas = go.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sort;

        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;

        go.AddComponent<GraphicRaycasterEx>();

        return go;
    }

    #endregion

    #region Unity

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (_Instance)
        {
            _Instance = null;
        }
        _Instance = this;
        this.CreateRoot();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        _Instance = null;
    }

    #endregion

    #region Static

    private static KUIRoot _Instance;

    public static KUIRoot Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject("UIRoot").AddComponent<KUIRoot>();
            }
            return _Instance;
        }
    }

    #endregion
}
