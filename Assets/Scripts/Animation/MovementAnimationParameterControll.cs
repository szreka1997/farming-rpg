using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimationParameterControll : MonoBehaviour
{
    private Animator myAnimator;


    private void Awake() {
        myAnimator = GetComponent<Animator>();
    }


    private void OnEnable() {
        EventHandler.MovementEvent += SetAnimationParameters;
    }

    private void OnDisable() {
        EventHandler.MovementEvent -= SetAnimationParameters;
    }


    private void AnimationEventPlayFootstepSound() {
        AudioManager.Instance.PlaySound(SoundName.EffectFootstepHardGround);
    }

    /// <summary>
	/// Sets the animation parameters if the Movement event called
	/// </summary>
    private void SetAnimationParameters(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle, bool isCarrying, 
            ToolEffect toolEffect,
            bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
            bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
            bool isSwingingToolRight, bool isSwingingToolLeft, bool isSwingingToolUp, bool isSwingingToolDown,
            bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown,
            bool idleRight, bool idleLeft, bool idleUp, bool idleDown) {

        myAnimator.SetFloat(Settings.xInput, xInput);
        myAnimator.SetFloat(Settings.yInput, yInput);
        myAnimator.SetBool(Settings.isWalking, isWalking);
        myAnimator.SetBool(Settings.isRunning, isRunning);
        
        myAnimator.SetInteger(Settings.toolEffect, (int)toolEffect);

        if(isUsingToolRight) {
            myAnimator.SetTrigger(Settings.isUsingToolRight);
        }
        if(isUsingToolLeft) {
            myAnimator.SetTrigger(Settings.isUsingToolLeft);
        }
        if(isUsingToolUp) {
            myAnimator.SetTrigger(Settings.isUsingToolUp);
        }
        if(isUsingToolDown) {
            myAnimator.SetTrigger(Settings.isUsingToolDown);
        }

        if(isLiftingToolRight) {
            myAnimator.SetTrigger(Settings.isLiftingToolRight);
        }
        if(isLiftingToolLeft) {
            myAnimator.SetTrigger(Settings.isLiftingToolLeft);
        }
        if(isLiftingToolUp) {
            myAnimator.SetTrigger(Settings.isLiftingToolUp);
        }
        if(isLiftingToolDown) {
            myAnimator.SetTrigger(Settings.isLiftingToolDown);
        }

        if(isSwingingToolRight) {
            myAnimator.SetTrigger(Settings.isSwingingToolRight);
        }
        if(isSwingingToolLeft) {
            myAnimator.SetTrigger(Settings.isSwingingToolLeft);
        }
        if(isSwingingToolUp) {
            myAnimator.SetTrigger(Settings.isSwingingToolUp);
        }
        if(isSwingingToolDown) {
            myAnimator.SetTrigger(Settings.isSwingingToolDown);
        }

        if(isPickingRight) {
            myAnimator.SetTrigger(Settings.isPickingRight);
        }
        if(isPickingLeft) {
            myAnimator.SetTrigger(Settings.isPickingLeft);
        }
        if(isPickingUp) {
            myAnimator.SetTrigger(Settings.isPickingUp);
        }
        if(isPickingDown) {
            myAnimator.SetTrigger(Settings.isPickingDown);
        }

        if(idleRight) {
            myAnimator.SetTrigger(Settings.idleRight);
        }
        if(idleLeft) {
            myAnimator.SetTrigger(Settings.idleLeft);
        }
        if(idleUp) {
            myAnimator.SetTrigger(Settings.idleUp);
        }
        if(idleDown) {
            myAnimator.SetTrigger(Settings.idleDown);
        } 
    }



    
}
