using UnityEngine;
using UnityEngine.UI;

public class SelectMenu : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField]
    private Color unactiveColor;
    [SerializeField]
    private Color activeColor;

    [Header("Buttons")]
    [SerializeField]
    private GameObject pathTileButton;
    [SerializeField]
    private GameObject endBuildingButton;
    [SerializeField]
    private GameObject startBuildingButton;

    void Start()
    {
        pathTileButton.GetComponent<Image>().color = unactiveColor;
        endBuildingButton.GetComponent<Image>().color = unactiveColor;
        startBuildingButton.GetComponent<Image>().color = unactiveColor;
    }

    public void ActivePathTileButton()
    {
        pathTileButton.GetComponent<Image>().color = activeColor;

        endBuildingButton.GetComponent<Image>().color = unactiveColor;
        startBuildingButton.GetComponent<Image>().color = unactiveColor;
    }

    public void ActiveStartBuildingButton()
    {
        startBuildingButton.GetComponent<Image>().color = activeColor;

        endBuildingButton.GetComponent<Image>().color = unactiveColor;
        pathTileButton.GetComponent<Image>().color = unactiveColor;
    }

    public void ActiveEndBuildingButton()
    {
        endBuildingButton.GetComponent<Image>().color = activeColor;

        pathTileButton.GetComponent<Image>().color = unactiveColor;
        startBuildingButton.GetComponent<Image>().color = unactiveColor;
    }
}
