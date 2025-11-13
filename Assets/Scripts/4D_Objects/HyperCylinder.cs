using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HyperCylinder : _4D_Object
{
    private List<Vector4> verticies;
    private List<(int, int)> edges;

    // Construction components
    public float radius = 1.0f;     // reasonable unit size in Unity space
    public float height = 2.0f;     // extrusion distance along W
    public int n_theta = 24;        // number of steps around the circular direction
    public int n_phi = 12;          // latitude divisions (vertical sphere detail)
    public int n_h = 8;             // number of slices along the W axis

    // Animation components
    public Color sliceColor = Color.cyan; // easy-to-see default col

    void CreateVertices()
    {
        verticies = new();

        for(int i = 0; i < n_theta; i++)
        {
            float theta = 2 * Mathf.PI * i / n_theta;

            for(int j = 0; j < n_phi; j++)
            {
                float phi = Mathf.PI * j / (n_phi / 2);

                for(int k = 0; k < n_h; k++)
                {
                    float w = -height / 2 + k * height / (n_h - 1);
                    float x = radius * Mathf.Cos(theta) * Mathf.Sin(phi);
                    float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                    float z = radius * Mathf.Cos((float)phi);

                    verticies.Add(new Vector4(x, y, z, w));
                }
            }
        }
    }

    void CreateEdges()
    {
        edges = new();

        int Index(int i, int j, int k, int nTheta, int nPhi, int nW)
            => i * nPhi * nW + j * nW + k;

        for (int i = 0; i < n_theta; i++)
        {
            for (int j = 0; j < n_phi; j++)
            {
                for (int k = 0; k < n_h; k++)
                {
                    int idx = Index(i, j, k, n_theta, n_phi, n_h);

                    edges.Add((idx, Index((i + 1) % n_theta, j, k, n_theta, n_phi, n_h)));
                    if (j + 1 < n_phi)
                        edges.Add((idx, Index(i, j + 1, k, n_theta, n_phi, n_h)));
                    if (k + 1 < n_h)
                        edges.Add((idx, Index(i, j, k + 1, n_theta, n_phi, n_h)));
                }
            }
        }
    }

    private void Start()
    {
        rotationSpeed = 0.05f;
        CreateVertices();
        CreateEdges();
    }
        
    protected override void Update()
    {
        base.Update();

        ///I'm lazy but will put here for the anim


        Vector4[] rotated = new Vector4[verticies.Count];
        for (int i = 0; i < verticies.Count; i++)
        {
            Vector4 p = verticies[i];

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


        for (int i = 0; i < edges.Count; i++)
        {
            Vector4 p1 = rotated[edges[i].Item1];
            Vector4 p2 = rotated[edges[i].Item2];

                Vector3 a = Project(p1);
                Vector3 b = Project(p2);
                RuntimeDraw.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), sliceColor);
            
        }
    }

    Vector3 Project(Vector4 p)
    {
        float w = 1f / (2f - p.w);
        return new Vector3(p.x * w, p.y * w, p.z * w);
    }
}

