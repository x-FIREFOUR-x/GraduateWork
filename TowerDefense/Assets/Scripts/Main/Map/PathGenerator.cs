using System.Collections.Generic;
using UnityEngine;

public class PathGenerator
{
    public List<Vector2Int> GeneratePath(int size, Vector2Int indexesStart, Vector2Int indexesEnd)
    {
        bool[,] visitedNode = new bool[size, size];
        List<Vector2Int> pathNode = new List<Vector2Int>();

        Vector2Int currentNode = indexesStart;
        visitedNode[currentNode.x, currentNode.y] = true;
        pathNode.Add(currentNode);

        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        while (currentNode != indexesEnd)
        {
            List<Vector2Int> neighboringNode = new List<Vector2Int>();

            for (int i = 0; i < rowNum.Length; i++)
            {
                Vector2Int node = new Vector2Int(currentNode.x + rowNum[i], currentNode.y + columnNum[i]);

                if (visitedNode[node.x, node.y] == false
                    && (node.x > 0 && node.x < size - 1)
                    && (node.y > 0 && node.y < size - 1)
                    && NotMoreOneNeigbor(node, visitedNode))
                {
                    neighboringNode.Add(node);
                }
            }

            if (neighboringNode.Count > 0)
            {
                int randIndex = Random.Range(0, neighboringNode.Count);
                currentNode = neighboringNode[randIndex];

                visitedNode[currentNode.x, currentNode.y] = true;
                pathNode.Add(currentNode);
            }
            else
            {
                if (pathNode.Count != 1)
                {
                    pathNode.RemoveAt(pathNode.Count - 1);
                    currentNode = pathNode[pathNode.Count - 1];
                }
                else
                {
                    visitedNode = new bool[size, size];
                    currentNode = indexesStart;
                    visitedNode[currentNode.x, currentNode.y] = true;
                }
            }

        }

        return pathNode;
    }

    private bool NotMoreOneNeigbor(Vector2Int newNode, bool[,] visitedNode)
    {
        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        int countNeigbors = 0;

        for (int i = 0; i < rowNum.Length; i++)
        {
            Vector2Int node = new Vector2Int(newNode.x + rowNum[i], newNode.y + columnNum[i]);

            if (visitedNode[node.x, node.y] == true)
            {
                countNeigbors++;
            }
        }

        return countNeigbors == 1;
    }

    public List<Vector2Int> GetPathWithMatrix(int[,] matrix, Vector2Int indexesStart, Vector2Int indexesEnd)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        Vector2Int prevIndexes = indexesStart;
        Vector2Int currentIndexes = indexesStart;
        path.Add(new Vector2Int(currentIndexes.y, currentIndexes.x));

        while (currentIndexes != indexesEnd)
        {
            for (int i = 0; i < rowNum.Length; i++)
            {
                Vector2Int newIndexes = new Vector2Int(currentIndexes.x + rowNum[i], currentIndexes.y + columnNum[i]);

                if (matrix[newIndexes.x, newIndexes.y] == 1 && newIndexes != prevIndexes)
                {
                    prevIndexes = currentIndexes;
                    currentIndexes = newIndexes;
                    path.Add(new Vector2Int(currentIndexes.y, currentIndexes.x));
                    break;
                }
            }
        }

        return path;
    }
    
}
