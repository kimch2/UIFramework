// ***********************************************************************
// Company          : 
// Author           : KimCh
// Copyright(c)     : KimCh
//
// Last Modified By : KimCh
// Last Modified On : KimCh
// ***********************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Callback = System.Action;

public class KUIManager : MonoBehaviour
{
    #region Static

    /// <summary>
    /// 同步加载
    /// </summary>
    internal static Func<string, GameObject> UILoader;
    /// <summary>
    /// 异步加载
    /// </summary>
    internal static Action<string, Action<GameObject>> UIAsyncLoader;

    private static KUIManager _Instance;
    internal static KUIManager Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject("UIManager").AddComponent<KUIManager>();
            }
            return _Instance;
        }
    }

    public Camera GameCamera
    {
        get; private set;
    }

    public Canvas GameCanvas
    {
        get; private set;
    }

    #endregion

    #region Node Class

    /// <summary>
    /// 
    /// </summary>
    private class Node
    {
        private bool _start;
        private float _time = 1f;

        private KUIWindow _window;

        public KUIWindow window
        {
            get { return _window; }
        }

        public KUIWindow.UILayer uiLayer
        {
            get { return _window.uiLayer; }
        }

        public KUIWindow.UIMode uiMode
        {
            get { return _window.uiMode; }
        }

        public Node(KUIWindow window)
        {
            if (window != null)
            {
                _window = window;
            }
            else
            {
                Debug.Log("[F.UI] window is null.");
            }
        }

        public void Show(bool self = false)
        {
            if (window.gameObject)
            {
                window.gameObject.SetActive(true);
                window.transform.SetAsLastSibling();
                window.OnEnable();
                window.Show(self);
            }
        }

        public void Hide(bool self = false)
        {
            if (window.gameObject && window.gameObject.activeSelf)
            {
                window.gameObject.SetActive(false);
                window.OnDisable();
                window.Hide(self);
            }
        }

        public void Update(float delta)
        {
            if (!_start)
            {
                _start = true;
                _window.Start();
            }
            _window.Update();
            _time += delta;
            if (_time >= 1f)
            {
                _time -= 1f;
                _window.UpdatePerSecond();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private class Tree
    {
        Node _rootNode;
        List<Node> _nodes = new List<Node>();

        public Node rootNode
        {
            get { return _rootNode; }
        }

        public Tree prevTree
        {
            get;
            set;
        }

        public Tree nextTree
        {
            get;
            set;
        }

        public Tree(Node root)
        {
            _rootNode = root;
        }

        /// <summary>
        /// 显示树
        /// </summary>
        public void Show()
        {
            _rootNode.Show();
            for (int i = 0; i < _nodes.Count - 1; i++)
            {
                _nodes[i].Show();
            }
        }

        /// <summary>
        /// 关闭树 删除树所有支节点
        /// </summary>
        public void Hide()
        {
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                _nodes[i].Hide();
            }
            _nodes.Clear();
            _rootNode.Hide();
        }

        public Node Peek()
        {
            if (_nodes.Count > 0)
            {
                return _nodes[_nodes.Count - 1];
            }
            return _rootNode;
        }

        public void Push(Node node)
        {
            var uiLayer = node.uiLayer;
            var uiMode = node.uiMode;

            switch (uiMode)
            {
                case KUIWindow.UIMode.kSequence:
                    _nodes.Add(node);
                    break;
                case KUIWindow.UIMode.kSequenceHide:
                    for (int i = _nodes.Count - 1; i >= 0; i--)
                    {
                        _nodes[i].Hide();
                    }
                    _nodes.Add(node);
                    break;
                case KUIWindow.UIMode.kSequenceRemove:
                    for (int i = _nodes.Count - 1; i >= 0; i--)
                    {
                        _nodes[i].Hide();
                    }
                    _nodes.Clear();
                    _nodes.Add(node);
                    break;
                case KUIWindow.UIMode.kSequenceHideAll:
                    for (int i = _nodes.Count - 1; i >= 0; i--)
                    {
                        _nodes[i].Hide();
                    }
                    rootNode.Hide();
                    _nodes.Add(node);
                    break;
                default:
                    break;
            }
            node.Show(true);
        }

        public void Popup(Node node)
        {
            var count = _nodes.Count;
            if (count > 0 && node == _nodes[count - 1])
            {
                _nodes.RemoveAt(count - 1);
                if (node.uiMode == KUIWindow.UIMode.kSequenceHideAll)
                {
                    rootNode.Show();
                }

                if (count > 1)
                {
                    if (node.uiMode == KUIWindow.UIMode.kSequenceHide || node.uiMode == KUIWindow.UIMode.kSequenceHideAll)
                    {
                        var currNode = _nodes[count - 2];
                        currNode.Show();
                        for (int i = count - 3; i >= 0; i--)
                        {
                            var nextNode = _nodes[i];
                            if (currNode.uiMode == KUIWindow.UIMode.kSequence)
                            {
                                nextNode.Show();
                                currNode = nextNode;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else if (node.uiMode == KUIWindow.UIMode.kSequence)
                    {
                        var currNode = _nodes[count - 2];
                        currNode.Show();
                    }
                }
            }
            node.Hide(true);
        }

        public void Update(float delta)
        {
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                _nodes[i].Update(delta);
            }
            _rootNode.Update(delta);
        }
    }

    #endregion

    #region Field 

    /// <summary>
    /// 当前树
    /// </summary>
    private Tree _currTree;
    /// <summary>
    /// 
    /// </summary>
    private readonly Dictionary<Type, Node> _allNodes = new Dictionary<Type, Node>();

    #endregion

    #region Property 

    #endregion

    #region API Method

    /// <summary>
    /// 获取一个界面
    /// </summary>
    internal KUIWindow GetWindow(Type type)
    {
        Node node;
        if (_allNodes.TryGetValue(type, out node))
        {
            return node.window;
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal KUIWindow GetWindow<T>() where T : KUIWindow
    {
        Node node;
        if (_allNodes.TryGetValue(typeof(T), out node))
        {
            return node.window;
        }
        return null;
    }

    internal KUIWindow[] GetAllWindows()
    {
        var list = KListPool<KUIWindow>.Get();
        foreach (var kv in _allNodes)
        {
            list.Add(kv.Value.window);
        }
        var retArray = list.ToArray();
        KListPool<KUIWindow>.Release(list);
        return retArray;
    }

    internal KUIWindow[] GetActiveWindows()
    {
        var list = KListPool<KUIWindow>.Get();
        foreach (var kv in _allNodes)
        {
            if (kv.Value.window.active)
            {
                list.Add(kv.Value.window);
            }
        }
        var retArray = list.ToArray();
        KListPool<KUIWindow>.Release(list);
        return retArray;
    }

    internal int GetActiveWindowsCount()
    {
        int count = 0;
        foreach (var kv in _allNodes)
        {
            if (kv.Value.window.active)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Show Window
    /// </summary>
    internal void OpenWindow<T>(object data, Callback callback) where T : KUIWindow
    {
        var type = typeof(T);
        Node node;
        if (!_allNodes.TryGetValue(type, out node))
        {
            var window = (KUIWindow)Activator.CreateInstance(type);
            node = new Node(window);
            _allNodes.Add(type, node);
        }
        node.window.data = data;
        CreateNode(node, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal void CloseWindow<T>() where T : KUIWindow
    {
        Node node;
        if (_allNodes.TryGetValue(typeof(T), out node))
        {
            CloseNode(node, null);
        }
    }

    /// <summary>
    /// Close target window
    /// </summary>
    internal void CloseWindow(KUIWindow window)
    {
        if (window == null)
        {
            return;
        }

        Type type = window.GetType();
        Node node;
        if (_allNodes.TryGetValue(type, out node))
        {
            CloseNode(node, null);
        }
    }

    #endregion

    #region Private Method

    /// <summary>
    /// Sync Show UI Logic
    /// </summary>
    private void CreateNode(Node node, Callback callback)
    {
        if (!node.window.gameObject)
        {
            bool async = callback != null;
            if (async)
            {
                StartCoroutine(CreateWindowRoutine(node, callback));
            }
            else
            {
                CreateWindow(node);
            }
        }
        else
        {
            //push node to top if need back.
            PushNode(node);
            //
            //EnableWindow(node.window);

            if (callback != null)
            {
                callback();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="callback"></param>
    private void CloseNode(Node node, Callback callback)
    {
        //set this page's data null when hide.
        node.window.data = null;
        // pop node 
        PopupNode(node);
        // 
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="windowObject"></param>
    private void CreateUI(KUIWindow window, GameObject prefab)
    {
        var gameObject = Instantiate(prefab);

        switch (window.uiLayer)
        {
            case KUIWindow.UILayer.kBackground:
                KUIRoot.Instance.AnchorToBackground(gameObject);
                break;
            case KUIWindow.UILayer.kNormal:
                KUIRoot.Instance.AnchorToNormal(gameObject);
                break;
            case KUIWindow.UILayer.kFloat:
                KUIRoot.Instance.AnchorToFloat(gameObject);
                break;
            case KUIWindow.UILayer.kPopup:
                KUIRoot.Instance.AnchorToPopup(gameObject);
                break;
        }
        window.gameObject = gameObject;
    }
    /// <summary>
    /// 创建window object
    /// </summary>
    /// <param name="window"></param>
    private void CreateWindow(Node node)
    {
        if (string.IsNullOrEmpty(node.window.uiPath))
        {
#if DEBUG_MY
            Debug.LogError(string.Format("[F.UI].[{0}] ui path is null.", node.window.GetType()));
#endif
            return;
        }

        var prefab = UILoader(node.window.uiPath);

        if (prefab == null)
        {
#if DEBUG_MY
            Debug.LogError(string.Format("[F.UI].[{0}] can't load ui prefab.", node.window.GetType()));
#endif
            return;
        }

        CreateUI(node.window, prefab);
        node.window.asyncUI = false;
        node.window.Awake();

        //push node to top if need back.
        PushNode(node);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator CreateWindowRoutine(Node node, Callback callback)
    {
        if (string.IsNullOrEmpty(node.window.uiPath))
        {
#if DEBUG_MY
            Debug.LogError("[F.UI] ui path is null.");
#endif
            yield break;
        }

        bool loading = true;
        if (UIAsyncLoader != null)
        {
            UIAsyncLoader(node.window.uiPath, (prefab) =>
            {
                loading = false;

                if (prefab == null)
                {
#if DEBUG_MY
                    Debug.LogError("[F.UI] can't load ui prefab.");
#endif
                    return;
                }

                CreateUI(node.window, prefab);
                node.window.asyncUI = true;
                node.window.Awake();

                //push node to top if need back.
                PushNode(node);

                if (callback != null)
                {
                    callback();
                }
            });
        }

        float rTime = Time.realtimeSinceStartup;
        while (loading)
        {
            if (Time.realtimeSinceStartup - rTime > 6f)
            {
#if DEBUG_MY
                Debug.LogError("[F.UI] async load ui prefab timeout!");
#endif
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    private void DestroyWindow(KUIWindow window)
    {
        if (window != null)
        {
            _allNodes.Remove(window.GetType());
            if (window.gameObject)
            {
                Destroy(window.gameObject);
            }
            window.OnDestroy();
        }
    }

    #endregion

    #region Node Method

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Node PeekNode()
    {
        return _currTree != null ? _currTree.Peek() : null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private void PopupNode(Node node)
    {
        var uiLayer = node.uiLayer;
        switch (uiLayer)
        {
            case KUIWindow.UILayer.kBackground:
                if (_currTree != null)
                {
                    if (_currTree.rootNode == node)
                    {
                        _currTree.Hide();
                        if (_currTree.prevTree != null)
                        {
                            _currTree = _currTree.prevTree;
                            _currTree.Show();
                        }
                    }
                }
                break;
            case KUIWindow.UILayer.kNormal:
            case KUIWindow.UILayer.kPopup:
                if (_currTree != null)
                {
                    _currTree.Popup(node);
                }
                break;
        }
    }

    /// <summary>
    /// 处理节点顺序
    /// </summary>
    /// <param name="node"></param>
    private void PushNode(Node node)
    {
        var uiLayer = node.uiLayer;
        switch (uiLayer)
        {
            case KUIWindow.UILayer.kBackground:
                if (_currTree != null)
                {
                    if (_currTree.rootNode != node)
                    {
                        var nextTree = new Tree(node)
                        {
                            prevTree = _currTree
                        };
                        _currTree.Hide();
                        _currTree = nextTree;
                        _currTree.Show();
                    }
                }
                else
                {
                    _currTree = new Tree(node);
                    _currTree.Show();
                }
                break;
            case KUIWindow.UILayer.kNormal:
            case KUIWindow.UILayer.kPopup:
                if (_currTree != null)
                {
                    _currTree.Push(node);
                }
                break;
        }
    }

    #endregion

    #region Unity 

    // Use this for initialization
    private void Awake()
    {
        _Instance = this;
        if (!KUIRoot.Instance)
        {
        }
        GameCamera = GetComponentInChildren<Camera>();
        GameCanvas = GetComponentInChildren<Canvas>();
    }

    private List<Node> _nodeHelper = new List<Node>();
    // Update is called once per frame
    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        if (deltaTime > 0f)
        {
            _nodeHelper.Clear();
            foreach (var item in _allNodes)
            {
                if (item.Value.window.active)
                {
                    _nodeHelper.Add(item.Value);
                }
            }

            foreach (var node in _nodeHelper)
            {
                node.Update(deltaTime);
            }
        }
    }

    #endregion
}
