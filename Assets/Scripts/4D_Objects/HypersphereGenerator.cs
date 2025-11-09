using UnityEngine;

public class AnimatedHypersphereDebug : _4D_Object
{

    [Range(4, 50)] public int psiSegments = 12; // Number of divisions along the psi angle. Controls grid density in the psi direction.
    [Range(4, 50)] public int thetaSegments = 12; // Number of divisions along the theta angle. Controls grid density in the theta direction.
    [Range(4, 50)] public int phiSegments = 12; // Number of divisions along the phi angle. Controls grid density in the phi direction.
    public float radius = 1f; // The radius of the 4D hypersphere (S^3). Scales the overall size of the shape.

    public float projectionScale = 2f; // A scale multiplier applied to the final 3D projected coordinates to control visual size.
    public float maxDistanceClamp = 30f; // Limits how far projected points can go in 3D space to prevent extreme stretching.


    public bool rotateXW = true; // Toggles rotation in the 4D X–W plane. This mixes x and w coordinates.
    public bool rotateYW = true; // Toggles rotation in the Y–W plane. This mixes y and w coordinates.
    public bool rotateZW = false; // Toggles rotation in the Z–W plane. This mixes z and w coordinates.
    public bool rotateXY = false; // Toggles rotation in the X–Y plane. This mixes x and y coordinates (a normal 3D-like rotation).


    public Color lineColor = Color.magenta; // The color used when drawing debug lines in Unity’s Scene view.
    public float lineDuration = 0f; // How long lines remain visible. 0 means they appear for one frame only and are redrawn every update.

    private void Start()
    {
        animationSpeed = 1.0f;
    }

    void Update()
    {
        DrawHypersphere(Time.time * animationSpeed); // Each frame, draw the hypersphere with a time-dependent rotation.
    }

    void DrawHypersphere(float t)
    {
        // This function draws the 3D projection of a 4D hypersphere (S3) at a given rotation time 't'.
        // It builds the hypersphere using three nested angular loops: psi, theta, and phi.
        // For each combination of these angles, it calculates 4D points, rotates them in 4D space,
        // projects them into 3D, and connects them with lines to form a visible grid.

        // Loop over psi angle divisions (0 to pi). This defines one dimension of the hypersphere grid.
        for (int i = 0; i < psiSegments; i++)
        {
            float psi0 = Mathf.PI * i / psiSegments;   // psi0 is the starting psi angle of the current segment.
            float psi1 = Mathf.PI * (i + 1) / psiSegments; // psi1 is the next psi angle (end of the segment).
                                                           // psi controls one of the hypersphere’s angular coordinates, similar to latitude in 3D.

            // Loop over theta angle divisions (0 to π). This defines another angular dimension.
            for (int j = 0; j < thetaSegments; j++)
            {
                float theta0 = Mathf.PI * j / thetaSegments;   // theta0 = starting theta angle.
                float theta1 = Mathf.PI * (j + 1) / thetaSegments; // theta1 = next theta angle.
                                                                   // theta defines another layer of the hypersphere, like a longitude division on a globe.

                // Loop over phi angle divisions (0 to 2π). This is the third angular coordinate.
                for (int k = 0; k < phiSegments; k++)
                {
                    float phi0 = 2 * Mathf.PI * k / phiSegments;   // phi0 = starting phi angle.
                    float phi1 = 2 * Mathf.PI * (k + 1) / phiSegments; // phi1 = next phi angle.
                                                                       // phi completes the 4D angular coordinate system, wrapping around the shape fully (0 → 360°).

                    // Compute 4D Cartesian coordinates (x, y, z, w) of four neighboring points on the hypersphere.
                    // These four points represent a small grid patch defined by psi, theta, and phi differences.
                    Vector4 p000 = S3ToR4(psi0, theta0, phi0); // base corner of the patch
                    Vector4 p100 = S3ToR4(psi1, theta0, phi0); // moved one step in psi direction
                    Vector4 p010 = S3ToR4(psi0, theta1, phi0); // moved one step in theta direction
                    Vector4 p001 = S3ToR4(psi0, theta0, phi1); // moved one step in phi direction
                                                               // Together, these define a small section of the hypersphere surface in 4D space.

                    // Apply 4D rotation to each point.
                    // The rotation is controlled by time 't' and applies rotations in XW, YW, ZW, or XY planes.
                    // This rotation simulates movement of the hypersphere in 4D space.
                    p000 = Rotate4D(p000, t);
                    p100 = Rotate4D(p100, t);
                    p010 = Rotate4D(p010, t);
                    p001 = Rotate4D(p001, t);
                    // As time changes, these rotations cause the projected 3D shape to appear to twist and pulse.

                    // Project the rotated 4D points into 3D space.
                    // The projection maps 4D coordinates (x, y, z, w) onto 3D space using a stereographic projection.
                    // This works by dividing by (1 - w), which flattens the 4D object into a visible 3D representation.
                    Vector3 v000 = ProjectTo3D(p000);
                    Vector3 v100 = ProjectTo3D(p100);
                    Vector3 v010 = ProjectTo3D(p010);
                    Vector3 v001 = ProjectTo3D(p001);
                    // The result is a set of 3D points that Unity can render with lines or meshes.

                    // Draw lines between the projected 3D points to form the wireframe grid of the hypersphere.
                    // Each line connects the base point (v000) to its neighboring points along the psi, theta, and phi directions.
                    // This gives the appearance of a structured, animated 3D lattice that represents the 4D hypersphere’s projection.
                    RuntimeDraw.DrawLine(v000,v100, lineColor, lineDuration); // connects points along the psi direction
                    RuntimeDraw.DrawLine(v000,v010, lineColor, lineDuration); // connects points along the theta direction
                    RuntimeDraw.DrawLine(v000,v001, lineColor, lineDuration); // connects points along the phi direction
                                                                         // These lines, drawn across all subdivisions, form the full wireframe grid structure of the 4D hypersphere.
                }
            }
        }
    }

