using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

using TowerDefense.Main.Map.Tile;
using TowerDefense.Storage;


namespace TowerDefense.EditorTools.Tile
{
    // Bakes 3D tiles into regular assets: one model per tile variant (relief block + grass, stones, flowers
    // as two submeshes), materials, the decor palette texture, prefab variants of the game base tile prefabs
    // and one TileVariantsStorage that the game uses to pick them.
    // Files left in the output folders from earlier bakes are deleted.
    [InitializeOnLoad]
    public static class TilePrefabsBaker
    {
        // Tower and path tile assets are kept in "TowerTile" and "PathTile" subfolders of these
        private const string texturesFolder = "Assets/Textures/Tile";
        private const string meshesFolder = "Assets/Models/Tile";
        private const string materialsFolder = "Assets/Materials/MapComponents/Tile";
        private const string prefabsFolder = "Assets/Prefabs/MapComponents/Tile";
        private const string storagePath = "Assets/Resources/" + nameof(TileVariantsStorage) + ".asset";
        // Earlier bakes kept one storage per base prefab here
        private const string legacyStoragesFolder = "Assets/Resources/TileVariants";

        private const int grassVariants = 8;
        private const int pathTileVariants = 1;

        private static readonly string[] grassTextureNames = { "Tile_Grass_01", "Tile_Grass_02", "Tile_Grass_03" };

        // Side faces sample the dark groove in the texture corner
        private static readonly Vector2 sideUV = new Vector2(0.02f, 0.02f);

        private static readonly (string prefabPath, bool isPath)[] baseTiles =
        {
            ($"{prefabsFolder}/TowerTile/TowerTileBase.prefab", false),
            ($"{prefabsFolder}/ClickableTowerTile/ClickableTowerTileBase.prefab", false),
            ($"{prefabsFolder}/PathTile/PathTileBase.prefab", true),
        };


        static TilePrefabsBaker()
        {
            EditorApplication.delayCall += BakeIfMissing;
        }

        // Also rebakes when some variants are missing (an unfinished earlier bake)
        private static void BakeIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // Wait until the tile textures are imported
            if (AssetDatabase.LoadAssetAtPath<Texture2D>($"{TileFolder(texturesFolder, false)}/{grassTextureNames[0]}.png") == null)
                return;

            TileVariantsStorage storage = AssetDatabase.LoadAssetAtPath<TileVariantsStorage>(storagePath);

            foreach (var baseTile in baseTiles)
            {
                bool isBaked = storage != null && (baseTile.isPath
                    ? storage.PathTiles.Count > 0
                    : storage.GetTowerTile(Path.GetFileNameWithoutExtension(baseTile.prefabPath), 0) != null);

                if (!isBaked)
                {
                    Bake();
                    return;
                }
            }
        }

        [MenuItem("Tools/Tile/Bake Tile Prefabs")]
        public static void Bake()
        {
            HashSet<string> bakedPaths = new();

            foreach (bool isPath in new[] { false, true })
            {
                EnsureFolder(TileFolder(meshesFolder, isPath));
                EnsureFolder(TileFolder(materialsFolder, isPath));
            }

            Texture2D palette = BakePalette();
            Dictionary<string, Mesh> meshes = new();
            List<TileVariantsStorage.TowerTileVariants> towerTiles = new();
            List<TileVariantsStorage.PathTileVariants> pathTiles = new();

            bool allBaked = true;
            foreach (var baseTile in baseTiles)
                allBaked &= BakeBaseTile(baseTile.prefabPath, baseTile.isPath, palette, meshes, bakedPaths, towerTiles, pathTiles);

            // Keep the previous storage and assets when something could not be baked
            if (!allBaked)
            {
                AssetDatabase.SaveAssets();
                Debug.LogError("Tile prefabs bake failed, previous tile assets were kept");
                return;
            }

            SaveStorage(towerTiles, pathTiles);

            DeleteStaleAssets(new[]
            {
                TileFolder(meshesFolder, false), TileFolder(meshesFolder, true),
                TileFolder(materialsFolder, false), TileFolder(materialsFolder, true),
                $"{prefabsFolder}/TowerTile", $"{prefabsFolder}/ClickableTowerTile", $"{prefabsFolder}/PathTile"
            }, bakedPaths);
            if (AssetDatabase.IsValidFolder(legacyStoragesFolder))
                AssetDatabase.DeleteAsset(legacyStoragesFolder);

            AssetDatabase.SaveAssets();
            Debug.Log("Tile prefabs baked");
        }


