using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    #region Variables

    [Header("Panels")]
    [SerializeField] private GameObject blockRaycastBackground = null;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject inventoryMenu = null;
    [SerializeField] private GameObject shopMenu = null;

    [Header("Components")]
    [SerializeField] public GameObject blockCanvas;
    [SerializeField] public GameObject inputFieldGameobject;
    [SerializeField] TextMeshProUGUI shopName;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI moneyInventoryText;

    [Header("Scripts")]
    [SerializeField] private UIInventoryBar uiInventoryBar = null;
    [SerializeField] private PauseMenuInventoryManagement pauseMenuInventoryManagement = null;
    [SerializeField] private PauseMenuShopManagement pauseMenuShopManagement = null;

    [HideInInspector] public Shopper shopper = null;
    [HideInInspector] public Shop currentShop = null;
    [HideInInspector] public Purse playerPurse = null;

    public int slotNumber;
    public int quantityInTransaction;
    public bool isBuying;

    #endregion


    #region Properties

    private bool _pauseMenuOn = false;
    public bool PauseMenuOn { get => _pauseMenuOn; set => _pauseMenuOn = value; }

    private bool _inventoryMenuOn = false;
    public bool InventoryMenuOn { get => _inventoryMenuOn; set => _inventoryMenuOn = value; }

    private bool _shopMenuOn = false;
    public bool ShopMenuOn { get => _shopMenuOn; set => _shopMenuOn = value; }

    private bool _inputFieldOn = false;
    public bool InputFieldOn { get => _inputFieldOn; set => _inputFieldOn = value; }

    #endregion


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();

        shopper = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Shopper>();
        playerPurse = shopper.GetComponent<Purse>();

        blockCanvas.SetActive(false);
        pauseMenu.SetActive(false);
        inventoryMenu.SetActive(false);
        shopMenu.SetActive(false);
    }

    private void Start() {
        RefreshMoneyUI();
    }

    private void Update() {
        blockRaycastBackground.SetActive(PauseMenuOn || InventoryMenuOn || ShopMenuOn);

        if(InputFieldOn && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))) {
            ProcessTransactin();
        }

        PauseMenu();
        InventoryMenu();
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.ActiveShopChangedEvent += ShopChanged;
        EventHandler.PurseUpdatedEvent      += RefreshMoneyUI;
    }

    private void OnDisable() {
        EventHandler.ActiveShopChangedEvent -= ShopChanged;
        EventHandler.PurseUpdatedEvent      -= RefreshMoneyUI;
    }

    #endregion


    #region Event Subscriber Methods

    private void ShopChanged() {
        currentShop = shopper.GetActiveShop();
        
        if(currentShop != null) {
            EnableShopMenu();
            EnableInventoryMenu();
            shopName.text = currentShop.GetShopName();
            shopMenu.GetComponentInChildren<PauseMenuShopManagement>().ActiveShopChanged();
        } 
        else {
            DisableShopMenu();
        }

        FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
        EventHandler.CallShopOpenCloseStateChangedEvent(shopper.GetActiveShop());
    }

    private void RefreshMoneyUI() {
        if(InventoryMenuOn) {
            moneyInventoryText.text = $"Total: {playerPurse.GetBalance():N0}";
        }

        moneyText.text = $"{playerPurse.GetBalance():N0}";
    }

    #endregion


    #region Public Methods

    public void DisablePauseMenu() {
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();
        pauseMenuShopManagement.DestroyCurrentlyDraggedItems();

        PauseMenuOn = false;

        if(PauseMenuOn || InventoryMenuOn || ShopMenuOn) {
            Player.Instance.PlayerInputIsDisabled = true;
            Time.timeScale = 0;
        } 
        else {
            Player.Instance.PlayerInputIsDisabled = false;
            Time.timeScale = 1;
        }

        blockRaycastBackground.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        pauseMenu.SetActive(false);
    }

    public void DisableInventoryMenu() {
        // Destroy any currently dragged items
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();
        pauseMenuShopManagement.DestroyCurrentlyDraggedItems();

        InventoryMenuOn = false;

        if(PauseMenuOn || InventoryMenuOn || ShopMenuOn) {
            Player.Instance.PlayerInputIsDisabled = true;
            Time.timeScale = 0;
        } 
        else {
            Player.Instance.PlayerInputIsDisabled = false;
            Time.timeScale = 1;
        }

        inventoryMenu.SetActive(false);
    }

    public void DisableShopMenu() {
        // Destroy any currently dragged items
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();
        pauseMenuShopManagement.DestroyCurrentlyDraggedItems();

        ShopMenuOn = false;

        if(PauseMenuOn || InventoryMenuOn || ShopMenuOn) {
            Player.Instance.PlayerInputIsDisabled = true;
            Time.timeScale = 0;
        } 
        else {
            Player.Instance.PlayerInputIsDisabled = false;
            Time.timeScale = 1;
        }

        shopMenu.SetActive(false);
    }

    public void CloseShopMenu() {
        shopper.SetActiveShop(null);
    }

    public void CloseInputField() {
        blockCanvas.SetActive(false);
        InputFieldOn = false;
    }

    public void ProcessTransactin() {
        if(isBuying) {
            quantityInTransaction = FindObjectOfType<UIShopInputField>().GetEnteredNumber();
            pauseMenuShopManagement.ProcessBuying(slotNumber, quantityInTransaction);
        } else {
            quantityInTransaction = FindObjectOfType<UIShopInputField>().GetEnteredNumber();
            pauseMenuInventoryManagement.ProcessSelling(slotNumber, quantityInTransaction);
        }
        CloseInputField();
    }

    public void QuitGame() {
        Application.Quit();
    }

    #endregion


    #region Private Methods

    private void PauseMenu() {
        // Toggle pause menu if escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (PauseMenuOn) {
                DisablePauseMenu();
            }
            else {
                if(InputFieldOn) {
                    CloseInputField();
                    return;
                }

                if(ShopMenuOn) {
                    CloseShopMenu();
                    return;
                }

                if(InventoryMenuOn) {
                    DisableInventoryMenu();
                    return;
                }

                EnablePauseMenu();
            }
        }
    }

    private void InventoryMenu() {
        if (Input.GetKeyDown(KeyCode.I)) {
            if (InventoryMenuOn) {
                DisableInventoryMenu();
            }
            else {
                DisablePauseMenu();
                EnableInventoryMenu();
            }
        }
    }


    private void EnablePauseMenu() {
        // Destroy any currently dragged items
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        uiInventoryBar.ClearCurrentlySelectedItems();
        
        PauseMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;

        // Stops the update methods for the game objects to be run
        Time.timeScale = 0;

        blockRaycastBackground.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

        // Set the PauseMenuCanvas to active
        pauseMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();
    }

    private void EnableInventoryMenu() {
        // Destroy any currently dragged items
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        uiInventoryBar.ClearCurrentlySelectedItems();
        
        InventoryMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;

        // Stops the update methods for the game objects to be run
        Time.timeScale = 0;

        // Set the PauseMenuCanvas to active
        inventoryMenu.SetActive(true);
        EventHandler.CallPurseUpdatedEvent();

        // Trigger garbage collector
        System.GC.Collect();
    }

    private void EnableShopMenu() {
        // Destroy any currently dragged items
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        uiInventoryBar.ClearCurrentlySelectedItems();
        
        ShopMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;

        // Stops the update methods for the game objects to be run
        Time.timeScale = 0;

        // Set the PauseMenuCanvas to active
        shopMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();
    }

    #endregion

}
