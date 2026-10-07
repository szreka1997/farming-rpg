using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }


    /// <summary>
    /// Switch the collider that cinemachine uses to define the edges of the screen
    /// </summary>
    private void SwitchBoundingShape() {
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();
        CinemachineConfiner2D cinemachineConfiner2D = GetComponent<CinemachineConfiner2D>();
        
        cinemachineConfiner2D.m_BoundingShape2D = polygonCollider2D;

        // Since the confiner bounds have changed need to call this to clear the cache
        cinemachineConfiner2D.InvalidateCache();
    }

    
}