    Vector4 S3ToR4(float psi, float theta, float phi)
    {
        // This function converts 3D angular coordinates (psi, theta, phi) into 4D Cartesian coordinates (x, y, z, w).
        // It parameterizes a point on the surface of a 4D hypersphere, also known as S^3 (the 3-sphere).
        // The 3-sphere is the set of all 4D points that are exactly radius units away from the origin (x^2 + y^2 + z^2 + w^2 = radius^2).

        // Each of these formulas defines how the point's coordinates are determined by the angles psi, theta, and phi.
        // These are analogous to how in 3D a sphere uses two angles (latitude and longitude),
        // but here we need three angles because we are describing a surface embedded in 4D space.

        float x = Mathf.Cos(psi);
        // x is the first coordinate in 4D space.
        // When psi = 0, cos(psi) = 1, so x = 1 and the other coordinates become 0.
        // As psi increases, x decreases, distributing more length to the y, z, and w components.
        // This acts like a "latitude" control for how far the point moves from the x-axis into higher dimensions.

        float y = Mathf.Sin(psi) * Mathf.Cos(theta);
        // y depends on both psi and theta.
        // The sin(psi) term ensures that y is 0 when psi = 0 (at the “north pole” of the hypersphere).
        // The cos(theta) term changes the orientation around the y-axis, similar to longitude movement in 3D space.
        // Together they define how the point moves off the x-axis into the y direction.

        float z = Mathf.Sin(psi) * Mathf.Sin(theta) * Mathf.Cos(phi);
        // z depends on all three angles: psi, theta, and phi.
        // sin(psi) and sin(theta) control how much the z component is “activated” as we move away from the main axes.
        // cos(phi) introduces an additional rotation component that moves the point around the z-w plane.
        // This defines the third dimension of the hyperspherical position.

        float w = Mathf.Sin(psi) * Mathf.Sin(theta) * Mathf.Sin(phi);
        // w is the fourth coordinate, directly related to z through the use of sin(phi) instead of cos(phi).
        // This ensures that as phi moves from 0 to 2π, (z, w) traces a full circle in the z-w plane.
        // In essence, w and z work together to define a 2D circular rotation embedded inside the 4D space.

        // Multiply the resulting (x, y, z, w) by the radius to scale the unit hypersphere to the desired size.
        // This ensures all points lie on the surface of a 4D sphere with the given radius.
        return new Vector4(x, y, z, w) * radius;
    }


