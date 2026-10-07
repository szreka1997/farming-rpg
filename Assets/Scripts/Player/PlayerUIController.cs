using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class PlayerUIController : MonoBehaviour
{
    #region Variables

    [SerializeField] CursorMapping[] cursorMappings = null;
    [SerializeField] float raycastRadius = 1f;

    #endregion


    #region Unity Basic Methods

    private void Update() {
        if (InteractWithUI())        { return; }
        if (InteractWithComponent()) { return; }

        SetCursor(CursorType.None);
    }

    #endregion


    private bool InteractWithUI() {
        if(FindObjectOfType<UIManager>().PauseMenuOn || FindObjectOfType<UIManager>().InventoryMenuOn || FindObjectOfType<UIManager>().ShopMenuOn) {
            /*if(!EventSystem.current.IsPointerOverGameObject()) {
                SetCursor(CursorType.UI);
            }*/

            return true;
        }

        return false;
    }

    private bool InteractWithComponent() {
        RaycastHit2D[] hits = RaycastAllSorted();

        foreach (RaycastHit2D hit in hits) {
            IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();

            foreach (IRaycastable raycastable in raycastables) {
                if (raycastable.HandleRaycast(this)) {
                    SetCursor(raycastable.GetCursorType());
                    return true;
                }
            }
        }

        SetCursor(CursorType.None);
        return false;
    }

    private RaycastHit2D[] RaycastAllSorted() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(GetMouseRay().origin, raycastRadius, GetMouseRay().direction);
        float[] distances = new float[hits.Length];

        for (int i = 0; i < hits.Length; i++) {
            distances[i] = hits[i].distance;
        }

        Array.Sort(distances, hits);
        return hits;
    }

    public void SetCursor(CursorType type) {
        CursorMapping mapping = GetCursorMapping(type);
        UnityEngine.Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
    }

    private CursorMapping GetCursorMapping(CursorType type) {
        foreach (CursorMapping mapping in cursorMappings) {
            if (mapping.type == type) {
                return mapping;
            }
        }

        return cursorMappings[0];
    }

    private static Ray GetMouseRay() {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }


}