using UnityEngine;
/// <summary>
/// In 4D, un con este format dintr-o serie de sfere 3D, fiecare cu o raza diferita,
/// Fiecare valoare W reprezinta o „felie” (sfera) din con.
/// </summary>
public class Hypercone4D : MonoBehaviour
{
    [Header("Cone shape")]
    public float coneSlope = 0.8f;       // panta conului: r = coneSlope * |w|
    public float wMin = 0.05f;           // inceputul pt. W (ca sa nu fie perspectiva zero)
    public float wMax = 2.0f;            // sf. lui W
    public int wSlices = 18;             // nr. de felii (sfere) pe axa W (dea lungul axei conului)

    // Aceste doua variabile definesc cat de multe puncte sunt generate pe suprafata fiecarei sfere care compune conul 4D.
    public int latSamples = 10;          // cate divizii are axa verticala (de jos in sus)
    public int lonSamples = 18;          // cate divizii are cercul orizontal (in jurul axei)

    [Header("Projection & visuals")]
    public float projectionDistance = 3.0f;  // d in perspective projection (d - w)
    public bool usePerspective = true;
    public Color lineColor = new Color(0.2f, 0.8f, 1f, 1f);
    public float timeRotateSpeed = 15f;   //animatie pt. a rotii in planele XW si YZ.
    public bool animate = true;

    // Tablou care stocheaza toate punctele 4D pentru fiecare felie W:
    // samples[i] = lista de puncte (x, y, z, w) pentru felia i
    Vector4[][] samples;

    void Start()
    {
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
    /// Creeaza toate punctele 4D ale conului.
    /// Pentru fiecare valoare W, se construieste o sfera 3D (un strat al conului).
    /// </summary>
    void BuildSamples()
    {
        samples = new Vector4[wSlices][];

        for (int i = 0; i < wSlices; i++)
        {
            // t variaza de la 0 -> 1 pt. a parcurge toate felii de pe axa W
            float t = (float)i / (wSlices - 1);

            // Interpolam valoarea W intre wMin si wMax
            float w = Mathf.Lerp(wMin, wMax, t);

            // Calculam raza sferei 3D pentru aceasta valoare de W
            float r = coneSlope * Mathf.Abs(w);

            // Fiecare sfera 3D va avea (latSamples * lonSamples) puncte
            Vector4[] pts = new Vector4[latSamples * lonSamples];
            int idx = 0;

           
            // Cream punctele de pe sfera in coordonate sferice:
            // phi = latitudine -> unghiul dintre -pi/2 (sud) si +pi/2 (nord)
            // theta = longitudine -> unghiul de la 0 la 2pi (in jurul axei verticale)
            for (int lat = 0; lat < latSamples; lat++)
            {
                // v este raportul dintre 0 si 1 dea lungul latitudinii
                float v = (latSamples == 1) ? 0f : ((float)lat / (latSamples - 1));

                // phi = unghiul de latitudine (de la -pi/2 la +pi/2)
                float phi = Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, v);

                float cosPhi = Mathf.Cos(phi);
                float sinPhi = Mathf.Sin(phi);

                // Pentru fiecare latitudine, parcurgem toate longitudinile
                for (int lon = 0; lon < lonSamples; lon++)
                {
                    // u = raportul dintre 0 si 1 pentru longitudine
                    float u = (float)lon / lonSamples;

                    // theta = unghiul de longitudine (0 -> 2pi)
                    float theta = u * Mathf.PI * 2f;

                    // Coordonate carteziene 3D pt sfera
                    float x = r * cosPhi * Mathf.Cos(theta);
                    float y = r * cosPhi * Mathf.Sin(theta);
                    float z = r * sinPhi;

                    // Formam punctul complet 4D (x, y, z, w)
                    pts[idx++] = new Vector4(x, y, z, w);
                }
            }

            // Stocam toate punctele sferei in lista principală
            samples[i] = pts;
        }
    }

    void Update()
    {
        if (animate)
        {
            
        }

        DrawHypercone();
    }