        private static bool BakeBaseTile(string prefabPath, bool isPath, Texture2D palette, Dictionary<string, Mesh> meshes, HashSet<string> bakedPaths,
                                         List<TileVariantsStorage.TowerTileVariants> towerTiles, List<TileVariantsStorage.PathTileVariants> pathTiles)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (basePrefab == null)
            {
                Debug.LogError($"Base tile prefab not found: {prefabPath}");
                return false;
            }

            Material baseMaterial = basePrefab.GetComponent<Renderer>().sharedMaterial;
            if (baseMaterial == null)
            {
                Debug.LogError($"Base tile prefab has no material: {prefabPath}");
                return false;
            }

            Vector3 tileSize = basePrefab.transform.localScale;
            string tileName = basePrefab.name.Replace("Base", string.Empty);
            string variantsFolder = $"{prefabsFolder}/{tileName}";
            EnsureFolder(variantsFolder);

            if (isPath)
            {
                for (int bitMaskForPathsConnections = 0; bitMaskForPathsConnections < TilePathConnections.CombinationCount; bitMaskForPathsConnections++)
                {
                    TileVariantsStorage.PathTileVariants variants = new() { BitMaskForPathsConnections = bitMaskForPathsConnections };
                    for (int variant = 0; variant < pathTileVariants; variant++)
                        variants.Prefabs.Add(CreateVariant(bitMaskForPathsConnections, variant));

                    pathTiles.Add(variants);
                }
            }
            else
            {
                TileVariantsStorage.TowerTileVariants towerTileVariants = new() { BasePrefabName = basePrefab.name };
                for (int variant = 0; variant < grassVariants; variant++)
                    towerTileVariants.Prefabs.Add(CreateVariant(TileShape.NoPath, variant));

                towerTiles.Add(towerTileVariants);
            }

            return true;

