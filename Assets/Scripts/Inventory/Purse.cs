using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purse : MonoBehaviour
{
    [SerializeField] private int startingBalance = 10000;

    private int balance = 0;


    private void Awake() {
        balance = startingBalance;
    }


    public int GetBalance() {
        return balance;
    }

    public void UpdateBalance(int amount) {
        balance += amount;
        EventHandler.CallPurseUpdatedEvent();
    }
    
}
