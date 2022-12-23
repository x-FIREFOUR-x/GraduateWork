using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private string mainScene = "MainScene";

    public void Play()
    {
        SceneManager.LoadScene(mainScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
