using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using Random = System.Random;


namespace TowerDefense.Main.Map.Background
{
    public static class BackgroundGroundGenerator
    {
        private const int paletteWidth = 32;
        private const int paletteHeight = 64;

        private const float targetCellSize = 6f;

        // Radial distance (from the tile grid's edge) over which the foothill rises from flat ground
        // up to its full height, so the slope reads as gradually climbing toward the mountains - varied
        // per region by sharpnessNoise below so some stretches climb steeply and others gently
        private const float minRiseDistance = 22f;
        private const float maxRiseDistance = 95f;

        // The noisy texture (wiggle/fine/micro) ramps up over this much shorter span than the macro
        // climb above, so bumps and dips are already fully present right next to the tile grid
        private const float noiseRiseDistance = 8f;

        // Only this close to the tile grid is the ground actually seen in-camera, so relief density,
        // amplitude and color contrast are all concentrated into this band instead of spread evenly
        // across the whole (mostly off-screen) terrain
        private const float denseZoneDistance = 35f;

        // Color diversity is pushed hardest inside this first stretch (about a quarter of the ground)
        private const float colorZoneDistance = 20f;

        // groundY is the tile tops; under the grid the ground sits tileSeamDrop lower so it never
        // z-fights with the tiles' top faces, and open ground outside the grid starts outerStartRise above
        private const float tileSeamDrop = 0.2f;
        private const float outerStartRise = 0.05f;

        // Multiplier applied only to the downward half of the noise, so dips are shallower than bumps are tall
        private const float dipDepthScale = 0.5f;

        private const float baseAmplitude = 6f;
        private const float wiggleAmplitude = 3.5f;
        private const float fineAmplitude = 1.4f;
        private const float microAmplitude = 0.6f;

        public static GameObject Generate(Transform parent, Vector3 center, float gridHalfWidth, float groundY,
                                           float coverageRadius, Material material, int seed, out System.Func<float, float, float> heightAt)
        {
            Random rng = new Random(seed);

            GameObject terrain = new GameObject("BackgroundGround");
            terrain.transform.SetParent(parent, false);

            Mesh mesh = BuildMesh(rng, center, gridHalfWidth, groundY, coverageRadius, out heightAt);
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
            if (material != null)
                meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            return terrain;
        }

