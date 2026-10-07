using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRaycastable {
    CursorType GetCursorType();
    bool HandleRaycast(PlayerUIController callingController);
}
