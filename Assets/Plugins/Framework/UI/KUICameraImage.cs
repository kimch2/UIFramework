// ***********************************************************************
// Assembly         : Unity
// Author           : Kimch
// Created          : 
//
// Last Modified By : Kimch
// Last Modified On : 
// ***********************************************************************
// <copyright file= "KUICameraImage" company=""></copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Custom/Camera Image", 12)]
public class KUICameraImage : MaskableGraphic
{
    [SerializeField]
    Texture m_Texture;
    [SerializeField] Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

    protected KUICameraImage()
    {
        useLegacyMeshGeneration = false;
    }

    /// <summary>
    /// Returns the texture used to draw this Graphic.
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (m_Texture == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return m_Texture;
        }
    }

    /// <summary>
    /// Texture to be used.
    /// </summary>
    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// UV rectangle used by the texture.
    /// </summary>
    public Rect uvRect
    {
        get
        {
            return m_UVRect;
        }
        set
        {
            if (m_UVRect == value)
                return;
            m_UVRect = value;
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// Adjust the scale of the Graphic to make it pixel-perfect.
    /// </summary>

    public override void SetNativeSize()
    {
        Texture tex = mainTexture;
        if (tex != null)
        {
            int w = Mathf.RoundToInt(tex.width * uvRect.width);
            int h = Mathf.RoundToInt(tex.height * uvRect.height);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.sizeDelta = new Vector2(w, h);
        }
    }

    protected override void Start()
    {
        StartCoroutine(StartCamera());
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Texture tex = mainTexture;
        vh.Clear();
        if (tex != null)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            var scaleX = tex.width * tex.texelSize.x;
            var scaleY = tex.height * tex.texelSize.y;
            {
                var color32 = color;
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMin * scaleY));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMax * scaleY));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMax * scaleY));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMin * scaleY));

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
        }
    }

    private IEnumerator StartCamera()
    {
        yield return null;
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            color = Color.white;

            var devices = WebCamTexture.devices;
            if (devices != null && devices.Length > 0)
            {
                var size = rectTransform.sizeDelta;
                var camTex = new WebCamTexture((int)size.x, (int)size.y);
                camTex.wrapMode = TextureWrapMode.Repeat;
                texture = camTex;
                camTex.Play();
            }
        }
    }

    public Texture2D GetTexture2D()
    {
        var camTex = texture as WebCamTexture;
        if (camTex != null)
        {
            camTex.Pause();
            var tex = new Texture2D(camTex.width, camTex.height, TextureFormat.RGB24, false);
            tex.SetPixels32(camTex.GetPixels32());
            tex.Apply();
            camTex.Play();
            return tex;
        }
        return null;
    }
}

