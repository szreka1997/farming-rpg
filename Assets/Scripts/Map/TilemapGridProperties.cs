using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class TilemapGridProperties : MonoBehaviour
{
    // This code will only get compile if we're in the unity editor
    // If we're compiling this for a standalone build, this code isn't gonna be compiled
    #if UNITY_EDITOR

    #region Variables

    [SerializeField] private SO_GridProperties so_GridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.Diggable;

    private Tilemap tilemap;

    #endregion



    #region Unity Basic Methods

    private void Update() {        
        // Only populate in the editor
        if(!Application.IsPlaying(gameObject)) {
            Debug.Log("DISABLE PROPERTY TILEMAPS");
        }
    }

    #endregion



    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        // Only populate in the editor
        if(!Application.IsPlaying(gameObject)) {
            tilemap = GetComponent<Tilemap>();

            if(so_GridProperties != null) {
                so_GridProperties.gridPropertyList.Clear();
            }
        }
    }

    private void OnDisable() {        
        // Only populate in the editor
        if (!Application.IsPlaying(gameObject)) {
            UpdateGridProperties();

            if (so_GridProperties != null) {
                // This is required to ensure that the updated gridproperties gameobject gets saved when the game is saved - 
                // otherwise they are not saved.
                EditorUtility.SetDirty(so_GridProperties);
            }
        }
    }

    #endregion



    #region Private Methods 

    private void UpdateGridProperties() {
        // Compress timemap bounds
        // Compresses the bounds to just the tiles that are painted on the tilemap
        tilemap.CompressBounds();

        // Only populate in the editor
        if (!Application.IsPlaying(gameObject)) {
            if (so_GridProperties != null) {
                Vector3Int startCell = tilemap.cellBounds.min;  // Bottom Left corner
                Vector3Int endCell = tilemap.cellBounds.max;    // Top Right corner

                for(int x = startCell.x; x < endCell.x; x++) {
                    for(int y = startCell.y; y < endCell.y; y++) {
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        // if not null then the tile is painted
                        if (tile != null) {
                            so_GridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }
        }
    }

    #endregion

    #endif

}