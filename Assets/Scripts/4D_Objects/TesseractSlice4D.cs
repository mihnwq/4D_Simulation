using UnityEngine;

public class TesseractSlice4D : _4D_Object
{
    [Range(-2f, 2f)]
    public float wSlice = 0f; 

    public float size = 1f;
    public float sliceThickness = 0.5f;
    public Color sliceColor = Color.cyan;

    private Vector4[] points;
    private readonly int[,] edges =
    {
        {0,1},{1,3},{3,2},{2,0},
        {4,5},{5,7},{7,6},{6,4},
        {0,4},{1,5},{2,6},{3,7},
        {8,9},{9,11},{11,10},{10,8},
        {12,13},{13,15},{15,14},{14,12},
        {8,12},{9,13},{10,14},{11,15},
        {0,8},{1,9},{2,10},{3,11},
        {4,12},{5,13},{6,14},{7,15}
    };

    void Start()
    {
        
        
        points = new Vector4[16];
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    for (int w = -1; w <= 1; w += 2)
                        points[i++] = new Vector4(x, y, z, w) * size;
    }

    protected override void Update()
    {

        base.Update();

      

        
        Vector4[] rotated = new Vector4[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector4 p = points[i];

            // XW rotation
            float x = p.x * Mathf.Cos(angle) - p.w * Mathf.Sin(angle);
            float w = p.x * Mathf.Sin(angle) + p.w * Mathf.Cos(angle);
            p.x = x; p.w = w;

            // YZ rotation
            float y = p.y * Mathf.Cos(angle) - p.z * Mathf.Sin(angle);
            float z = p.y * Mathf.Sin(angle) + p.z * Mathf.Cos(angle);
            p.y = y; p.z = z;

            rotated[i] = p;
        }

        
        for (int i = 0; i < edges.GetLength(0); i++)
        {
            Vector4 p1 = rotated[edges[i, 0]];
            Vector4 p2 = rotated[edges[i, 1]];

           
            if (Mathf.Abs(p1.w - wSlice) < sliceThickness || Mathf.Abs(p2.w - wSlice) < sliceThickness)
            {
               
                Vector3 a = Project(p1);
                Vector3 b = Project(p2);
                RuntimeDraw.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), sliceColor);
            }
        }

        
        if (Input.GetKey(KeyCode.LeftArrow)) wSlice -= Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) wSlice += Time.deltaTime;
    }

    Vector3 Project(Vector4 p)
    {
        float w = 1f / (2f - p.w);
        return new Vector3(p.x * w, p.y * w, p.z * w);
    }
}

