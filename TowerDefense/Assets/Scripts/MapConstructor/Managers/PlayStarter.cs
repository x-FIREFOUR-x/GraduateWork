using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayStarter : MonoBehaviour
{
    [SerializeField]
    private string mainScene = "MainScene";
    [SerializeField]
    private Map map;

    public void Play()
    {
        if (map.IsAllBuilds())
        {
            int[,] tilesArray = map.GetTileArray();
            Vector2Int indexesStartBuild = map.GetIndexesStartBuild();
            Vector2Int indexesEndBuild = map.GetIndexesEndBuild();

            if (MapSaver.instance.SetData(tilesArray, indexesStartBuild, indexesEndBuild))
            {
                SceneManager.LoadScene(mainScene);
            }
        }
    }
}
