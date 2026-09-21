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
        // Tile assets are kept in a "TowerTile", "PathTile" or "BlockedTile" subfolder of these
        private const string texturesFolder = "Assets/Textures/Tile";
        private const string meshesFolder = "Assets/Models/Tile";
        private const string materialsFolder = "Assets/Materials/MapComponents/Tile";
        private const string prefabsFolder = "Assets/Prefabs/MapComponents/Tile";
        private const string storagePath = "Assets/Resources/" + nameof(TileVariantsStorage) + ".asset";
        // Earlier bakes kept one storage per base prefab here
        private const string legacyStoragesFolder = "Assets/Resources/TileVariants";

        private const int grassVariants = 8;
        private const int pathTileVariants = 1;
        private const int blockedVariants = TileDecorBuilder.BlockedVariants;

        // Texture and material names are the tile kind plus one of these
        private static readonly string[] grassTextureVariants = { "Grass_01", "Grass_02", "Grass_03" };
        private const string decorPaletteName = "DecorPalette";

        // Side faces sample the dark groove in the texture corner
        private static readonly Vector2 sideUV = new Vector2(0.02f, 0.02f);

        private enum TileKind { Tower, Path, Blocked }

        private static readonly (string prefabPath, TileKind kind)[] baseTiles =
        {
            ($"{prefabsFolder}/TowerTile/TowerTileBase.prefab", TileKind.Tower),
            ($"{prefabsFolder}/ClickableTowerTile/ClickableTowerTileBase.prefab", TileKind.Tower),
            ($"{prefabsFolder}/PathTile/PathTileBase.prefab", TileKind.Path),
            ($"{prefabsFolder}/BlockedTile/BlockedTileBase.prefab", TileKind.Blocked),
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
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(TileKind.Tower, grassTextureVariants[0])) == null)
                return;

            TileVariantsStorage storage = AssetDatabase.LoadAssetAtPath<TileVariantsStorage>(storagePath);

            foreach (var baseTile in baseTiles)
            {
                bool isBaked = storage != null && baseTile.kind switch
                {
                    TileKind.Path => storage.PathTiles.Count > 0,
                    TileKind.Blocked => storage.BlockedTiles.Count > 0,
                    _ => storage.GetTowerTile(Path.GetFileNameWithoutExtension(baseTile.prefabPath), 0) != null
                };

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

            foreach (TileKind kind in new[] { TileKind.Tower, TileKind.Path, TileKind.Blocked })
            {
                EnsureFolder(TileFolder(meshesFolder, kind));
                EnsureFolder(TileFolder(materialsFolder, kind));
            }

            Texture2D palette = BakePalette();
            // Keyed by mesh path, so kinds that number their variants alike still get a mesh each
            Dictionary<string, Mesh> meshes = new();
            List<TileVariantsStorage.TowerTileVariants> towerTiles = new();
            List<TileVariantsStorage.PathTileVariants> pathTiles = new();
            List<GameObject> blockedTiles = new();

            // Base prefabs live next to their variants and must survive the cleanup
            foreach (var baseTile in baseTiles)
                bakedPaths.Add(baseTile.prefabPath);

            bool allBaked = true;
            foreach (var baseTile in baseTiles)
                allBaked &= BakeBaseTile(baseTile.prefabPath, baseTile.kind, palette, meshes, bakedPaths, towerTiles, pathTiles, blockedTiles);

            // Keep the previous storage and assets when something could not be baked
            if (!allBaked)
            {
                AssetDatabase.SaveAssets();
                Debug.LogError("Tile prefabs bake failed, previous tile assets were kept");
                return;
            }

            SaveStorage(towerTiles, pathTiles, blockedTiles);

            DeleteStaleAssets(new[]
            {
                TileFolder(meshesFolder, TileKind.Tower), TileFolder(meshesFolder, TileKind.Path), TileFolder(meshesFolder, TileKind.Blocked),
                TileFolder(materialsFolder, TileKind.Tower), TileFolder(materialsFolder, TileKind.Path), TileFolder(materialsFolder, TileKind.Blocked),
                $"{prefabsFolder}/TowerTile", $"{prefabsFolder}/ClickableTowerTile", $"{prefabsFolder}/PathTile", $"{prefabsFolder}/BlockedTile"
            }, bakedPaths);
            if (AssetDatabase.IsValidFolder(legacyStoragesFolder))
                AssetDatabase.DeleteAsset(legacyStoragesFolder);

            AssetDatabase.SaveAssets();
            Debug.Log("Tile prefabs baked");
        }


        private static bool BakeBaseTile(string prefabPath, TileKind kind, Texture2D palette, Dictionary<string, Mesh> meshes, HashSet<string> bakedPaths,
                                         List<TileVariantsStorage.TowerTileVariants> towerTiles, List<TileVariantsStorage.PathTileVariants> pathTiles,
                                         List<GameObject> blockedTiles)
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

            if (kind == TileKind.Path)
            {
                for (int bitMaskForPathsConnections = 0; bitMaskForPathsConnections < TilePathConnections.CombinationCount; bitMaskForPathsConnections++)
                {
                    TileVariantsStorage.PathTileVariants variants = new() { BitMaskForPathsConnections = bitMaskForPathsConnections };
                    for (int variant = 0; variant < pathTileVariants; variant++)
                        variants.Prefabs.Add(CreateVariant(bitMaskForPathsConnections, variant));

                    pathTiles.Add(variants);
                }
            }
            else if (kind == TileKind.Blocked)
            {
                for (int variant = 0; variant < blockedVariants; variant++)
                    blockedTiles.Add(CreateVariant(TileShape.NoPath, variant));
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
                // A path tile is told apart by its shape, a tower or blocked tile only by its variant number
                string variantName = kind == TileKind.Path ? ShapeName(bitMaskForPathsConnections) : $"{variant:00}";
                // A path tile has one texture per shape, tower and blocked tiles cycle through the grass textures
                string textureVariant = kind == TileKind.Path ? variantName : grassTextureVariants[variant % grassTextureVariants.Length];

                string texturePath = TexturePath(kind, textureVariant);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                    Debug.LogWarning($"Tile texture not found: {texturePath}");

                string meshPath = $"{TileFolder(meshesFolder, kind)}/{TileName(kind)}_{variantName}.asset";
                if (!meshes.TryGetValue(meshPath, out Mesh mesh))
                {
                    mesh = SaveAsset(BuildTileMesh(bitMaskForPathsConnections, variant, tileSize, kind == TileKind.Blocked), meshPath);
                    meshes[meshPath] = mesh;
                    bakedPaths.Add(meshPath);
                }

                Material tileMaterial = texture != null ? GetMaterial(baseMaterial, texture, textureVariant, kind, bakedPaths) : baseMaterial;
                Material decorMaterial = GetMaterial(baseMaterial, palette, decorPaletteName, kind, bakedPaths);

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

        private static void SaveStorage(List<TileVariantsStorage.TowerTileVariants> towerTiles, List<TileVariantsStorage.PathTileVariants> pathTiles,
                                        List<GameObject> blockedTiles)
        {
            TileVariantsStorage storage = AssetDatabase.LoadAssetAtPath<TileVariantsStorage>(storagePath);
            if (storage == null)
            {
                storage = ScriptableObject.CreateInstance<TileVariantsStorage>();
                storage.SetTiles(towerTiles, pathTiles, blockedTiles);
                AssetDatabase.CreateAsset(storage, storagePath);
            }
            else
            {
                storage.SetTiles(towerTiles, pathTiles, blockedTiles);
                EditorUtility.SetDirty(storage);
            }

            AssetDatabase.SaveAssetIfDirty(storage);
        }

        // Submesh 0 is the relief block with the tile texture, submesh 1 is the decor with the palette.
        // Decor is built in world units and converted to the unit block space of the scaled tile.
        private static Mesh BuildTileMesh(int bitMaskForPathsConnections, int variant, Vector3 tileSize, bool isBlocked)
        {
            Mesh block = TileMeshBuilder.Build(bitMaskForPathsConnections, sideUV);
            Mesh decor = TileDecorBuilder.Build(bitMaskForPathsConnections, variant, tileSize, isBlocked);

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
            string path = $"{texturesFolder}/{decorPaletteName}.png";

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

        private static Material GetMaterial(Material baseMaterial, Texture texture, string variantName, TileKind kind, HashSet<string> bakedPaths)
        {
            string path = $"{TileFolder(materialsFolder, kind)}/{baseMaterial.name}_{variantName}.mat";
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

        private static string TileFolder(string folder, TileKind kind)
        {
            return $"{folder}/{TileName(kind)}";
        }

        // The enum name plus "Tile" names both the subfolder and the assets inside it: TowerTile, PathTile, BlockedTile
        private static string TileName(TileKind kind)
        {
            return $"{kind}Tile";
        }


        private static string TexturePath(TileKind kind, string variantName)
        {
            return $"{TileFolder(texturesFolder, kind)}/{TileName(kind)}_{variantName}.png";
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
