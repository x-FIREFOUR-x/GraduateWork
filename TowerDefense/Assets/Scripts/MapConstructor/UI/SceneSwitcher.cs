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
    [SerializeField]
    private MessageManager messageManager ;

    public void Play()
    {
        if (!map.IsAllBuilds())
        {
            messageManager.OpenUncorrectMapMessage();
            return;
        }

        int[,] tilesArray = map.GetTileArray();
        Vector2Int indexesStartBuild = map.GetIndexesStartBuild();
        Vector2Int indexesEndBuild = map.GetIndexesEndBuild();

        if (MapSaver.instance.SetData(tilesArray, indexesStartBuild, indexesEndBuild))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainScene);
        }
        else
        {
            messageManager.OpenUncorrectMapMessage();
        }
    }

    public void Back()
    {
        Destroy(MapSaver.instance.gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }
}
