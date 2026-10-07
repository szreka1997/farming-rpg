[System.Serializable]
public class InventoryItem {
    public int itemCode;
    public int itemQuantity;


    public InventoryItem() {
        this.itemCode     = 0;
        this.itemQuantity = 0;
    }

    public InventoryItem(int itemCode, int itemQuantity) {
        this.itemCode     = itemCode;
        this.itemQuantity = itemQuantity;
    }
}
