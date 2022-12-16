using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopComponent : MonoBehaviour
{
    private Color colorText;

    [Header("Turret")]
    [SerializeField]
    private GameObject buttonBuy;
    [SerializeField]
    private TMPro.TextMeshProUGUI textName;
    [SerializeField]
    private TMPro.TextMeshProUGUI textPrice;

    
    public void Initialize(Color color, int price)
    {
        colorText = color;

        textName.color = colorText;
        textPrice.color = colorText;

        textPrice.text = price.ToString() + "$";
    }
    
}