    /// <summary>
    /// Deseneaza intreaga structura 3D rezultata din proiectia conului 4D.
    /// </summary>
    void DrawHypercone()
    {
        float angle = (animate ? Time.time * Mathf.Deg2Rad * timeRotateSpeed : 0f);
        
        Color col = lineColor;

        // Pt fiecare felie (valoare W)
        for (int i = 0; i < wSlices; i++)
        {
            Vector4[] ringPts = samples[i];
            int vertsPerSlice = ringPts.Length;

            // --------------------------------------------------------------
            // Desenam „paralelele” (liniile orizontale ale sferei)
            //    Adica legam punctele consecutive pe aceeasi latitudine.
            // --------------------------------------------------------------
            for (int lat = 0; lat < latSamples; lat++)
            {
                for (int lon = 0; lon < lonSamples; lon++)
                {
                    int a = lat * lonSamples + lon;
                    int b = lat * lonSamples + ((lon + 1) % lonSamples); // conectam capetele (wrap-around)

                    Vector3 pa = ProjectAndRotate4Dto3D(ringPts[a], angle);
                    Vector3 pb = ProjectAndRotate4Dto3D(ringPts[b], angle);

                    Debug.DrawLine(transform.TransformPoint(pa), transform.TransformPoint(pb), col);
                }
            }

            // --------------------------------------------------------------
            //  Desenam liniile verticale care unesc paralelele
            //    Conecteaza punctele intre doua latitudini adiacente.
            // --------------------------------------------------------------
            for (int lon = 0; lon < lonSamples; lon++)
            {
                for (int lat = 0; lat < latSamples - 1; lat++)
                {
                    int a = lat * lonSamples + lon;
                    int b = (lat + 1) * lonSamples + lon;

                    Vector3 pa = ProjectAndRotate4Dto3D(ringPts[a], angle);
                    Vector3 pb = ProjectAndRotate4Dto3D(ringPts[b], angle);

                    Debug.DrawLine(transform.TransformPoint(pa), transform.TransformPoint(pb), col);
                }
            }

            // --------------------------------------------------------------
            //  Conectam aceeasi pozitie (lat, lon) intre doua sfere consecutive în W
            //    Aceasta creeaza „peretii” conului (legaturile intre felii).
            // --------------------------------------------------------------
            if (i < wSlices - 1)
            {
                Vector4[] nextPts = samples[i + 1];
                for (int v = 0; v < vertsPerSlice; v++)
                {
                    Vector3 pa = ProjectAndRotate4Dto3D(ringPts[v], angle);
                    Vector3 pb = ProjectAndRotate4Dto3D(nextPts[v], angle);
                    Debug.DrawLine(transform.TransformPoint(pa), transform.TransformPoint(pb), col);
                }
            }
        }
    }

    /// <summary>
    /// Roteste un punct 4D si il proiecteaza in 3D pentru afisare.
    /// Rotatiile se fac in planurile XW si YZ pentru efect vizual 4D.
    /// </summary>
    Vector3 ProjectAndRotate4Dto3D(Vector4 p, float angle)
    {
        float cosA = Mathf.Cos(angle);
        float sinA = Mathf.Sin(angle);

        // Rotim in planul XW -> amestecam coordonatele X si W
        float x = p.x * cosA - p.w * sinA;
        float w = p.x * sinA + p.w * cosA;

        // Rotim in planul YZ -> amestecam coordonatele Y si Z
        float y = p.y * cosA - p.z * sinA;
        float z = p.y * sinA + p.z * cosA;

        Vector4 rotated = new Vector4(x, y, z, w);

        // --------------------------------------------------------------
        // PROIECTIE 4D → 3D
        // --------------------------------------------------------------
        if (usePerspective)
        {
            // Proiectie in perspectiva: scaleaza (x, y, z) in functie de (d - w)
            // Cand W este aproape de d, obiectul „se deformeaza” mai puternic.
            float denom = projectionDistance - rotated.w;
            if (Mathf.Abs(denom) < 0.0001f)
                denom = 0.0001f * Mathf.Sign(denom == 0 ? 1f : denom);

            float scale = 1f / denom;
            return new Vector3(rotated.x * scale, rotated.y * scale, rotated.z * scale);
        }
        else
        {
            // Proiectie ortografica: pur si simplu ignoram componenta W.
            return new Vector3(rotated.x, rotated.y, rotated.z);
        }
    }
    

    
}
