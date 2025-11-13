using UnityEngine;
/// <summary>
/// In 4D, a cone consists of a series of 3D spheres, each with a different radius.
/// Each W value represents a “slice” (sphere) of the cone.
/// </summary>
public class Hypercone4D : _4D_Object
{

    [Header("Cone shape")]
    public float coneSlope = 0.8f;       // cone slope: r = coneSlope * |w|
    public float wMin = 0.05f;           // start of W (to avoid zero perspective)
    public float wMax = 2.0f;            // end of W
    public int wSlices = 18;             // number of slices (spheres) along the W axis (along the cone)

    // These two variables define how many points are generated on the surface of each sphere composing the 4D cone.
    public int latSamples = 10;          // how many divisions along the vertical axis (bottom to top)
    public int lonSamples = 18;          // how many divisions around the horizontal circle (around the axis)

    [Header("Projection & visuals")]
    public float projectionDistance = 3.0f;  // d in perspective projection (d - w)
    public bool usePerspective = true;
    public Color lineColor = new Color(0.2f, 0.8f, 1f, 1f);
    public bool animate = true;

    // Array storing all 4D points for each W slice:
    // samples[i] = list of points (x, y, z, w) for slice i
    Vector4[][] samples;

    void Start()
    {
        rotationSpeed = 0.04f;
        BuildSamples();
    }

    void OnValidate()
    {
        wSlices = Mathf.Max(2, wSlices);
        latSamples = Mathf.Max(2, latSamples);
        lonSamples = Mathf.Max(3, lonSamples);
        projectionDistance = Mathf.Max(0.001f, projectionDistance);
        if (wMin == 0f) wMin = 0.01f;
        BuildSamples();
    }

    /// <summary>
    /// Creates all 4D points of the cone.
    /// For each W value, it builds a 3D sphere (a layer of the cone).
    /// </summary>
    void BuildSamples()
    {
        samples = new Vector4[wSlices][];

        for (int i = 0; i < wSlices; i++)
        {
            // t varies from 0 -> 1 to traverse all slices along W
            float t = (float)i / (wSlices - 1);

            // Interpolate W between wMin and wMax
            float w = Mathf.Lerp(wMin, wMax, t);

            // Calculate the sphere radius for this W value
            float r = coneSlope * Mathf.Abs(w);

            // Each 3D sphere will have (latSamples * lonSamples) points
            Vector4[] pts = new Vector4[latSamples * lonSamples];
            int idx = 0;

            // Create points on the sphere using spherical coordinates:
            // phi = latitude -> angle between -pi/2 (south) and +pi/2 (north)
            // theta = longitude -> angle from 0 to 2pi (around the vertical axis)
            for (int lat = 0; lat < latSamples; lat++)
            {
                // v is the ratio from 0 to 1 along the latitude
                float v = (latSamples == 1) ? 0f : ((float)lat / (latSamples - 1));

                // phi = latitude angle (from -pi/2 to +pi/2)
                float phi = Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, v);

                float cosPhi = Mathf.Cos(phi);
                float sinPhi = Mathf.Sin(phi);

                // For each latitude, go through all longitudes
                for (int lon = 0; lon < lonSamples; lon++)
                {
                    // u = ratio from 0 to 1 for longitude
                    float u = (float)lon / lonSamples;

                    // theta = longitude angle (0 -> 2pi)
                    float theta = u * Mathf.PI * 2f;

                    // Cartesian 3D coordinates for the sphere
                    float x = r * cosPhi * Mathf.Cos(theta);
                    float y = r * cosPhi * Mathf.Sin(theta);
                    float z = r * sinPhi;

                    // Create full 4D point (x, y, z, w)
                    pts[idx++] = new Vector4(x, y, z, w);
                }
            }

            // Store all sphere points in the main list
            samples[i] = pts;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (animate)
        {
            // Placeholder for animation logic
        }

        DrawHypercone();
    }

