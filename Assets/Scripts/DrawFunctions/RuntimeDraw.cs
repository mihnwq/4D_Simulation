using System.Collections.Generic;
using UnityEngine;

public static class RuntimeDraw
{
    private struct LineItem
    {
        public Vector3 a;
        public Vector3 b;
        public Color color;
        public float expiry;
    }

    private static readonly List<LineItem> lines = new List<LineItem>();
    private static Material lineMaterial;
    private static RuntimeBehaviour instance;
    private static bool initialized = false;

    public static void DrawLine(Vector3 a, Vector3 b, Color color, float duration = 0f)
    {
        EnsureInitialized();

        var li = new LineItem
        {
            a = a,
            b = b,
            color = color,
            expiry = (duration > 0f) ? Time.time + duration : 0f
        };
        lines.Add(li);
    }


    private static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;


        var go = new GameObject("RuntimeDraw_Manager");
        go.hideFlags = HideFlags.HideAndDontSave;
        GameObject.DontDestroyOnLoad(go);
        instance = go.AddComponent<RuntimeBehaviour>();


        CreateMaterialFallback();
    }

    private static void CreateMaterialFallback()
    {
        if (lineMaterial != null) return;


        string[] shaderNames = new string[] {
            "Hidden/Internal-Colored",
            "Sprites/Default",
            "Unlit/Color",
            "Unlit/Texture" // last resort
        };

        foreach (var s in shaderNames)
        {
            var shader = Shader.Find(s);
            if (shader != null)
            {
                lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

                try
                {
                    lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    lineMaterial.SetInt("_ZWrite", 0);
                }
                catch { }
                break;
            }
        }

        if (lineMaterial == null)
        {
            Debug.LogWarning("[RuntimeDraw] No suitable shader found. Lines will not render. Tried Hidden/Internal-Colored, Sprites/Default, Unlit/Color.");
        }
    }


    private class RuntimeBehaviour : MonoBehaviour
    {
        void Start()
        {

            if (lineMaterial == null)
                CreateMaterialFallback();

            if (lineMaterial == null)
                Debug.LogWarning("[RuntimeDraw] Shader lookup failed in Start; drawing disabled.");
        }

        void OnDisable()
        {

            if (lineMaterial != null)
            {
                Object.Destroy(lineMaterial);
                lineMaterial = null;
            }
            initialized = false;
        }

        void OnDestroy()
        {
            OnDisable();
        }


        void OnRenderObject()
        {
            if (lineMaterial == null || lines.Count == 0)
                return;

            float now = Time.time;


            var drawList = new List<LineItem>(lines.Count);

            for (int i = lines.Count - 1; i >= 0; --i)
            {
                var L = lines[i];
                if (L.expiry == 0f)
                {

                    drawList.Add(L);
                    lines.RemoveAt(i);
                }
                else if (L.expiry > now)
                {
                
                    drawList.Add(L);
                }
                else
                {
                 
                    lines.RemoveAt(i);
                }
            }

            if (drawList.Count == 0)
                return;

            // draw
            lineMaterial.SetPass(0);

            GL.PushMatrix();

            GL.Begin(GL.LINES);
            for (int i = 0; i < drawList.Count; i++)
            {
                var L = drawList[i];
                GL.Color(L.color);
                GL.Vertex(L.a);
                GL.Vertex(L.b);
            }
            GL.End();
            GL.PopMatrix();
        }
    }
}
