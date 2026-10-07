using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryTextbox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshTop1 = null;
    [SerializeField] private TextMeshProUGUI textMeshTop2 = null;
    [SerializeField] private TextMeshProUGUI textMeshTop3 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom1 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom2 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom3 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom4 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom5 = null;

    [SerializeField] private Image moneyImage1 = null;
    [SerializeField] private Image moneyImage2 = null;



    /// <summary>
    /// Set text values 
	/// </summary>
    public void SetTextboxText(string textTop1, string textTop2, string textTop3, 
                               string textBottom1, string textBottom2, string textBottom3, string textBottom4, string textBottom5) {

        textMeshTop1.text    = textTop1;
        textMeshTop2.text    = textTop2;
        textMeshTop3.text    = textTop3;

        textMeshBottom1.text = textBottom1;
        textMeshBottom2.text = textBottom2;
        textMeshBottom3.text = textBottom3;
        textMeshBottom4.text = textBottom4;
        textMeshBottom5.text = textBottom5;
    }

    public void SetMoney1Image(Sprite sprite) {
        moneyImage1.sprite = sprite;
    }

    public void SetMoney2Image(Sprite sprite) {
        moneyImage2.sprite = sprite;
    }

    public void SetPriceTextColor(Color color) {
        textMeshBottom3.color = color;
    }


    
}
