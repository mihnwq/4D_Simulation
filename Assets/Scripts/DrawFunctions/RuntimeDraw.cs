using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Robust runtime DrawLine replacement.
/// - Call RuntimeDraw.DrawLine(start, end, color, duration);
/// - Auto-initializes; no need to attach to camera.
/// - Uses OnRenderObject to draw; tries several shader fallbacks.
/// </summary>
public static class RuntimeDraw
{
    private struct LineItem
    {
        public Vector3 a;
        public Vector3 b;
        public Color color;
        public float expiry; // 0 => single-frame, >0 => Time.time + duration
    }

    private static readonly List<LineItem> lines = new List<LineItem>();
    private static Material lineMaterial;
    private static RuntimeBehaviour instance;
    private static bool initialized = false;

    // Public API
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

    // Ensure a GameObject with RuntimeBehaviour exists
    private static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        // Create manager GameObject
        var go = new GameObject("RuntimeDraw_Manager");
        go.hideFlags = HideFlags.HideAndDontSave;
        GameObject.DontDestroyOnLoad(go);
        instance = go.AddComponent<RuntimeBehaviour>();

        // prepare material (we do lazy shader fallback in the behaviour's Start as well)
        CreateMaterialFallback();
    }

    private static void CreateMaterialFallback()
    {
        if (lineMaterial != null) return;

        // Try common candidates (Hidden/Internal-Colored first, then Sprites/Default, then Unlit/Color)
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
                // if shader supports color blending properties we set them (safe to call even if unsupported)
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

    // Internal behaviour attached to the manager GameObject to receive OnRenderObject
    private class RuntimeBehaviour : MonoBehaviour
    {
        void Start()
        {
            // if material not created yet (or was lost), try again
            if (lineMaterial == null)
                CreateMaterialFallback();

            if (lineMaterial == null)
                Debug.LogWarning("[RuntimeDraw] Shader lookup failed in Start; drawing disabled.");
        }

        void OnDisable()
        {
            // keep resources tidy
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

        // Called for each camera when the object is rendered (safest generic hook)
        void OnRenderObject()
        {
            if (lineMaterial == null || lines.Count == 0)
                return;

            float now = Time.time;

            // Build draw list (and prune expired items)
            // We will draw single-frame lines (expiry==0) once and remove them immediately.
            var drawList = new List<LineItem>(lines.Count);

            for (int i = lines.Count - 1; i >= 0; --i)
            {
                var L = lines[i];
                if (L.expiry == 0f)
                {
                    // single-frame -> draw this frame only
                    drawList.Add(L);
                    lines.RemoveAt(i);
                }
                else if (L.expiry > now)
                {
                    // still alive -> draw and keep
                    drawList.Add(L);
                }
                else
                {
                    // expired -> remove
                    lines.RemoveAt(i);
                }
            }

            if (drawList.Count == 0)
                return;

            // draw
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Use modelview/projection from Unity (world-space vertices expected)
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
