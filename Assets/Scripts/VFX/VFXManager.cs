using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{
    [SerializeField] private GameObject reapingGrassYellowPrefab = null;
    [SerializeField] private GameObject reapingGrassBluePrefab = null;
    [SerializeField] private GameObject reapingCactusPrefab = null;
    [SerializeField] private GameObject KotShogLeavesFallingPrefab = null;
    [SerializeField] private GameObject FleckFallingPrefab = null;
    [SerializeField] private GameObject choppingTreeTrunkPurplePrefab = null;
    [SerializeField] private GameObject choppingTreeTrunkBluePrefab = null;
    [SerializeField] private GameObject breakingStonePrefab = null;

    private WaitForSeconds twoSeconds;


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();
        twoSeconds = new WaitForSeconds(2f);

    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.HarvestActionEffectEvent += DisplayHarvestActionEffect;
    }

    private void OnDisable() {
        EventHandler.HarvestActionEffectEvent -= DisplayHarvestActionEffect;
    }

    #endregion



    private void DisplayHarvestActionEffect(Vector3 effectPosition, HarvestActionEffect harvestActionEffect) {
        switch (harvestActionEffect) {
            case HarvestActionEffect.ReapingGrassYellow:
                GameObject reapingGrassYellow = PoolManager.Instance.ReuseObject(reapingGrassYellowPrefab, effectPosition, Quaternion.identity);
                reapingGrassYellow.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(reapingGrassYellow, twoSeconds));
                break;

            case HarvestActionEffect.ReapingGrassBlue:
                GameObject reapingGrassBlue = PoolManager.Instance.ReuseObject(reapingGrassBluePrefab, effectPosition, Quaternion.identity);
                reapingGrassBlue.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(reapingGrassBlue, twoSeconds));
                break;

            case HarvestActionEffect.ReapingCactus:
                GameObject reapingCactus = PoolManager.Instance.ReuseObject(reapingCactusPrefab, effectPosition, Quaternion.identity);
                reapingCactus.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(reapingCactus, twoSeconds));
                break;

            case HarvestActionEffect.KotShogLeavesFalling:
                GameObject deciduousLeaveFalling = PoolManager.Instance.ReuseObject(KotShogLeavesFallingPrefab, effectPosition, Quaternion.identity);
                deciduousLeaveFalling.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(deciduousLeaveFalling, twoSeconds));
                break;

            case HarvestActionEffect.FleckLeavesFalling:
                GameObject pineConesFalling = PoolManager.Instance.ReuseObject(FleckFallingPrefab, effectPosition, Quaternion.identity);
                pineConesFalling.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(pineConesFalling, twoSeconds));
                break;

            case HarvestActionEffect.ChoppingTreeTrunkPurple:
                GameObject choppingTreeTrunkPurple = PoolManager.Instance.ReuseObject(choppingTreeTrunkPurplePrefab, effectPosition, Quaternion.identity);
                choppingTreeTrunkPurple.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(choppingTreeTrunkPurple, twoSeconds));
                break;

            case HarvestActionEffect.ChoppingTreeTrunkBlue:
                GameObject choppingTreeTrunkBlue = PoolManager.Instance.ReuseObject(choppingTreeTrunkBluePrefab, effectPosition, Quaternion.identity);
                choppingTreeTrunkBlue.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(choppingTreeTrunkBlue, twoSeconds));
                break;


            case HarvestActionEffect.BreakingStone:
                GameObject breakingStone = PoolManager.Instance.ReuseObject(breakingStonePrefab, effectPosition, Quaternion.identity);
                breakingStone.SetActive(true);
                StartCoroutine(DisableHarvestActionEffectRoutine(breakingStone, twoSeconds));
                break;

            default:
                break;
        }
    }

    private IEnumerator DisableHarvestActionEffectRoutine(GameObject effectGameObject, WaitForSeconds secondsToWait) {
        yield return secondsToWait;
        effectGameObject.SetActive(false);
    }


}