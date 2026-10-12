using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Map.Tile;


namespace TowerDefense
{
    public class MapSaver : MonoBehaviour
    {
        public static MapSaver instance;

        private TileKind[,] tilesMatrix;
        private Vector2Int indexesStart;
        private Vector2Int indexesEnd;

        public bool IsSave { get; private set; }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            IsSave = false;

            DontDestroyOnLoad(this.gameObject);
        }

        public bool SetData(TileKind[,] tiles, Vector2Int start, Vector2Int end, out PathGraphBuildError error)
        {
            tilesMatrix = tiles;
            indexesStart = start;
            indexesEnd = end;

            if (ConstructedMapIsCorrect(out error))
            {
                IsSave = true;
                return true;
            }
            else
            {
                tilesMatrix = null;
                indexesStart = new Vector2Int();
                indexesEnd = new Vector2Int();
                return false;
            }

        }

        public HashSet<Vector2Int> GetCells(TileKind kind)
        {
            HashSet<Vector2Int> cells = new();

            for (int i = 0; i < tilesMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < tilesMatrix.GetLength(1); j++)
                {
                    if (tilesMatrix[i, j] == kind)
                        cells.Add(ToGameIndexes(new Vector2Int(i, j)));
                }
            }

            return cells;
        }

        public Vector2Int GetStartCell()
        {
            return ToGameIndexes(indexesStart);
        }

        public Vector2Int GetEndCell()
        {
            return ToGameIndexes(indexesEnd);
        }

        private bool ConstructedMapIsCorrect(out PathGraphBuildError error)
        {
            MapSizeParamsStorage mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

            return PathGraphBuilder.TryBuild(GetCells(TileKind.Path), GetStartCell(), GetEndCell(), mapSizeParams.CellToPosition, out _, out error);
        }

        private static Vector2Int ToGameIndexes(Vector2Int constructorIndexes)
        {
            return new Vector2Int(constructorIndexes.y, constructorIndexes.x);
        }
    }

}
