using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopper : MonoBehaviour
{
    private Shop activeShop = null;



    public void SetActiveShop(Shop shop) {
        activeShop = shop;
        EventHandler.CallActiveShopChangedEvent();
    }

    public Shop GetActiveShop() {
        return activeShop;
    }





}
