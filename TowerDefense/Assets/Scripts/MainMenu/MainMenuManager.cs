using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private string mainScene = "MainScene";

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
