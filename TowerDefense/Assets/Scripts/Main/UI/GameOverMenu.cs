using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField]
    private string mainMenuScene = "MainMenuScene";

    [SerializeField]
    private GameObject endBuildingPrefab;
    [SerializeField]
    private GameObject gameOverMenuUI;

    void Update()
    {
        if (endBuildingPrefab.GetComponent<EndBuilding>().health <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Time.timeScale = 0f;

        gameOverMenuUI.SetActive(true);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        Destroy(MapSaver.instance.gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }
}