    Vector4 Rotate4D(Vector4 v, float t)
    {
        // This function rotates a 4D vector 'v' through various possible 2D planes in 4D space.
        // Each rotation acts like a standard 2D rotation but applied to a specific pair of axes (XW, YW, ZW, XY).
        // The parameter 't' represents the rotation angle in radians, usually based on time.
        // These rotations simulate how a 4D object spins, creating dynamic changes when projected into 3D.

        float cosA = Mathf.Cos(t); // Precompute the cosine of the rotation angle to avoid recalculating it multiple times.
        float sinA = Mathf.Sin(t); // Precompute the sine of the rotation angle for the same reason.
                                   // These two values will be reused in all plane rotations for efficiency.

        // ---- XW rotation ----
        // This rotation mixes the x and w components of the vector, creating rotation in the XW plane.
        // It is analogous to a 2D rotation in a plane, but instead of XY, the axes are X and W.
        if (rotateXW)
        {
            float x = v.x * cosA - v.w * sinA; // Standard 2D rotation: x' = x*cos - w*sin.
            float w = v.x * sinA + v.w * cosA; // Standard 2D rotation: w' = x*sin + w*cos.
            v.x = x; v.w = w; // Store the rotated values back into the vector.
                              // This rotation causes parts of the 4D object to move along the w dimension as it spins around the x-axis.
        }

        // ---- YW rotation ----
        // This mixes the y and w components, rotating the object in the YW plane.
        // It introduces a different “axis of rotation” that changes the shape’s projection uniquely.
        if (rotateYW)
        {
            float y = v.y * cosA - v.w * sinA; // Apply the 2D rotation matrix to y and w.
            float w = v.y * sinA + v.w * cosA; // Update w using the same rotation rule.
            v.y = y; v.w = w; // Store the updated results.
                              // This rotation causes distortion primarily in the vertical (y) and depth (w) directions.
        }

        // ---- ZW rotation ----
        // This affects the z and w coordinates.
        // Rotating in this plane alters how the shape expands and contracts in the z-w space.
        if (rotateZW)
        {
            float z = v.z * cosA - v.w * sinA; // Apply standard rotation formula for z and w.
            float w = v.z * sinA + v.w * cosA; // Calculate new w coordinate after rotation.
            v.z = z; v.w = w; // Update z and w in the vector.
                              // This produces a rotation that affects the “depth” of the 4D figure when viewed in 3D.
        }

        // ---- XY rotation ----
        // This one is the most intuitive—it rotates within the normal 3D space, mixing x and y.
        // It is similar to rotating an object flat on a plane in standard 3D geometry.
        if (rotateXY)
        {
            float x = v.x * cosA - v.y * sinA; // Apply 2D rotation formula for x and y.
            float y = v.x * sinA + v.y * cosA; // Compute new y coordinate.
            v.x = x; v.y = y; // Save rotated coordinates back into the vector.
                              // This rotation gives the hypersphere an additional spinning motion visible in 3D.
        }

        return v; // Return the fully rotated 4D vector.
    }


    Vector3 ProjectTo3D(Vector4 p)
    {
        // This function projects a 4D point (x, y, z, w) into 3D space so it can be visualized.
        // It uses a method similar to stereographic projection, which maps a higher-dimensional surface
        // onto a lower-dimensional space (here from 4D → 3D).
        // The 'w' coordinate determines how much the point is stretched or compressed in the projection.

        float denom = 1f - p.w; // Compute denominator for projection.
                                // The projection divides (x, y, z) by (1 - w). When w approaches 1, the denominator approaches zero,
                                // which would cause the projected coordinates to shoot off to infinity (very large values).

        if (Mathf.Abs(denom) < 1e-5f)
            denom = 1e-5f * Mathf.Sign(denom);
        // To avoid division by zero or extremely large values,
        // we clamp 'denom' to a small nonzero number while keeping its sign.
        // This ensures numerical stability when w is very close to 1.

        Vector3 proj = new Vector3(p.x, p.y, p.z) / denom;
        // Perform the actual stereographic projection.
        // Dividing by (1 - w) flattens the 4D coordinates into 3D, maintaining perspective-like depth effects.
        // When w is small, the projection is near-normal, but as w increases, the point moves farther away in 3D space.

        if (proj.magnitude > maxDistanceClamp)
            proj = proj.normalized * maxDistanceClamp;
        // Limit (clamp) the distance of the projected point to avoid infinitely distant positions.
        // This prevents the visualization from exploding due to extreme perspective stretching.

        return transform.TransformPoint(proj * projectionScale);
        // Multiply the projected 3D vector by 'projectionScale' to control its visible size.
        // Then use TransformPoint() to convert the local coordinate into world space,
        // applying the GameObject's position, rotation, and scale.
        // The result is a stable, properly scaled 3D point that can be rendered in Unity’s scene.
    }
}
