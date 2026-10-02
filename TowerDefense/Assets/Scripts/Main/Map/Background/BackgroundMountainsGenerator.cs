using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using Random = System.Random;


namespace TowerDefense.Main.Map.Background
{
    public static class BackgroundMountainsGenerator
    {
        private const int paletteWidth = 32;
        private const int paletteHeight = 64;

        // A spire's jagged silhouette never pushes a vertex further than this multiple of its nominal
        // base radius out from its own centre (see CliffMeshBuilder.Build) - used here to work out how
        // much clearance a spire actually needs from the grid, instead of guessing a blanket distance
        private const float maxSpireBulge = 1.35f;

        // CliffMeshBuilder also leans the whole spire sideways by up to ~0.3 * radius per axis (see
        // its `lean` vector); this is the worst-case combined horizontal drift that adds on top of the
        // bulge, so a tall leaning spire's peak cannot end up hanging out over the grid either
        private const float maxSpireLean = 0.43f;

        private static float SpireClearance(float spireRadius)
        {
            return spireRadius * (maxSpireBulge + maxSpireLean) + 2f;
        }

        public static GameObject Generate(Transform parent, Vector3 center, float gridHalfWidth, float ringRadius, int count, int seed,
                                           Material material, float scale = 1f)
        {
            Random rng = new Random(seed);

            GameObject container = new GameObject("BackgroundMountains");
            container.transform.SetParent(parent, false);

            float arcLength = ringRadius * (Mathf.PI * 2 / count);

            float densityFreq = (float)Range(rng, 2, 4);
            float densityPhase = (float)Range(rng, 0, Mathf.PI * 2);

            for (int i = 0; i < count; i++)
            {
                float baseAngle = i * Mathf.PI * 2 / count;
                float density = 0.5f + 0.5f * Mathf.Sin(baseAngle * densityFreq + densityPhase);
                if (density < 0.32f)
                    continue;

                float angle = (float)(i + Range(rng, -0.25, 0.25)) * Mathf.PI * 2 / count;
                float radius = ringRadius * (float)Range(rng, 0.8, 1.05);
                Vector3 radial = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 clusterCenter = center + radial * radius;

                GameObject cluster = new GameObject($"MountainCluster_{i}");
                cluster.transform.SetParent(container.transform, false);

                float mainHeight = (float)Range(rng, 17.4, 28) * scale;
                float mainRadius = (float)Range(rng, 12, 18) * scale;
                Vector3 mainPosition = ClearOfGrid(clusterCenter, center, gridHalfWidth, SpireClearance(mainRadius));
                AddSpire(cluster.transform, material, rng, mainPosition, mainHeight, mainRadius);

                int satellites = rng.Next(3, 6);
                for (int s = 0; s < satellites; s++)
                {
                    float side = s % 2 == 0 ? 1f : -1f;
                    float tangentOffset = side * (float)Range(rng, arcLength * 0.35, arcLength * 0.8);
                    float radialJitter = (float)Range(rng, -4, 6);
                    Vector3 satellitePoint = clusterCenter + tangent * tangentOffset + radial * radialJitter;

                    float falloff = Mathf.Clamp01(1f - Mathf.Abs(tangentOffset) / (arcLength * 1.1f));
                    float height = Mathf.Lerp(8.6f, 18.6f, falloff) * (float)Range(rng, 0.85, 1.15) * scale;
                    float baseRadius = (float)Range(rng, 6.9, 12) * scale;

                    Vector3 satellitePosition = ClearOfGrid(satellitePoint, center, gridHalfWidth, SpireClearance(baseRadius));
                    AddSpire(cluster.transform, material, rng, satellitePosition, height, baseRadius);
                }
            }

            return container;
        }

        private static Vector3 ClearOfGrid(Vector3 point, Vector3 gridCenter, float gridHalfWidth, float clearance)
        {
            float dx = point.x - gridCenter.x;
            float dz = point.z - gridCenter.z;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            if (distance < 1e-4f)
                return point;

            float cos = dx / distance;
            float sin = dz / distance;
            float boundaryDistance = gridHalfWidth / Mathf.Max(Mathf.Abs(cos), Mathf.Abs(sin));
            float requiredDistance = boundaryDistance + clearance;

            if (distance >= requiredDistance)
                return point;

            return gridCenter + new Vector3(cos, 0, sin) * requiredDistance;
        }

