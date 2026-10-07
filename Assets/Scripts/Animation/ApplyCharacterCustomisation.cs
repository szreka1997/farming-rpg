using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class colorSwap
{
    public Color fromColor;
    public Color toColor;

    public colorSwap(Color fromColor, Color toColor) {
        this.fromColor = fromColor;
        this.toColor = toColor;
    }
}


public class ApplyCharacterCustomisation : MonoBehaviour
{
    #region Variables

    // Input Textures
    [Header("Base Textures")]
    [SerializeField] private Texture2D maleFarmerBaseTexture = null;
    [SerializeField] private Texture2D femaleFarmerBaseTexture = null;
    [SerializeField] private Texture2D shirtsBaseTexture = null; 
    [SerializeField] private Texture2D hairBaseTexture = null;  
    [SerializeField] private Texture2D hatsBaseTexture = null;   
    [SerializeField] private Texture2D adornmentsBaseTexture = null;
    private Texture2D farmerBaseTexture;                               // If female then = femaleFarmerBaseTexture else = makeFarmerBaseTexture

    // Created Textures
    [Header("OutputBase Texture To Be Used For Animation")]
    [SerializeField] private Texture2D farmerBaseCustomised = null;    // Animations will use this texture, final output texture
    [SerializeField] private Texture2D hairCustomised = null;
    [SerializeField] private Texture2D hatsCustomised = null;
    private Texture2D farmerBaseShirtsUpdated;                         // Shirts positioned perfectly based on direction and animation (Shirt only)
    private Texture2D farmerBaseAdornmentsUpdated;
    private Texture2D selectedShirt;                                   // Set of red shirts or set of green shirts
    private Texture2D selectedAdornment;

    // Select Sex
    [Header("Select Sex: 0 = Male, 1 = Female")]
    [Range(0, 1)]
    [SerializeField] private int inputSex = 0;

    // Select Shirt Style
    [Header("Select Shirt Style")]
    [Range(0, 1)]
    [SerializeField] private int inputShirtStyleNo = 0;

    [Header("Select Trouser Color")]
    [SerializeField] private Color inputTrouserColor = Color.blue;

    // Select Hair Style
    [Header("Select Hair Style")]
    [Range(0, 2)]
    [SerializeField] private int inputHairStyleNo = 0;
    [SerializeField] private Color inputHairColor = Color.black;

    // Select Skin Type
    [Header("Select Skin Type")]
    [Range(0, 3)]
    [SerializeField] private int inputSkinType = 0;

    // Select Hat Style
    [Header("Select Hat Style")]
    [Range(0, 1)]
    [SerializeField] private int inputHatStyleNo = 0;

    // Select Adornments Style
    [Header("Select Adornments Style")]
    [Range(0, 2)]
    [SerializeField] private int inputAdornmentsStyleNo = 0;

    private Facing[,] bodyFacingMatrix;               // Directions that the player's body is facing in
    private Vector2Int[,] bodyShirtOffsetMatrix;
    private Vector2Int[,] bodyAdornmentsOffsetArray;

    // Dimensions
    private int bodyRows = 21;
    private int bodyColumns = 6;
    private int farmerSpriteWidth = 16;
    private int farmerSpriteHeight = 32;

    private int shirtTextureWidth = 9;
    private int shirtTextureHeight = 36;
    private int shirtSpriteWidth = 9;
    private int shirtSpriteHeight = 9;
    private int shirtStylesInSpriteWidth = 16;

    private int hairTextureWidth = 16;
    private int hairTextureHeight = 96;
    private int hairStylesInSpriteWidth = 8;

    private int hatTextureWidth = 20;
    private int hatTextureHeight = 80;
    private int hatStylesInSpriteWidth = 12;

    private int adornmentsTextureWidth = 16;
    private int adornmentsTextureHeight = 32;
    private int adornmentsSpriteWidth = 16;
    private int adornmentsSpriteHeight = 16;
    private int adornmentsStylesInSpriteWidth = 8;

    private List<colorSwap> colorSwapList;

    // Target arm colours for color replacement
    private Color32 armTargetColor1 = new Color32(77, 13, 13, 255);  // darkest
    private Color32 armTargetColor2 = new Color32(138, 41, 41, 255); // next darkest
    private Color32 armTargetColor3 = new Color32(172, 50, 50, 255); // lightest

