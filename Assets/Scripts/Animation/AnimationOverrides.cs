using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AnimationOverrides : MonoBehaviour
{
    #region Variables

    [SerializeField] private GameObject player = null;
    [SerializeField] private SO_AnimationType[] soAnimationTypeArray = null;

    private Dictionary<AnimationClip, SO_AnimationType> animationTypeDictionaryByAnimationClipKey;
    private Dictionary<string, SO_AnimationType> animationTypeDictionaryByCompositeAttributeKey;

    #endregion



    #region Unity Basic Methods

    private void Start() {
        InitialiseAnimationTypeDictionaryByAnimationClipKey();
        InitialiseAnimationTypeDictionaryByCompositeAttributeKey();
    }

    #endregion



    #region Public Methods

    public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributeList) {
        foreach(CharacterAttribute characterAttribute in characterAttributeList) {
            Animator currentAnimator = null;
            List<KeyValuePair<AnimationClip, AnimationClip>> animationKeyValuePairList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            
            string animatorSOAssetName = characterAttribute.characterPart.ToString();
            Animator[] animatorArray = player.GetComponentsInChildren<Animator>();

            foreach (Animator animator in animatorArray) {
                if (animator.name == animatorSOAssetName) {
                    currentAnimator = animator;
                    break;
                }
            }

            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);

            // Parsing an array to the list will automatically create the list based on the array
            List<AnimationClip> currentAnimationClipList = new List<AnimationClip>(animatorOverrideController.animationClips);

            foreach (AnimationClip animationClip in currentAnimationClipList) {
                SO_AnimationType so_AnimationType;
                bool foundAnimation = animationTypeDictionaryByAnimationClipKey.TryGetValue(animationClip, out so_AnimationType);

                if (foundAnimation) {
                    string key = characterAttribute.characterPart.ToString() +
                                 characterAttribute.partVariantColour.ToString() +
                                 characterAttribute.partVariantType.ToString() +
                                 so_AnimationType.animationName.ToString();

                    SO_AnimationType swapSO_AnimationType;
                    bool foundSwapAnimation = animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out swapSO_AnimationType);

                    if (foundSwapAnimation) {
                        AnimationClip swapAnimationClip = swapSO_AnimationType.animationClip;
                        animationKeyValuePairList.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, swapAnimationClip));
                    }
                }
            }

            // Apply animation updates to animation override controller and then update animator with the new controller
            animatorOverrideController.ApplyOverrides(animationKeyValuePairList);
            currentAnimator.runtimeAnimatorController = animatorOverrideController;
        }
    }

    #endregion



    #region Private Methods

    private void InitialiseAnimationTypeDictionaryByAnimationClipKey() {
        animationTypeDictionaryByAnimationClipKey = new Dictionary<AnimationClip, SO_AnimationType>();

        foreach (SO_AnimationType item in soAnimationTypeArray) {
            animationTypeDictionaryByAnimationClipKey.Add(item.animationClip, item);
        }
    }

    private void InitialiseAnimationTypeDictionaryByCompositeAttributeKey() {
        animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();

        foreach (SO_AnimationType item in soAnimationTypeArray) {
            string key = item.characterPart.ToString() +
                         item.partVariantColour.ToString() +
                         item.partVariantType.ToString() +
                         item.animationName.ToString();

            animationTypeDictionaryByCompositeAttributeKey.Add(key, item);
        }
    }

    #endregion




}