        private static void AddSpire(Transform parent, Material material, Random rng, Vector3 position, float height, float baseRadius)
        {
            GameObject spire = new GameObject("Spire");
            spire.transform.SetParent(parent, false);
            spire.transform.position = position;

            Mesh mesh = new CliffMeshBuilder(rng).Build(height, baseRadius);

            MeshFilter meshFilter = spire.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = spire.AddComponent<MeshRenderer>();
            if (material != null)
                meshRenderer.sharedMaterial = material;

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static double Range(Random rng, double min, double max)
        {
            return min + rng.NextDouble() * (max - min);
        }


        private class CliffMeshBuilder
        {
            private readonly Random rng;
            private readonly List<Vector3> vertices = new();
            private readonly List<Vector3> normals = new();
            private readonly List<Vector2> uvs = new();
            private readonly List<int> triangles = new();

            public CliffMeshBuilder(Random rng)
            {
                this.rng = rng;
            }

            public Mesh Build(float height, float radius)
            {
                int sides = rng.Next(32, 38);
                int levels = rng.Next(5, 7);
                float tone = Range(0.25f, 0.75f);
                float turn = Range(0, Mathf.PI * 2);

                Vector3 lean = new Vector3(Range(-0.1f, 0.1f), 0, Range(-0.1f, 0.1f)) * radius;

                float ridgeFreq1 = Range(2f, 3f);
                float ridgePhase1 = Range(0, Mathf.PI * 2);
                float ridgeAmp1 = Range(0.09f, 0.15f);
                float ridgeFreq2 = Range(4f, 6f);
                float ridgePhase2 = Range(0, Mathf.PI * 2);
                float ridgeAmp2 = Range(0.045f, 0.07f);

                float footFreq = Range(1f, 2f);
                float footPhase = Range(0, Mathf.PI * 2);
                float footAmp = Range(0.14f, 0.2f);

                float zoneFreqAngle = Range(1.5f, 3f);
                float zoneFreqHeight = Range(1f, 2f);
                float zonePhase = Range(0, Mathf.PI * 2);
                float zoneAmp = Range(0.12f, 0.2f);

                float[] ringRadius = new float[levels + 1];
                float[] ringTop = new float[levels + 1];
                float rises = 0;

                ringRadius[0] = radius;
                for (int level = 1; level <= levels; level++)
                {
                    float ledge = Range(0.08f, 0.22f);
                    ringRadius[level] = Mathf.Clamp(ringRadius[level - 1] * (1 - ledge), radius * 0.1f, radius * maxSpireBulge);

                    ringTop[level] = ringTop[level - 1] + Range(1.2f, 3.2f);
                    rises = ringTop[level];
                }

                Vector3[] points = new Vector3[(levels + 1) * sides];
                float[] pointTones = new float[(levels + 1) * sides];
                float[] pointElevations = new float[(levels + 1) * sides];
                Vector3[] ringCenters = new Vector3[levels + 1];
                for (int level = 0; level <= levels; level++)
                {
                    float t = ringTop[level] / rises;
                    Vector3 center = Vector3.up * (height * t) + lean * t * t;

                    for (int i = 0; i < sides; i++)
                    {
                        float angle = turn + i * Mathf.PI * 2 / sides;
                        Vector3 direction = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                        float ridge = 1
                            + ridgeAmp1 * Mathf.Sin(angle * ridgeFreq1 + ridgePhase1)
                            + ridgeAmp2 * Mathf.Sin(angle * ridgeFreq2 + ridgePhase2)
                            + footAmp * (1f - t) * Mathf.Sin(angle * footFreq + footPhase);
                        float jaggedness = Range(0.96f, 1.04f);
                        float verticalJitter = Range(-height * 0.008f, height * 0.008f);

                        float vertexRadius = Mathf.Min(ringRadius[level] * ridge * jaggedness, radius * maxSpireBulge);

                        Vector3 ridgedCenter = center + Vector3.up * verticalJitter;
                        points[level * sides + i] = ridgedCenter + direction * vertexRadius;

                        float zone = zoneAmp * Mathf.Sin(angle * zoneFreqAngle + t * zoneFreqHeight * Mathf.PI * 2 + zonePhase);
                        pointTones[level * sides + i] = Mathf.Clamp01(tone + zone + Range(-0.03f, 0.03f));
                        pointElevations[level * sides + i] = Mathf.Clamp01(t + Range(-0.04f, 0.04f));
                    }
                }

                for (int level = 0; level <= levels; level++)
                {
                    Vector3 sum = Vector3.zero;
                    for (int i = 0; i < sides; i++)
                        sum += points[level * sides + i];

                    ringCenters[level] = sum / sides;
                }

                for (int level = 0; level < levels; level++)
                {
                    Vector3 lower = ringCenters[level];
                    Vector3 upper = ringCenters[level + 1];

                    for (int i = 0; i < sides; i++)
                    {
                        int next = (i + 1) % sides;
                        Vector3 a = points[level * sides + i];
                        Vector3 b = points[level * sides + next];
                        Vector3 c = points[(level + 1) * sides + next];
                        Vector3 d = points[(level + 1) * sides + i];

                        Vector3 normalAd = JitterNormal(BandNormal(a, d, lower, upper));
                        Vector3 normalBc = JitterNormal(BandNormal(b, c, lower, upper));

                        Vector2 uvA = UV(pointTones[level * sides + i], pointElevations[level * sides + i]);
                        Vector2 uvB = UV(pointTones[level * sides + next], pointElevations[level * sides + next]);
                        Vector2 uvC = UV(pointTones[(level + 1) * sides + next], pointElevations[(level + 1) * sides + next]);
                        Vector2 uvD = UV(pointTones[(level + 1) * sides + i], pointElevations[(level + 1) * sides + i]);

                        AddSmoothQuad(a, b, c, d, normalAd, normalBc, uvA, uvB, uvC, uvD);
                    }
                }

                int top = levels * sides;
                Vector3 topCenter = ringCenters[levels];
                float peakRadius = Mathf.Max((points[top] - topCenter).magnitude, 0.01f);

                float lastHeightGain = Mathf.Max(ringTop[levels] - ringTop[levels - 1], 0.5f);
                float lastRadiusLoss = Mathf.Max(ringRadius[levels - 1] - ringRadius[levels], radius * 0.05f);
                float localSlope = lastHeightGain / lastRadiusLoss;
                float peakHeight = Mathf.Clamp(peakRadius * localSlope * Range(0.85f, 1.15f), peakRadius, peakRadius * 3.5f);
                Vector3 peakOffset = new Vector3(Range(-0.65f, 0.65f), 0, Range(-0.65f, 0.65f)) * peakRadius;
                Vector3 peak = topCenter + peakOffset + Vector3.up * peakHeight;
                Vector2 peakUv = UV(Mathf.Clamp01(tone + 0.15f), 1f);

                for (int i = 0; i < sides; i++)
                {
                    int next = (i + 1) % sides;
                    Vector3 rimA = points[top + i];
                    Vector3 rimB = points[top + next];

                    Vector3 outwardA = Outward(rimA, topCenter);
                    Vector3 outwardB = Outward(rimB, topCenter);
                    Vector3 slopeA = new Vector3(outwardA.x * peakHeight, peakRadius, outwardA.z * peakHeight).normalized;
                    Vector3 slopeB = new Vector3(outwardB.x * peakHeight, peakRadius, outwardB.z * peakHeight).normalized;

                    AddSmoothTriangle(rimA, peak, rimB, slopeA, Vector3.up, slopeB, peakUv);
                }

                Mesh mesh = new Mesh();
                if (vertices.Count > 65000)
                    mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();

                return mesh;
            }

            private void AddSmoothQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normalAd, Vector3 normalBc,
                                        Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
            {
                int start = vertices.Count;
                AddVertex(a, normalAd, uvA);
                AddVertex(b, normalBc, uvB);
                AddVertex(c, normalBc, uvC);
                AddVertex(d, normalAd, uvD);

                triangles.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            }

            private void AddSmoothTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 na, Vector3 nb, Vector3 nc, Vector2 uv)
            {
                int start = vertices.Count;
                AddVertex(a, na, uv);
                AddVertex(b, nb, uv);
                AddVertex(c, nc, uv);
                triangles.AddRange(new[] { start, start + 1, start + 2 });
            }

            private static Vector3 Outward(Vector3 point, Vector3 center)
            {
                Vector3 direction = new(point.x - center.x, 0, point.z - center.z);
                return direction.sqrMagnitude > 1e-8f ? direction.normalized : Vector3.forward;
            }

            private static Vector3 BandNormal(Vector3 bottom, Vector3 top, Vector3 bottomCenter, Vector3 topCenter)
            {
                Vector3 outward = Outward(bottom, bottomCenter);
                float bottomReach = new Vector2(bottom.x - bottomCenter.x, bottom.z - bottomCenter.z).magnitude;
                float topReach = new Vector2(top.x - topCenter.x, top.z - topCenter.z).magnitude;
                float rise = Mathf.Max(top.y - bottom.y, 0.001f);

                return new Vector3(outward.x * rise, bottomReach - topReach, outward.z * rise).normalized;
            }

            private Vector3 JitterNormal(Vector3 normal)
            {
                const float strength = 0.07f;
                Vector3 jitter = new Vector3(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f)) * strength;
                return (normal + jitter).normalized;
            }

            private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                vertices.Add(position);
                normals.Add(normal);
                uvs.Add(uv);
            }

            private float Range(float min, float max)
            {
                return min + (float)rng.NextDouble() * (max - min);
            }

            private static Vector2 UV(float tone, float elevation)
            {
                float u = Mathf.Lerp(0.5f / paletteWidth, 1 - 0.5f / paletteWidth, Mathf.Clamp01(tone));
                float v = Mathf.Lerp(0.5f / paletteHeight, 1 - 0.5f / paletteHeight, Mathf.Clamp01(elevation));
                return new Vector2(u, v);
            }
        }
    }
}
