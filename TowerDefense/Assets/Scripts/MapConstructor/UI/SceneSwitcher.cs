using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher: MonoBehaviour
{
    [SerializeField]
    private string mainScene = "MainScene";
    [SerializeField]
    private string mainMenuScene = "MainMenuScene";
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
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainScene);
            }
        }
    }

    public void Back()
    {
        Destroy(MapSaver.instance.gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }
}