    /// <summary>
    /// Draws the entire 3D structure resulting from projecting the 4D cone.
    /// </summary>
    void DrawHypercone()
    {

        Color col = lineColor;

        // For each slice (W value)
        for (int i = 0; i < wSlices; i++)
        {
            Vector4[] ringPts = samples[i];
            int vertsPerSlice = ringPts.Length;

            // --------------------------------------------------------------
            // Draw “parallels” (horizontal lines of the sphere)
            // Connect consecutive points on the same latitude.
            // --------------------------------------------------------------
            for (int lat = 0; lat < latSamples; lat++)
            {
                for (int lon = 0; lon < lonSamples; lon++)
                {
                    int a = lat * lonSamples + lon;
                    int b = lat * lonSamples + ((lon + 1) % lonSamples); // wrap-around connection

                    Vector3 pa = ProjectAndRotate4Dto3D(ringPts[a], angle);
                    Vector3 pb = ProjectAndRotate4Dto3D(ringPts[b], angle);

                    RuntimeDraw.DrawLine(transform.TransformPoint(pa), transform.TransformPoint(pb), col);
                }
            }

            // --------------------------------------------------------------
            // Draw vertical lines connecting parallels
            // Connect points between two adjacent latitudes.
            // --------------------------------------------------------------
            for (int lon = 0; lon < lonSamples; lon++)
            {
                for (int lat = 0; lat < latSamples - 1; lat++)
                {
                    int a = lat * lonSamples + lon;
                    int b = (lat + 1) * lonSamples + lon;

                    Vector3 pa = ProjectAndRotate4Dto3D(ringPts[a], angle);
                    Vector3 pb = ProjectAndRotate4Dto3D(ringPts[b], angle);

                    RuntimeDraw.DrawLine(transform.TransformPoint(pa), transform.TransformPoint(pb), col);
                }
            }

            // --------------------------------------------------------------
            // Connect the same position (lat, lon) between two consecutive spheres in W
            // This creates the “walls” of the cone (links between slices).
            // --------------------------------------------------------------
            if (i < wSlices - 1)
            {
                Vector4[] nextPts = samples[i + 1];
                for (int v = 0; v < vertsPerSlice; v++)
                {
                    Vector3 pa = ProjectAndRotate4Dto3D(ringPts[v], angle);
                    Vector3 pb = ProjectAndRotate4Dto3D(nextPts[v], angle);
                    RuntimeDraw.DrawLine(transform.TransformPoint(pa), transform.TransformPoint(pb), col);
                }
            }
        }
    }

    /// <summary>
    /// Rotates a 4D point and projects it into 3D for display.
    /// Rotations are done in the XW and YZ planes for a 4D visual effect.
    /// </summary>
    Vector3 ProjectAndRotate4Dto3D(Vector4 p, float angle)
    {
        float cosA = Mathf.Cos(angle);
        float sinA = Mathf.Sin(angle);

        // Rotate in the XW plane -> mix X and W coordinates
        float x = p.x * cosA - p.w * sinA;
        float w = p.x * sinA + p.w * cosA;

        // Rotate in the YZ plane -> mix Y and Z coordinates
        float y = p.y * cosA - p.z * sinA;
        float z = p.y * sinA + p.z * cosA;

        Vector4 rotated = new Vector4(x, y, z, w);

        // --------------------------------------------------------------
        // PROJECTION 4D → 3D
        // --------------------------------------------------------------
        if (usePerspective)
        {
            // Perspective projection: scales (x, y, z) based on (d - w)
            // When W is close to d, the object appears more distorted.
            float denom = projectionDistance - rotated.w;
            if (Mathf.Abs(denom) < 0.0001f)
                denom = 0.0001f * Mathf.Sign(denom == 0 ? 1f : denom);

            float scale = 1f / denom;
            return new Vector3(rotated.x * scale, rotated.y * scale, rotated.z * scale);
        }
        else
        {
            // Orthographic projection: simply ignore the W component.
            return new Vector3(rotated.x, rotated.y, rotated.z);
        }
    }
}
