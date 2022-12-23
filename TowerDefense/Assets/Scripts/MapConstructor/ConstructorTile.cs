using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructorTile : MonoBehaviour
{
    private Renderer render;

    [Header("Attributes")]

    [SerializeField]
    private Color hoverColor;
    private Color unhoverColor;


    void Awake()
    {
        render = GetComponent<Renderer>();
        unhoverColor = render.material.color;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) &&
            render.material.color == hoverColor)
        {
            LeftMouseClick();
        }

        if (Input.GetMouseButtonDown(1) &&
            render.material.color == hoverColor)
        {
            RightMouseClick();
        }
    }

    private void RightMouseClick()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().BuildConstructorTile(this.gameObject);
    }

    private void LeftMouseClick()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().BuildPathTile(this.gameObject);
    }

    private void OnMouseEnter()
    {
        render.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        render.material.color = unhoverColor;
    }
}