            GameObject CreateVariant(int bitMaskForPathsConnections, int variant)
            {
                string textureName = bitMaskForPathsConnections == TileShape.NoPath
                    ? grassTextureNames[variant % grassTextureNames.Length]
                    : $"Tile_Path_{ShapeName(bitMaskForPathsConnections)}";

                string texturePath = $"{TileFolder(texturesFolder, isPath)}/{textureName}.png";
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                    Debug.LogWarning($"Tile texture not found: {texturePath}");

                string variantName = $"{ShapeName(bitMaskForPathsConnections)}_{variant:00}";
                if (!meshes.TryGetValue(variantName, out Mesh mesh))
                {
                    string meshPath = $"{TileFolder(meshesFolder, isPath)}/Tile_{variantName}.asset";
                    mesh = SaveAsset(BuildTileMesh(bitMaskForPathsConnections, variant, tileSize), meshPath);
                    meshes[variantName] = mesh;
                    bakedPaths.Add(meshPath);
                }

                Material tileMaterial = texture != null ? GetMaterial(baseMaterial, texture, isPath, bakedPaths) : baseMaterial;
                Material decorMaterial = GetMaterial(baseMaterial, palette, isPath, bakedPaths);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                try
                {
                    instance.GetComponent<MeshFilter>().sharedMesh = mesh;
                    instance.GetComponent<Renderer>().sharedMaterials = new[] { tileMaterial, decorMaterial };

                    string path = $"{variantsFolder}/{tileName}_{variantName}.prefab";
                    bakedPaths.Add(path);
                    return PrefabUtility.SaveAsPrefabAsset(instance, path);
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void SaveStorage(List<TileVariantsStorage.TowerTileVariants> towerTiles, List<TileVariantsStorage.PathTileVariants> pathTiles)
        {
            TileVariantsStorage storage = AssetDatabase.LoadAssetAtPath<TileVariantsStorage>(storagePath);
            if (storage == null)
            {
                storage = ScriptableObject.CreateInstance<TileVariantsStorage>();
                storage.SetTiles(towerTiles, pathTiles);
                AssetDatabase.CreateAsset(storage, storagePath);
            }
            else
            {
                storage.SetTiles(towerTiles, pathTiles);
                EditorUtility.SetDirty(storage);
            }

            AssetDatabase.SaveAssetIfDirty(storage);
        }

        // Submesh 0 is the relief block with the tile texture, submesh 1 is the decor with the palette.
        // Decor is built in world units and converted to the unit block space of the scaled tile.
        private static Mesh BuildTileMesh(int bitMaskForPathsConnections, int variant, Vector3 tileSize)
        {
            Mesh block = TileMeshBuilder.Build(bitMaskForPathsConnections, sideUV);
            Mesh decor = TileDecorBuilder.Build(bitMaskForPathsConnections, variant, tileSize);

            List<Vector3> vertices = new(block.vertices);
            List<Vector3> normals = new(block.normals);
            List<Vector2> uvs = new(block.uv);
            int decorStart = vertices.Count;

            Vector3[] decorVertices = decor.vertices;
            Vector3[] decorNormals = decor.normals;
            for (int i = 0; i < decorVertices.Length; i++)
            {
                Vector3 v = decorVertices[i];
                Vector3 n = decorNormals[i];
                vertices.Add(new Vector3(v.x / tileSize.x, v.y / tileSize.y + 0.5f, v.z / tileSize.z));
                normals.Add(new Vector3(n.x * tileSize.x, n.y * tileSize.y, n.z * tileSize.z).normalized);
            }
            uvs.AddRange(decor.uv);

            int[] decorTriangles = decor.triangles;
            for (int i = 0; i < decorTriangles.Length; i++)
                decorTriangles[i] += decorStart;

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(block.triangles, 0);
            mesh.SetTriangles(decorTriangles, 1);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            Object.DestroyImmediate(block);
            Object.DestroyImmediate(decor);

            return mesh;
        }

        private static Texture2D BakePalette()
        {
            string path = $"{texturesFolder}/TileDecorPalette.png";

            Texture2D texture = TileDecorBuilder.CreatePalette();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material GetMaterial(Material baseMaterial, Texture texture, bool isPath, HashSet<string> bakedPaths)
        {
            string path = $"{TileFolder(materialsFolder, isPath)}/{baseMaterial.name}_{texture.name}.mat";
            if (bakedPaths.Contains(path))
                return AssetDatabase.LoadAssetAtPath<Material>(path);

            Material material = new Material(baseMaterial)
            {
                color = Color.white,
                mainTexture = texture,
                enableInstancing = true
            };
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", 0);

            bakedPaths.Add(path);
            return SaveAsset(material, path);
        }

        // Creates the asset or overwrites the existing one in place, so references to it stay valid
        private static T SaveAsset<T>(T asset, string path) where T : Object
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                return asset;
            }

            string name = existing.name;
            EditorUtility.CopySerialized(asset, existing);
            existing.name = name;
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(asset);

            return existing;
        }

        // Only files lying directly in the bake output folders are touched, listed from disk
        private static void DeleteStaleAssets(string[] folders, HashSet<string> bakedPaths)
        {
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                    continue;

                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    if (file.EndsWith(".meta"))
                        continue;

                    string path = $"{folder}/{Path.GetFileName(file)}";
                    if (!bakedPaths.Contains(path))
                        AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static string ShapeName(int bitMaskForPathsConnections)
        {
            const int N = TilePathConnections.North, E = TilePathConnections.East, S = TilePathConnections.South, W = TilePathConnections.West;

            return bitMaskForPathsConnections switch
            {
                TileShape.NoPath => "Grass",
                0 => "Single",
                N => "End_N",
                E => "End_E",
                S => "End_S",
                W => "End_W",
                N | S => "Straight_NS",
                E | W => "Straight_EW",
                N | E => "Corner_NE",
                E | S => "Corner_ES",
                S | W => "Corner_SW",
                W | N => "Corner_WN",
                N | E | S => "T_NES",
                E | S | W => "T_ESW",
                S | W | N => "T_SWN",
                W | N | E => "T_WNE",
                _ => "Cross"
            };
        }

        private static string TileFolder(string folder, bool isPath)
        {
            return $"{folder}/{(isPath ? "PathTile" : "TowerTile")}";
        }

        private static void EnsureFolder(string folder)
        {
            string parent = "Assets";
            foreach (string part in folder.Substring("Assets/".Length).Split('/'))
            {
                string current = $"{parent}/{part}";
                if (!AssetDatabase.IsValidFolder(current))
                    AssetDatabase.CreateFolder(parent, part);

                parent = current;
            }
        }
    }

}