        private static Mesh BuildMesh(Random rng, Vector3 center, float gridHalfWidth, float groundY, float coverageRadius,
                                       out System.Func<float, float, float> heightAt)
        {
            float offsetX = (float)Range(rng, 0, 10000);
            float offsetZ = (float)Range(rng, 0, 10000);
            float broadFreq = (float)Range(rng, 0.012, 0.02);
            float detailFreq = (float)Range(rng, 0.05, 0.08);
            float microFreq = (float)Range(rng, 0.15, 0.25);

            float colorOffsetX = (float)Range(rng, 0, 10000);
            float colorOffsetZ = (float)Range(rng, 0, 10000);
            float colorFreq = (float)Range(rng, 0.02, 0.035);

            float colorOffsetX2 = (float)Range(rng, 0, 10000);
            float colorOffsetZ2 = (float)Range(rng, 0, 10000);
            float colorFreq2 = (float)Range(rng, 0.1, 0.16);

            // Drives a full, un-averaged sweep across the whole palette width - used as-is (not blended
            // down with other noise) so the ground's first stretch reads as a vivid, gradient-like band
            // of color instead of an averaged, muted middle tone
            float paletteOffsetX = (float)Range(rng, 0, 10000);
            float paletteOffsetZ = (float)Range(rng, 0, 10000);
            float paletteFreq = (float)Range(rng, 0.07, 0.1);

            float sweepOffsetX2 = (float)Range(rng, 0, 10000);
            float sweepOffsetZ2 = (float)Range(rng, 0, 10000);
            float sweepFreq2 = (float)Range(rng, 0.16, 0.24);

            float shadeOffsetX = (float)Range(rng, 0, 10000);
            float shadeOffsetZ = (float)Range(rng, 0, 10000);
            float shadeFreq = (float)Range(rng, 0.09, 0.14);

            float sharpOffsetX = (float)Range(rng, 0, 10000);
            float sharpOffsetZ = (float)Range(rng, 0, 10000);
            float sharpFreq = (float)Range(rng, 0.008, 0.015);

            // Where this noise peaks gets the bumpy wiggle/fine/micro relief - the threshold is relaxed
            // near the tile grid (see denseZoneDistance) so that visible band is densely covered with
            // several distinct patches, while the sparse default far away barely matters since it's off-screen
            float featureOffsetX = (float)Range(rng, 0, 10000);
            float featureOffsetZ = (float)Range(rng, 0, 10000);
            float featureFreq = (float)Range(rng, 0.05, 0.08);

            const float totalNoiseAmplitude = wiggleAmplitude + fineAmplitude + microAmplitude;
            const float maxHeight = baseAmplitude + totalNoiseAmplitude;
            const float elevationSpan = maxHeight + totalNoiseAmplitude;

            // Cell size is snapped so the tile grid's edge falls exactly on a mesh grid line - otherwise
            // quads straddling the edge would interpolate the rising outside terrain back over the tiles
            float cellSize = gridHalfWidth / Mathf.Max(1, Mathf.RoundToInt(gridHalfWidth / targetCellSize));

            int half = Mathf.CeilToInt(coverageRadius / cellSize);
            int edgeIndex = Mathf.Max(1, Mathf.RoundToInt(gridHalfWidth / cellSize));

            // The two grid lines on the tile grid's edge are each doubled: the inner copy stays below
            // the tiles, the outer copy starts the open ground - a zero-width step instead of a slope
            // that would climb from under the tiles to above them
            List<float> offsets = new();
            List<bool> outerCopy = new();
            for (int k = -half; k <= half; k++)
            {
                if (k == -edgeIndex)
                {
                    offsets.Add(k * cellSize);
                    outerCopy.Add(true);
                    offsets.Add(k * cellSize);
                    outerCopy.Add(false);
                }
                else if (k == edgeIndex)
                {
                    offsets.Add(k * cellSize);
                    outerCopy.Add(false);
                    offsets.Add(k * cellSize);
                    outerCopy.Add(true);
                }
                else
                {
                    offsets.Add(k * cellSize);
                    outerCopy.Add(false);
                }
            }
            int sideCount = offsets.Count;

            float EdgeDistanceAt(float x, float z)
            {
                float dx = Mathf.Max(Mathf.Abs(x - center.x) - gridHalfWidth, 0f);
                float dz = Mathf.Max(Mathf.Abs(z - center.z) - gridHalfWidth, 0f);
                return Mathf.Sqrt(dx * dx + dz * dz);
            }

            float ReliefMaskAt(float x, float z, float edgeDistance)
            {
                float denseT = Mathf.Clamp01(edgeDistance / denseZoneDistance);
                float threshold = Mathf.Lerp(0.1f, 0.78f, denseT);

                float featureNoise = Mathf.PerlinNoise((x + featureOffsetX) * featureFreq, (z + featureOffsetZ) * featureFreq);
                float mask = Mathf.Clamp01((featureNoise - threshold) / 0.1f);
                return mask * mask * (3f - 2f * mask);
            }

            float Height(float x, float z, bool outerEdge)
            {
                float edgeDistance = EdgeDistanceAt(x, z);

                float sharpness = Mathf.PerlinNoise((x + sharpOffsetX) * sharpFreq, (z + sharpOffsetZ) * sharpFreq);
                float localRiseDistance = Mathf.Lerp(minRiseDistance, maxRiseDistance, sharpness);

                float t = Mathf.Clamp01(edgeDistance / localRiseDistance);
                float falloff = t * t * (3f - 2f * t);

                float wiggle = Mathf.PerlinNoise((x + offsetX) * broadFreq, (z + offsetZ) * broadFreq) - 0.5f;
                float fine = Mathf.PerlinNoise((x + offsetX) * detailFreq, (z + offsetZ) * detailFreq) - 0.5f;
                float micro = Mathf.PerlinNoise((x + offsetX) * microFreq, (z + offsetZ) * microFreq) - 0.5f;

                float noiseT = Mathf.Clamp01(edgeDistance / noiseRiseDistance);
                float noiseFalloff = noiseT * noiseT * (3f - 2f * noiseT);

                float denseT = Mathf.Clamp01(edgeDistance / denseZoneDistance);
                float nearBoost = Mathf.Lerp(3.2f, 1f, denseT);

                float noise = (wiggle * 2f * wiggleAmplitude + fine * 2f * fineAmplitude + micro * 2f * microAmplitude)
                              * noiseFalloff * ReliefMaskAt(x, z, edgeDistance) * nearBoost;

                if (noise < 0f)
                    noise *= dipDepthScale;

                float lift = falloff * baseAmplitude + noise;

                bool outside = edgeDistance > 0f || outerEdge;
                lift += outside ? outerStartRise : -tileSeamDrop;

                return groundY + lift;
            }

            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();

            const float normalSample = 0.5f;

            for (int row = 0; row < sideCount; row++)
            {
                float z = center.z + offsets[row];

                for (int col = 0; col < sideCount; col++)
                {
                    float x = center.x + offsets[col];
                    bool outer = outerCopy[row] || outerCopy[col];
                    float y = Height(x, z, outer);

                    float heightDx = Height(x + normalSample, z, outer) - Height(x - normalSample, z, outer);
                    float heightDz = Height(x, z + normalSample, outer) - Height(x, z - normalSample, outer);
                    Vector3 normal = EdgeDistanceAt(x, z) > 0f || outer
                        ? new Vector3(-heightDx, normalSample * 2f, -heightDz).normalized
                        : Vector3.up;

                    float elevation = Mathf.Clamp01((y - groundY + totalNoiseAmplitude) / elevationSpan);
                    float broadTone = Mathf.PerlinNoise((x + colorOffsetX) * colorFreq, (z + colorOffsetZ) * colorFreq);
                    float patchTone = Mathf.PerlinNoise((x + colorOffsetX2) * colorFreq2, (z + colorOffsetZ2) * colorFreq2);
                    float farTone = Mathf.Clamp01(broadTone * 0.6f + patchTone * 0.4f);

                    float edgeDistance = EdgeDistanceAt(x, z);

                    float sweepA = Mathf.PerlinNoise((x + paletteOffsetX) * paletteFreq, (z + paletteOffsetZ) * paletteFreq);
                    float sweepB = Mathf.PerlinNoise((x + sweepOffsetX2) * sweepFreq2, (z + sweepOffsetZ2) * sweepFreq2);
                    float gradientT = Mathf.Clamp01(edgeDistance / colorZoneDistance);
                    float stretched = Mathf.Clamp01((sweepA * 0.7f + sweepB * 0.3f - 0.5f) * 2.2f + 0.5f);
                    float nearTone = Mathf.PingPong(stretched * 1.4f + gradientT * 0.9f + elevation * 0.4f, 1f);

                    float shade = Mathf.PerlinNoise((x + shadeOffsetX) * shadeFreq, (z + shadeOffsetZ) * shadeFreq);
                    float nearShade = Mathf.Lerp(shade, elevation, gradientT);

                    float denseT = Mathf.Clamp01(edgeDistance / denseZoneDistance);
                    float tone = Mathf.Lerp(nearTone, farTone, denseT);
                    float shadeV = Mathf.Lerp(nearShade, elevation, denseT);
                    Vector2 uv = UV(tone, shadeV);

                    vertices.Add(new Vector3(x, y, z));
                    normals.Add(normal);
                    uvs.Add(uv);
                }
            }

            for (int row = 0; row < sideCount - 1; row++)
            {
                for (int col = 0; col < sideCount - 1; col++)
                {
                    int a = row * sideCount + col;
                    int b = row * sideCount + col + 1;
                    int c = (row + 1) * sideCount + col + 1;
                    int d = (row + 1) * sideCount + col;

                    AddUpwardTriangle(triangles, vertices, a, b, c);
                    AddUpwardTriangle(triangles, vertices, a, c, d);
                }
            }

            Mesh mesh = new Mesh();
            if (vertices.Count > 65000)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);

            heightAt = (x, z) => Height(x, z, true);

            return mesh;
        }

        private static void AddUpwardTriangle(List<int> triangles, List<Vector3> vertices, int ia, int ib, int ic)
        {
            Vector3 faceNormal = Vector3.Cross(vertices[ib] - vertices[ia], vertices[ic] - vertices[ia]);

            triangles.Add(ia);
            if (faceNormal.y < 0f)
            {
                triangles.Add(ic);
                triangles.Add(ib);
            }
            else
            {
                triangles.Add(ib);
                triangles.Add(ic);
            }
        }

        private static double Range(Random rng, double min, double max)
        {
            return min + rng.NextDouble() * (max - min);
        }

        private static Vector2 UV(float tone, float elevation)
        {
            float u = Mathf.Lerp(0.5f / paletteWidth, 1 - 0.5f / paletteWidth, Mathf.Clamp01(tone));
            float v = Mathf.Lerp(0.5f / paletteHeight, 1 - 0.5f / paletteHeight, Mathf.Clamp01(elevation));
            return new Vector2(u, v);
        }
    }
}
