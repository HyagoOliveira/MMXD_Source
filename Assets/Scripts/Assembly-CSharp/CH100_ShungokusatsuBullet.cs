using System;
using UnityEngine;

public class CH100_ShungokusatsuBullet : BasicBullet
{
    protected Transform _tfHitTarget;

    protected StageObjBase _sobHitTarget;

    protected long _duration = -1L;

    protected long _hurtCycle;

    protected long _hitCnt = 1L;

    public double MaxHit = 5.0;

    public bool bNeedBackPoolColliderBullet;

    public bool bNeedBackPoolModelName;

    protected OrangeTimer _otHurtTimer;

    public string sCycleACB = "";

    public string sCycleCUE = "";

    protected override void Awake()
    {
        base.Awake();
        _otHurtTimer = OrangeTimerManager.GetTimer();
    }

    public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
    {
        BulletID = pData.n_ID;
        BulletData = pData;
        nNetID = nInNetID;
        nRecordID = nInRecordID;
        Owner = owner;
        if (pData.s_FIELD != "null")
        {
            string[] array = pData.s_FIELD.Split(',');
            _duration = long.Parse(array[5]);
            _hurtCycle = long.Parse(array[6]);
            MaxHit = Math.Truncate((double)_duration / (double)_hurtCycle);
        }
        FxMuzzleFlare = BulletData.s_USE_FX;
        FxImpact = BulletData.s_HIT_FX;
        FxEnd = BulletData.s_VANISH_FX;
        if (FxMuzzleFlare == "null")
        {
            FxMuzzleFlare = string.Empty;
        }
        else
        {
            MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FxMuzzleFlare, 5);
        }
        if (FxImpact == "null")
        {
            FxImpact = string.Empty;
        }
        else
        {
            MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FxImpact, 5);
        }
        if (FxEnd == "null")
        {
            FxEnd = string.Empty;
        }
        else
        {
            MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FxEnd, 5);
        }
        BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
        if (((uint)BulletData.n_FLAG & 2u) != 0)
        {
            BulletMask = BulletScriptableObjectInstance.BulletLayerMaskBullet;
        }
        _UseSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_USE_SE);
        _HitSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_SE);
        if (_UseSE[0] != "" && (_UseSE[1].EndsWith("_lp") || _UseSE[1].EndsWith("_lg")))
        {
            checkLoopSE = true;
        }
        GetHistGuardSE();
    }

    public void SetHitTarget(Transform pTarget)
    {
        _tfHitTarget = pTarget;
        if (!_tfHitTarget)
        {
            return;
        }
        StageObjParam stageObjParam = _tfHitTarget.GetComponent<StageObjParam>();
        if (stageObjParam == null)
        {
            PlayerCollider component = _tfHitTarget.GetComponent<PlayerCollider>();
            if (component != null && component.IsDmgReduceShield())
            {
                stageObjParam = component.GetDmgReduceOwner();
            }
        }
        if (stageObjParam != null)
        {
            _sobHitTarget = stageObjParam.tLinkSOB;
        }
    }

    public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
    {
        base.Active(pPos, pDirection, pTargetMask, pTarget);
        _otHurtTimer.TimerStart();
        if ((bool)_tfHitTarget)
        {
            CaluDmg(BulletData, _tfHitTarget);
        }
    }

    public override void LateUpdateFunc()
    {
        if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
        {
            return;
        }
        if (_otHurtTimer.GetMillisecond() >= _hurtCycle && (double)_hitCnt < MaxHit)
        {
            _otHurtTimer.TimerStart();
            if (_tfHitTarget != null)
            {
                if (_sobHitTarget != null && (int)_sobHitTarget.Hp > 0)
                {
                    CaluDmg(BulletData, _tfHitTarget);
                }
                PlayCycleSE();
            }
            _hitCnt++;
        }
        if (_duration != -1 && ActivateTimer.GetMillisecond() >= _duration)
        {
            BackToPool();
        }
    }

    public override void BackToPool()
    {
        _hitCnt = 1L;
        _tfHitTarget = null;
        _otHurtTimer.TimerStop();
        MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
        ActivateTimer.TimerStop();
        _capsuleCollider.enabled = false;
        if (BackCallback != null)
        {
            BackCallback(this);
            BackCallback = null;
        }
        HitCallback = null;
        if (_UseSE != null && _UseSE[0] != "" && _UseSE[2] != "")
        {
            base.SoundSource.PlaySE(_UseSE[0], _UseSE[2]);
        }
        StopFx();
        isPetBullet = (isBossBullet = (isBuffTrigger = false));
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
        bIsEnd = true;
        if (bNeedBackPoolColliderBullet)
        {
            MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, "PoolColliderBullet");
        }
        else if (bNeedBackPoolModelName)
        {
            MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
        }
    }

    protected void PlayCycleSE()
    {
        if (sCycleACB != "" && sCycleCUE != "")
        {
            base.SoundSource.PlaySE(sCycleACB, sCycleCUE);
        }
    }
}
