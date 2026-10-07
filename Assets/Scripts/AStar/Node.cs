using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We need to implement that interface, because we're gonna store Nodes in generic list and want to use Sort method on list
public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0;             // distance from starting node
    public int hCost = 0;             // distance from finishing node
    public bool isObstacle = false;
    public int movementPenalty;
    public Node parentNode;

    public int FCost  { get => gCost + hCost;  }



    public Node(Vector2Int gridPosition) {
        this.gridPosition = gridPosition;
        parentNode = null;
    }



    public int CompareTo(Node nodeToCompare) {
        // compare will be < 0 if this instance Fcost is less than nodeToCompare.FCost
        // compare will be > 0 if this instance Fcost is greater than nodeToCompare.FCost
        // compare will be == 0 if the values are the same

        // FCost - int  (integers also can use CompareTo method)
        int compare = FCost.CompareTo(nodeToCompare.FCost);

        if (compare == 0) {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return compare;
    }


}
