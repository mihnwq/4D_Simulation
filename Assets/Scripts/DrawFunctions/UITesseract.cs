using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UITesseract4D : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tesseract Settings")]
    public float size = 40f;
    public float lineThickness = 2f;

    [Header("Rotation Speeds (4D)")]
    ///I think im better off with only one speed per. axis.
    public float rotXY = 0.7f;
    public float rotXZ = 0.5f;
    public float rotXW = 0.8f;
    public float rotYZ = 0.6f;
    public float rotYW = 0.9f;
    public float rotZW = 0.4f;
    public float rotationSpeed;

    private bool hovered = false;

    private Vector4[] baseVerts;

    protected override void OnEnable()
    {
        base.OnEnable();
        GenerateBaseVerts();
        SetVerticesDirty();
    }
    void GenerateBaseVerts()
    {
        baseVerts = new Vector4[16];
        int i = 0;

        // ALL combinations of (-1 or +1) for x,y,z,w
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    for (int w = -1; w <= 1; w += 2)
                        baseVerts[i++] = new Vector4(x, y, z, w);
    }

    void Update()
    {
        if (hovered)
            SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (baseVerts == null)
            GenerateBaseVerts();

        vh.Clear();

        Vector2[] points2D = new Vector2[16];

        Matrix4x4 R = Rotation4D();

     
        for (int i = 0; i < 16; i++)
        {
            Vector4 v4 = R * baseVerts[i];

         
            float w = 3.0f - v4.w;
            Vector3 v3 = new Vector3(v4.x, v4.y, v4.z) / w;

           
            points2D[i] = new Vector2(v3.x, v3.y) * size;
        }


        for (int i = 0; i < 16; i++)
        {
            for (int bit = 0; bit < 4; bit++)
            {
                int j = i ^ (1 << bit);
                if (j > i)
                    DrawLine(vh, points2D[i], points2D[j]);
            }
        }
    }

    private float rotationAngle = 0f;
    

    Matrix4x4 Rotation4D()
    {
        float t = Time.time;

     //   rotationAngle = Time.time * rotationSpeed;

      /*  float cx = Mathf.Cos(t * rotXY); float sx = Mathf.Sin(t * rotXY);
        float cy = Mathf.Cos(t * rotXZ); float sy = Mathf.Sin(t * rotXZ);
        float cz = Mathf.Cos(t * rotXW); float sz = Mathf.Sin(t * rotXW);
        float cw = Mathf.Cos(t * rotYZ); float sw = Mathf.Sin(t * rotYZ);
        float cu = Mathf.Cos(t * rotYW); float su = Mathf.Sin(t * rotYW);
        float cv = Mathf.Cos(t * rotZW); float sv = Mathf.Sin(t * rotZW);*/

        float cx = Mathf.Cos(rotationAngle); float sx = Mathf.Sin(rotationAngle);
        float cy = Mathf.Cos(rotationAngle); float sy = Mathf.Sin(rotationAngle);
        float cz = Mathf.Cos(rotationAngle); float sz = Mathf.Sin(rotationAngle);
        float cw = Mathf.Cos(rotationAngle); float sw = Mathf.Sin(rotationAngle);
        float cu = Mathf.Cos(rotationAngle); float su = Mathf.Sin(rotationAngle);
        float cv = Mathf.Cos(rotationAngle); float sv = Mathf.Sin(rotationAngle);

        Matrix4x4 R = Matrix4x4.identity;

        // XY
        R = MultiplyRotation(R, 0, 1, cx, sx);
        // XZ
        R = MultiplyRotation(R, 0, 2, cy, sy);
        // XW
        R = MultiplyRotation(R, 0, 3, cz, sz);
        // YZ
        R = MultiplyRotation(R, 1, 2, cw, sw);
        // YW
        R = MultiplyRotation(R, 1, 3, cu, su);
        // ZW
        R = MultiplyRotation(R, 2, 3, cv, sv);

        rotationAngle = Time.time * rotationSpeed;

        return R;
    }

    Matrix4x4 MultiplyRotation(Matrix4x4 M, int a, int b, float c, float s)
    {
        Matrix4x4 R = Matrix4x4.identity;

        R[a, a] = c; R[a, b] = -s;
        R[b, a] = s; R[b, b] = c;

        return M * R;
    }

    private void DrawLine(VertexHelper vh, Vector2 a, Vector2 b)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 n = new Vector2(-dir.y, dir.x) * lineThickness;

        UIVertex v0 = UIVertex.simpleVert;
        UIVertex v1 = UIVertex.simpleVert;
        UIVertex v2 = UIVertex.simpleVert;
        UIVertex v3 = UIVertex.simpleVert;

        v0.color = color; v1.color = color; v2.color = color; v3.color = color;

        v0.position = a - n;
        v1.position = a + n;
        v2.position = b + n;
        v3.position = b - n;

        int idx = vh.currentVertCount;
        vh.AddVert(v0); vh.AddVert(v1); vh.AddVert(v2); vh.AddVert(v3);

        vh.AddTriangle(idx + 0, idx + 1, idx + 2);
        vh.AddTriangle(idx + 2, idx + 3, idx + 0);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovered = false;
    }
}