    // Target skin colours for color replacement
    private Color32 skinTargetColor1 = new Color32(145, 117, 90, 255);   // darkest
    private Color32 skinTargetColor2 = new Color32(204, 155, 108, 255);  // next darkest
    private Color32 skinTargetColor3 = new Color32(207, 166, 128, 255);  // next darkest
    private Color32 skinTargetColor4 = new Color32(238, 195, 154, 255);  // lightest

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        // Initialise color swap list
        colorSwapList = new List<colorSwap>();

        // Process Customisation
        ProcessCustomisation();
    }

    #endregion



    private void ProcessCustomisation() {
        ProcessGender();
        ProcessShirt();
        ProcessArms();
        ProcessTrousers();
        ProcessHair();
        ProcessSkin();
        ProcessHat();
        ProcessAdornments();
        MergeCustomisations();
    }



    #region Process Customisation

    private void ProcessGender() {
        // Set base spritesheet by gender
        if (inputSex == 0) {
            farmerBaseTexture = maleFarmerBaseTexture;
        } 
        else if (inputSex == 1) {
            farmerBaseTexture = femaleFarmerBaseTexture;
        }

        // Get base pixels
        Color[] farmerBasePixels = farmerBaseTexture.GetPixels();      // Gets all the pixels from a particulat texture

        // Set changed base pixels
        farmerBaseCustomised.SetPixels(farmerBasePixels);              // farmerBaseCustomized - target output
        farmerBaseCustomised.Apply();                                  // Apply the pixels to the texture
        // Basically we set farmerBaseCustomized to be eaqual farmerBaseTexture
    }

    private void ProcessShirt() {
        bodyFacingMatrix = new Facing[bodyColumns, bodyRows];
        PopulateBodyFacingMatrix();

        bodyShirtOffsetMatrix = new Vector2Int[bodyColumns, bodyRows];
        PopulateBodyShirtOffsetMatrix();

        // Create Selected Shirt Texture
        AddShirtToTexture(inputShirtStyleNo);

        // Apply shirt texture to base
        ApplyShirtTextureToBase();
    }

    private void ProcessArms() {
        // Get arm pixels to recolor
        Color[] farmerPixelsToRecolour = farmerBaseTexture.GetPixels(0, 0, 288, farmerBaseTexture.height);

        // Populate Arm Color Swap List
        PopulateArmColorSwapList();

        // Change arm colours
        ChangePixelColors(farmerPixelsToRecolour, colorSwapList);

        // Set recoloured pixels
        farmerBaseCustomised.SetPixels(0, 0, 288, farmerBaseTexture.height, farmerPixelsToRecolour);

        // Apply texture changes
        farmerBaseCustomised.Apply();
    }

    private void ProcessTrousers() {
        // Get trouser pixels to recolor
        Color[] farmerTrouserPixels = farmerBaseTexture.GetPixels(288, 0, 96, farmerBaseTexture.height);

        // Change trouser colours
        TintPixelColors(farmerTrouserPixels, inputTrouserColor);

        // Set changed trouser pixels
        farmerBaseCustomised.SetPixels(288, 0, 96, farmerBaseTexture.height, farmerTrouserPixels);

        // Apply texture changes
        farmerBaseCustomised.Apply();

    }

    private void ProcessHair() {
        // Create Selected Hair Texture
        AddHairToTexture(inputHairStyleNo);

        // Get hair pixels to recolor
        Color[] farmerSelectedHairPixels = hairCustomised.GetPixels();

        // Tint hair pixels
        TintPixelColors(farmerSelectedHairPixels, inputHairColor);

        hairCustomised.SetPixels(farmerSelectedHairPixels);
        hairCustomised.Apply();
    }

    private void ProcessSkin() {
        // Get skin pixels to recolor
        Color[] farmerPixelsToRecolour = farmerBaseCustomised.GetPixels(0, 0, 288, farmerBaseTexture.height);

        // Populate Skin Color Swap List
        PopulateSkinColorSwapList(inputSkinType);

        // Change skin colours
        ChangePixelColors(farmerPixelsToRecolour, colorSwapList);

        // Set recoloured pixels
        farmerBaseCustomised.SetPixels(0, 0, 288, farmerBaseTexture.height, farmerPixelsToRecolour);

        // Apply texture changes
        farmerBaseCustomised.Apply();
    }

    private void ProcessHat() {
        // Create Selected Hat Texture
        AddHatToTexture(inputHatStyleNo);
    }

    private void ProcessAdornments() {
        bodyAdornmentsOffsetArray = new Vector2Int[bodyColumns, bodyRows];
        PopulateBodyAdornmentsOffsetArray();

        // Create Selected Adornments Texture
        AddAdornmentsToTexture(inputAdornmentsStyleNo);

        //Create new adornments base texture
        farmerBaseAdornmentsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        farmerBaseAdornmentsUpdated.filterMode = FilterMode.Point;

        // Set adornments base texture to transparent
        SetTextureToTransparent(farmerBaseAdornmentsUpdated);
        ApplyAdornmentsTextureToBase();
    }

    private void MergeCustomisations() {
        // Farmer Shirt Pixels
        Color[] farmerShirtPixels = farmerBaseShirtsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        // Farmer Trouser Pixels
        Color[] farmerTrouserPixelsSelection = farmerBaseCustomised.GetPixels(288, 0, 96, farmerBaseTexture.height);

        // Farmer Body Pixels
        Color[] farmerBodyPixels = farmerBaseCustomised.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        // Farmer Adornments Pixels
        Color[] farmerAdornmentsPixels = farmerBaseAdornmentsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        MergeColourArray(farmerBodyPixels, farmerTrouserPixelsSelection);
        MergeColourArray(farmerBodyPixels, farmerShirtPixels);
        MergeColourArray(farmerBodyPixels, farmerAdornmentsPixels);

        // Paste merged  pixels
        farmerBaseCustomised.SetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height, farmerBodyPixels);

        // Apply texture changes
        farmerBaseCustomised.Apply();
    }

    #endregion


    #region Shirt Customisation

    private void PopulateBodyFacingMatrix() {
        bodyFacingMatrix[0, 0] = Facing.None;        // [0, 0] is the bottom left sprite of the sprite sheet
        bodyFacingMatrix[1, 0] = Facing.None;
        bodyFacingMatrix[2, 0] = Facing.None;
        bodyFacingMatrix[3, 0] = Facing.None;
        bodyFacingMatrix[4, 0] = Facing.None;
        bodyFacingMatrix[5, 0] = Facing.None;

        bodyFacingMatrix[0, 1] = Facing.None;
        bodyFacingMatrix[1, 1] = Facing.None;
        bodyFacingMatrix[2, 1] = Facing.None;
        bodyFacingMatrix[3, 1] = Facing.None;
        bodyFacingMatrix[4, 1] = Facing.None;
        bodyFacingMatrix[5, 1] = Facing.None;

        bodyFacingMatrix[0, 2] = Facing.None;
        bodyFacingMatrix[1, 2] = Facing.None;
        bodyFacingMatrix[2, 2] = Facing.None;
        bodyFacingMatrix[3, 2] = Facing.None;
        bodyFacingMatrix[4, 2] = Facing.None;
        bodyFacingMatrix[5, 2] = Facing.None;

        bodyFacingMatrix[0, 3] = Facing.None;
        bodyFacingMatrix[1, 3] = Facing.None;
        bodyFacingMatrix[2, 3] = Facing.None;
        bodyFacingMatrix[3, 3] = Facing.None;
        bodyFacingMatrix[4, 3] = Facing.None;
        bodyFacingMatrix[5, 3] = Facing.None;

        bodyFacingMatrix[0, 4] = Facing.None;
        bodyFacingMatrix[1, 4] = Facing.None;
        bodyFacingMatrix[2, 4] = Facing.None;
        bodyFacingMatrix[3, 4] = Facing.None;
        bodyFacingMatrix[4, 4] = Facing.None;
        bodyFacingMatrix[5, 4] = Facing.None;

        bodyFacingMatrix[0, 5] = Facing.None;
        bodyFacingMatrix[1, 5] = Facing.None;
        bodyFacingMatrix[2, 5] = Facing.None;
        bodyFacingMatrix[3, 5] = Facing.None;
        bodyFacingMatrix[4, 5] = Facing.None;
        bodyFacingMatrix[5, 5] = Facing.None;

        bodyFacingMatrix[0, 6] = Facing.None;
        bodyFacingMatrix[1, 6] = Facing.None;
        bodyFacingMatrix[2, 6] = Facing.None;
        bodyFacingMatrix[3, 6] = Facing.None;
        bodyFacingMatrix[4, 6] = Facing.None;
        bodyFacingMatrix[5, 6] = Facing.None;

        bodyFacingMatrix[0, 7] = Facing.None;
        bodyFacingMatrix[1, 7] = Facing.None;
        bodyFacingMatrix[2, 7] = Facing.None;
        bodyFacingMatrix[3, 7] = Facing.None;
        bodyFacingMatrix[4, 7] = Facing.None;
        bodyFacingMatrix[5, 7] = Facing.None;

        bodyFacingMatrix[0, 8] = Facing.None;
        bodyFacingMatrix[1, 8] = Facing.None;
        bodyFacingMatrix[2, 8] = Facing.None;
        bodyFacingMatrix[3, 8] = Facing.None;
        bodyFacingMatrix[4, 8] = Facing.None;
        bodyFacingMatrix[5, 8] = Facing.None;

        bodyFacingMatrix[0, 9] = Facing.None;
        bodyFacingMatrix[1, 9] = Facing.None;
        bodyFacingMatrix[2, 9] = Facing.None;
        bodyFacingMatrix[3, 9] = Facing.None;
        bodyFacingMatrix[4, 9] = Facing.None;
        bodyFacingMatrix[5, 9] = Facing.None;

        bodyFacingMatrix[0, 10] = Facing.Back;
        bodyFacingMatrix[1, 10] = Facing.Back;
        bodyFacingMatrix[2, 10] = Facing.Right;
        bodyFacingMatrix[3, 10] = Facing.Right;
        bodyFacingMatrix[4, 10] = Facing.Right;
        bodyFacingMatrix[5, 10] = Facing.Right;

        bodyFacingMatrix[0, 11] = Facing.Front;
        bodyFacingMatrix[1, 11] = Facing.Front;
        bodyFacingMatrix[2, 11] = Facing.Front;
        bodyFacingMatrix[3, 11] = Facing.Front;
        bodyFacingMatrix[4, 11] = Facing.Back;
        bodyFacingMatrix[5, 11] = Facing.Back;

        bodyFacingMatrix[0, 12] = Facing.Back;
        bodyFacingMatrix[1, 12] = Facing.Back;
        bodyFacingMatrix[2, 12] = Facing.Right;
        bodyFacingMatrix[3, 12] = Facing.Right;
        bodyFacingMatrix[4, 12] = Facing.Right;
        bodyFacingMatrix[5, 12] = Facing.Right;

        bodyFacingMatrix[0, 13] = Facing.Front;
        bodyFacingMatrix[1, 13] = Facing.Front;
        bodyFacingMatrix[2, 13] = Facing.Front;
        bodyFacingMatrix[3, 13] = Facing.Front;
        bodyFacingMatrix[4, 13] = Facing.Back;
        bodyFacingMatrix[5, 13] = Facing.Back;

        bodyFacingMatrix[0, 14] = Facing.Back;
        bodyFacingMatrix[1, 14] = Facing.Back;
        bodyFacingMatrix[2, 14] = Facing.Right;
        bodyFacingMatrix[3, 14] = Facing.Right;
        bodyFacingMatrix[4, 14] = Facing.Right;
        bodyFacingMatrix[5, 14] = Facing.Right;

        bodyFacingMatrix[0, 15] = Facing.Front;
        bodyFacingMatrix[1, 15] = Facing.Front;
        bodyFacingMatrix[2, 15] = Facing.Front;
        bodyFacingMatrix[3, 15] = Facing.Front;
        bodyFacingMatrix[4, 15] = Facing.Back;
        bodyFacingMatrix[5, 15] = Facing.Back;

        bodyFacingMatrix[0, 16] = Facing.Back;
        bodyFacingMatrix[1, 16] = Facing.Back;
        bodyFacingMatrix[2, 16] = Facing.Right;
        bodyFacingMatrix[3, 16] = Facing.Right;
        bodyFacingMatrix[4, 16] = Facing.Right;
        bodyFacingMatrix[5, 16] = Facing.Right;

        bodyFacingMatrix[0, 17] = Facing.Front;
        bodyFacingMatrix[1, 17] = Facing.Front;
        bodyFacingMatrix[2, 17] = Facing.Front;
        bodyFacingMatrix[3, 17] = Facing.Front;
        bodyFacingMatrix[4, 17] = Facing.Back;
        bodyFacingMatrix[5, 17] = Facing.Back;

        bodyFacingMatrix[0, 18] = Facing.Back;
        bodyFacingMatrix[1, 18] = Facing.Back;
        bodyFacingMatrix[2, 18] = Facing.Back;
        bodyFacingMatrix[3, 18] = Facing.Right;
        bodyFacingMatrix[4, 18] = Facing.Right;
        bodyFacingMatrix[5, 18] = Facing.Right;

        bodyFacingMatrix[0, 19] = Facing.Right;
        bodyFacingMatrix[1, 19] = Facing.Right;
        bodyFacingMatrix[2, 19] = Facing.Right;
        bodyFacingMatrix[3, 19] = Facing.Front;
        bodyFacingMatrix[4, 19] = Facing.Front;
        bodyFacingMatrix[5, 19] = Facing.Front;

        bodyFacingMatrix[0, 20] = Facing.Front;
        bodyFacingMatrix[1, 20] = Facing.Front;
        bodyFacingMatrix[2, 20] = Facing.Front;
        bodyFacingMatrix[3, 20] = Facing.Back;
        bodyFacingMatrix[4, 20] = Facing.Back;
        bodyFacingMatrix[5, 20] = Facing.Back;
    }

    private void PopulateBodyShirtOffsetMatrix() {
        bodyShirtOffsetMatrix[0, 0] = new Vector2Int(99, 99);   // (0, 0) is the bottom left corner of the sprite
        bodyShirtOffsetMatrix[1, 0] = new Vector2Int(99, 99);   // (4, 9) 4 to the right, 9 to the  the up
        bodyShirtOffsetMatrix[2, 0] = new Vector2Int(99, 99);   // That's where we're gonna place the shirt sprite
        bodyShirtOffsetMatrix[3, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 0] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 1] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 2] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 3] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 4] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 5] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 6] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 7] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 8] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[1, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[2, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[3, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[4, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetMatrix[5, 9] = new Vector2Int(99, 99);

        bodyShirtOffsetMatrix[0, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetMatrix[1, 10] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[2, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetMatrix[3, 10] = new Vector2Int(4, 12);
        bodyShirtOffsetMatrix[4, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetMatrix[5, 10] = new Vector2Int(4, 10);

        bodyShirtOffsetMatrix[0, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetMatrix[1, 11] = new Vector2Int(4, 12);
        bodyShirtOffsetMatrix[2, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetMatrix[3, 11] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[4, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetMatrix[5, 11] = new Vector2Int(4, 12);

        bodyShirtOffsetMatrix[0, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetMatrix[1, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetMatrix[2, 12] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[3, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[4, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[5, 12] = new Vector2Int(4, 9);

        bodyShirtOffsetMatrix[0, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[1, 13] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[2, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetMatrix[3, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetMatrix[4, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[5, 13] = new Vector2Int(4, 9);

        bodyShirtOffsetMatrix[0, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[1, 14] = new Vector2Int(4, 12);
        bodyShirtOffsetMatrix[2, 14] = new Vector2Int(4, 7);
        bodyShirtOffsetMatrix[3, 14] = new Vector2Int(4, 5);
        bodyShirtOffsetMatrix[4, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[5, 14] = new Vector2Int(4, 12);

        bodyShirtOffsetMatrix[0, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetMatrix[1, 15] = new Vector2Int(4, 5);
        bodyShirtOffsetMatrix[2, 15] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[3, 15] = new Vector2Int(4, 12);
        bodyShirtOffsetMatrix[4, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetMatrix[5, 15] = new Vector2Int(4, 5);

        bodyShirtOffsetMatrix[0, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[1, 16] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[2, 16] = new Vector2Int(4, 7);
        bodyShirtOffsetMatrix[3, 16] = new Vector2Int(4, 8);
        bodyShirtOffsetMatrix[4, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[5, 16] = new Vector2Int(4, 10);

        bodyShirtOffsetMatrix[0, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetMatrix[1, 17] = new Vector2Int(4, 8);
        bodyShirtOffsetMatrix[2, 17] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[3, 17] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[4, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetMatrix[5, 17] = new Vector2Int(4, 8);

        bodyShirtOffsetMatrix[0, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[1, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[2, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[3, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[4, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[5, 18] = new Vector2Int(4, 9);

        bodyShirtOffsetMatrix[0, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[1, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[2, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[3, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[4, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[5, 19] = new Vector2Int(4, 9);

        bodyShirtOffsetMatrix[0, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[1, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[2, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[3, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetMatrix[4, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetMatrix[5, 20] = new Vector2Int(4, 9);
    }

    private void AddShirtToTexture(int shirtStyleNo) {
        // Create shirt texture
        selectedShirt = new Texture2D(shirtTextureWidth, shirtTextureHeight);
        selectedShirt.filterMode = FilterMode.Point;    // Point ~ Pixel Perfect

        // Calculate coordinates for shirt pixels
        int y = (shirtStyleNo / shirtStylesInSpriteWidth) * shirtTextureHeight;
        int x = (shirtStyleNo % shirtStylesInSpriteWidth) * shirtTextureWidth;

        // Get shirts pixels
        Color[] shirtPixels = shirtsBaseTexture.GetPixels(x, y, shirtTextureWidth, shirtTextureHeight);

        // Apply selected shirt pixels to texture
        selectedShirt.SetPixels(shirtPixels);
        selectedShirt.Apply();
    }

    private void ApplyShirtTextureToBase() {
        // Create new shirt base texture
        farmerBaseShirtsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        farmerBaseShirtsUpdated.filterMode = FilterMode.Point;

        // Set shirt base texture to transparent
        SetTextureToTransparent(farmerBaseShirtsUpdated);

        Color[] frontShirtPixels;
        Color[] backShirtPixels;
        Color[] rightShirtPixels;

        frontShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 3, shirtSpriteWidth, shirtSpriteHeight);
        backShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 0, shirtSpriteWidth, shirtSpriteHeight);
        rightShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 2, shirtSpriteWidth, shirtSpriteHeight);

        // Loop through base texture and apply shirt pixels

        for (int x = 0; x < bodyColumns; x++) {
            for (int y = 0; y < bodyRows; y++) {
                int pixelX = x * farmerSpriteWidth;
                int pixelY = y * farmerSpriteHeight;

                if (bodyShirtOffsetMatrix[x, y] != null) {
                    if (bodyShirtOffsetMatrix[x, y].x == 99 && bodyShirtOffsetMatrix[x, y].y == 99) { // do not populate with shirt
                        continue;     // continue to the next iteration of the loop
                    }

                    pixelX += bodyShirtOffsetMatrix[x, y].x;
                    pixelY += bodyShirtOffsetMatrix[x, y].y;
                }

                // Switch on facing direction
                switch (bodyFacingMatrix[x, y]) {
                    case Facing.Front:
                        // Populate front shirt pixels
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, frontShirtPixels);
                        break;

                    case Facing.Back:
                        // Populate back shirt pixels
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, backShirtPixels);
                        break;

                    case Facing.Right:
                        // Populate right shirt pixels
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, rightShirtPixels);
                        break;

                    default:
                        break;
                }
            }
        }

        // Apply shirt texture pixels
        farmerBaseShirtsUpdated.Apply();
    }

    private void SetTextureToTransparent(Texture2D texture2D) {
        Color[] fill = new Color[texture2D.height * texture2D.width];

        for (int i = 0; i < fill.Length; i++) {
            fill[i] = Color.clear;      // Color(0, 0, 0, 0)
        }

        texture2D.SetPixels(fill);
    }

    #endregion


    #region Arm Customisation

    private void PopulateArmColorSwapList() {
        // Clear color swap list
        colorSwapList.Clear();

        // Arm replacement colors
        colorSwapList.Add(new colorSwap(armTargetColor1, selectedShirt.GetPixel(0, 7)));
        colorSwapList.Add(new colorSwap(armTargetColor2, selectedShirt.GetPixel(0, 6)));
        colorSwapList.Add(new colorSwap(armTargetColor3, selectedShirt.GetPixel(0, 5)));
    }

    private void ChangePixelColors(Color[] baseArray, List<colorSwap> colorSwapList) {
        for (int i = 0; i < baseArray.Length; i++) {
            // Loop through color swap list
            if (colorSwapList.Count > 0) {
                for (int j = 0; j < colorSwapList.Count; j++) {
                    if (baseArray[i] == colorSwapList[j].fromColor) {
                        baseArray[i] = colorSwapList[j].toColor;
                    }
                }
            }
        }
    }

    #endregion


    #region Trouser Customisation

    private void TintPixelColors(Color[] basePixelArray, Color tintColor)  {
        // Loop through pixels to tint
        for (int i = 0; i < basePixelArray.Length; i++) {
            basePixelArray[i].r *= tintColor.r;
            basePixelArray[i].g *= tintColor.g;
            basePixelArray[i].b *= tintColor.b;
        }
    }

    #endregion


    #region Hair Customisation

    private void AddHairToTexture(int hairStyleNo) {
        // Calculate coordinates for hair pixels
        int y = (hairStyleNo / hairStylesInSpriteWidth) * hairTextureHeight;
        int x = (hairStyleNo % hairStylesInSpriteWidth) * hairTextureWidth;

        // Get hair pixels
        Color[] hairPixels = hairBaseTexture.GetPixels(x, y, hairTextureWidth, hairTextureHeight);

        // Apply selected hair pixels to texture
        hairCustomised.SetPixels(hairPixels);
        hairCustomised.Apply();
    }

    #endregion


    #region Skin Customisation

    private void PopulateSkinColorSwapList(int skinType) {
        // Clear color swap list
        colorSwapList.Clear();

        // Skin replacement colors
        // Switch on skin type
        switch (skinType) {
            case 1:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(187, 157, 128, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(231, 187, 144, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(221, 186, 154, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(213, 189, 167, 255)));
                break;

            case 2:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(105, 69, 2, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(128, 87, 12, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(145, 103, 26, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(161, 114, 25, 255)));
                break;

            case 3:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(151, 132, 0, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(187, 166, 15, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(209, 188, 39, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(211, 199, 112, 255)));
                break;

            default:
                colorSwapList.Add(new colorSwap(skinTargetColor1, skinTargetColor1));
                colorSwapList.Add(new colorSwap(skinTargetColor2, skinTargetColor2));
                colorSwapList.Add(new colorSwap(skinTargetColor3, skinTargetColor3));
                colorSwapList.Add(new colorSwap(skinTargetColor4, skinTargetColor4));
                break;
        }
    }

    #endregion


    #region Hat Customisation

    private void AddHatToTexture(int hatStyleNo) {
        // Calculate coordinates for hat pixels
        int y = (hatStyleNo / hatStylesInSpriteWidth) * hatTextureHeight;
        int x = (hatStyleNo % hatStylesInSpriteWidth) * hatTextureWidth;

        // Get hat pixels
        Color[] hatPixels = hatsBaseTexture.GetPixels(x, y, hatTextureWidth, hatTextureHeight);

        // Apply selected hat pixels to texture
        hatsCustomised.SetPixels(hatPixels);
        hatsCustomised.Apply();
    }

    #endregion


    #region Adornment Customisation

    private void PopulateBodyAdornmentsOffsetArray() {
        bodyAdornmentsOffsetArray[0, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 0] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 1] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 2] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 3] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 4] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 5] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 6] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 7] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 8] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 9] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 10] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 10] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 10] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[3, 10] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[4, 10] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[5, 10] = new Vector2Int(0, 0 + 16);

        bodyAdornmentsOffsetArray[0, 11] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[1, 11] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[2, 11] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[3, 11] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 11] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 11] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 12] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 12] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 12] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[3, 12] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[4, 12] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 12] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 13] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 13] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 13] = new Vector2Int(1, -1 + 16);
        bodyAdornmentsOffsetArray[3, 13] = new Vector2Int(1, -1 + 16);
        bodyAdornmentsOffsetArray[4, 13] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 13] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 14] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 14] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 14] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[3, 14] = new Vector2Int(0, -5 + 16);
        bodyAdornmentsOffsetArray[4, 14] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 14] = new Vector2Int(0, 1 + 16);

        bodyAdornmentsOffsetArray[0, 15] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[1, 15] = new Vector2Int(0, -5 + 16);
        bodyAdornmentsOffsetArray[2, 15] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 15] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[4, 15] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 15] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 16] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 16] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 16] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[3, 16] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[4, 16] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 16] = new Vector2Int(0, 0 + 16);

        bodyAdornmentsOffsetArray[0, 17] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[1, 17] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[2, 17] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 17] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 17] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 17] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 18] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 18] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 18] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 19] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 19] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 19] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 20] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 20] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 20] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 20] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 20] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 20] = new Vector2Int(99, 99);
    }

    private void AddAdornmentsToTexture(int adornmentsStyleNo) {
        // Create adornment texture
        selectedAdornment = new Texture2D(adornmentsTextureWidth, adornmentsTextureHeight);
        selectedAdornment.filterMode = FilterMode.Point;

        // Calculate coordinates for adornments pixels
        int y = (adornmentsStyleNo / adornmentsStylesInSpriteWidth) * adornmentsTextureHeight;
        int x = (adornmentsStyleNo % adornmentsStylesInSpriteWidth) * adornmentsTextureWidth;

        // Get adornments pixels
        Color[] adornmentsPixels = adornmentsBaseTexture.GetPixels(x, y, adornmentsTextureWidth, adornmentsTextureHeight);

        // Apply selected adornments pixels to texture
        selectedAdornment.SetPixels(adornmentsPixels);
        selectedAdornment.Apply();
    }

    private void ApplyAdornmentsTextureToBase() {
        Color[] frontAdornmentsPixels;
        Color[] rightAdornmentsPixels;

        frontAdornmentsPixels = selectedAdornment.GetPixels(0, adornmentsSpriteHeight * 1, adornmentsSpriteWidth, adornmentsSpriteHeight);
        rightAdornmentsPixels = selectedAdornment.GetPixels(0, adornmentsSpriteHeight * 0, adornmentsSpriteWidth, adornmentsSpriteHeight);

        for (int x = 0; x < bodyColumns; x++) {
            for (int y = 0; y < bodyRows; y++) {
                int pixelX = x * farmerSpriteWidth;
                int pixelY = y * farmerSpriteHeight;

                if (bodyAdornmentsOffsetArray[x, y] != null) {
                    pixelX += bodyAdornmentsOffsetArray[x, y].x;
                    pixelY += bodyAdornmentsOffsetArray[x, y].y;
                }

                // Switch on facing direction
                switch (bodyFacingMatrix[x, y]) {
                    case Facing.Front:
                        // Populate front adornments pixels
                        farmerBaseAdornmentsUpdated.SetPixels(pixelX, pixelY, adornmentsSpriteWidth, adornmentsSpriteHeight, frontAdornmentsPixels);
                        break;

                    case Facing.Right:
                        // Populate right adornments pixels
                        farmerBaseAdornmentsUpdated.SetPixels(pixelX, pixelY, adornmentsSpriteWidth, adornmentsSpriteHeight, rightAdornmentsPixels);
                        break;

                    default:
                        break;
                }
            }
        }

        // Apply adornments texture pixels
        farmerBaseAdornmentsUpdated.Apply();
    }

    #endregion


    #region Merge

    private void MergeColourArray(Color[] baseArray, Color[] mergeArray) {
        for (int i = 0; i < baseArray.Length; i++) {
            if (mergeArray[i].a > 0) {
                //Merge array has color
                if (mergeArray[i].a >= 1) {
                    // Fully replace
                    baseArray[i] = mergeArray[i];
                }
                else{
                    //Interpolate colors
                    float alpha = mergeArray[i].a;

                    baseArray[i].r += (mergeArray[i].r - baseArray[i].r) * alpha;
                    baseArray[i].g += (mergeArray[i].g - baseArray[i].g) * alpha;
                    baseArray[i].b += (mergeArray[i].b - baseArray[i].b) * alpha;
                    baseArray[i].a += mergeArray[i].a;
                }
            }
        }
    }

    #endregion

}
