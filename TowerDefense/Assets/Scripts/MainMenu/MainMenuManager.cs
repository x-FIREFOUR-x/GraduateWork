using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private string mainScene = "MainScene";
    [SerializeField]
    private string mapConstrucorScene = "MapConstructorScene";

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainScene);
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
