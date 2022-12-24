using UnityEngine;

public class MapSaver : MonoBehaviour
{
    public static MapSaver instance;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
        {
            instance = this;
        }
    }

    public bool ÑonstructedMapIsCorrect()
    {
        return true;
    }
}
