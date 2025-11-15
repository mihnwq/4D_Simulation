using UnityEngine;

public class ThreeDFourDConversion : MonoBehaviour
{

    public float rotationSpeed = 1f;

    public float wDistance = 3f; 

    [Header("W Controls")]
    public float wOffset = 0f;          
    public bool wOscillation = false;    
    public float wOscillationAmplitude = 0.5f;
    public float wOscillationSpeed = 2f;

    public bool wDeform = false;         
    public float wDeformStrength = 0.2f;

    private Mesh originalMesh;
    private Mesh workingMesh;

    private Vector4[] baseVertices4D;     
    private Vector4[] currentVertices4D;  

    void Start()
    {

        originalMesh = GetComponent<MeshFilter>().mesh;
        workingMesh = Instantiate(originalMesh);
        GetComponent<MeshFilter>().mesh = workingMesh;


        Vector3[] verts3 = originalMesh.vertices;
        baseVertices4D = new Vector4[verts3.Length];
        currentVertices4D = new Vector4[verts3.Length];

        for (int i = 0; i < verts3.Length; i++)
        {
            baseVertices4D[i] = new Vector4(verts3[i].x, verts3[i].y, verts3[i].z, 0f);
        }
    }

    void Update()
    {
        float t = Time.time * rotationSpeed;

  
        for (int i = 0; i < baseVertices4D.Length; i++)
            currentVertices4D[i] = baseVertices4D[i];



        for (int i = 0; i < currentVertices4D.Length; i++)
        {
            Vector4 v = currentVertices4D[i];


            v.w += wOffset;

            if (wOscillation)
                v.w += Mathf.Sin(Time.time * wOscillationSpeed) * wOscillationAmplitude;

         
            if (wDeform)
                v.w += (v.x + v.y + v.z) * wDeformStrength;

            currentVertices4D[i] = v;
        }

  

        for (int i = 0; i < currentVertices4D.Length; i++)
        {
            Vector4 v = currentVertices4D[i];
            v = RotateXW(v, t * 0.5f); // XW rotation
            v = RotateYW(v, t * 0.3f); // YW rotation
            currentVertices4D[i] = v;
        }


        Vector3[] projected3D = new Vector3[currentVertices4D.Length];
        for (int i = 0; i < currentVertices4D.Length; i++)
        {
            projected3D[i] = ProjectTo3D(currentVertices4D[i]);
        }

        // Update mesh
        workingMesh.vertices = projected3D;
        workingMesh.RecalculateNormals();
        workingMesh.RecalculateBounds();
    }

  

    Vector4 RotateXW(Vector4 v, float angle)
    {
        float c = Mathf.Cos(angle);
        float s = Mathf.Sin(angle);

        float x = v.x * c - v.w * s;
        float w = v.x * s + v.w * c;

        return new Vector4(x, v.y, v.z, w);
    }

    Vector4 RotateYW(Vector4 v, float angle)
    {
        float c = Mathf.Cos(angle);
        float s = Mathf.Sin(angle);

        float y = v.y * c - v.w * s;
        float w = v.y * s + v.w * c;

        return new Vector4(v.x, y, v.z, w);
    }

    Vector3 ProjectTo3D(Vector4 v)
    {
        float k = wDistance;

        float wFactor = k - v.w;
        if (wFactor < 0.01f) wFactor = 0.01f; 

        return new Vector3(
            v.x / wFactor,
            v.y / wFactor,
            v.z / wFactor
        );
    }
}
