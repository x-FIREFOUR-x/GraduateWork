using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map
{
    // Scatters obstacles over the cells the path does not use. The path is generated first and never touched,
    // so blocking a cell only takes a tower spot away and can never cut the enemies off from the end building.
    public class BlockedTilesGenerator
    {
        public HashSet<Vector2Int> Generate(int size, List<Vector2Int> generatedPath, float shareOfFreeTiles)
        {
            HashSet<Vector2Int> blockedTiles = new();
            if (shareOfFreeTiles <= 0)
                return blockedTiles;

            HashSet<Vector2Int> pathTiles = new(generatedPath);
            List<Vector2Int> freeTiles = new();

            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    Vector2Int tile = new(row, column);
                    if (!pathTiles.Contains(tile))
                        freeTiles.Add(tile);
                }
            }

            int countBlockedTiles = Mathf.RoundToInt(freeTiles.Count * Mathf.Clamp01(shareOfFreeTiles));
            for (int i = 0; i < countBlockedTiles; i++)
            {
                int drawnIndex = Random.Range(i, freeTiles.Count);
                (freeTiles[i], freeTiles[drawnIndex]) = (freeTiles[drawnIndex], freeTiles[i]);
                blockedTiles.Add(freeTiles[i]);
            }

            return blockedTiles;
        }
    }

}
