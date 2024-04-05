using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private string defenderGameScene = "DefenderGameScene";
    [SerializeField]
    private string mapConstrucorScene = "MapConstructorScene";

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(defenderGameScene);
    }

    public void Constructor()
    {
        SceneManager.LoadScene(mapConstrucorScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
