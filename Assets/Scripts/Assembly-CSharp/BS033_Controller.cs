using CodeStage.AntiCheat.ObscuredTypes;

public class BS033_Controller : BS033_EventController
{
	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return BaseHurt(tHurtPassParam);
	}

	public override void SetActive(bool isActive)
	{
		AI_STATE aiStage = _aiStage;
		if (aiStage == AI_STATE.mob_003)
		{
			DoSetActive(isActive);
			if (isActive)
			{
				_originalXPos = _transform.position.x;
				_originalYPos = _transform.position.y;
				_targetXPos = _originalXPos;
				_globalWaypoints[0] = _originalYPos + 0.25f;
				_globalWaypoints[1] = _originalYPos - 0.25f;
				Hp = EnemyData.n_HP;
				_mainStatus = MainStatus.IDLE;
				Controller.enabled = true;
				_aimIk.enabled = true;
				SetColliderEnable(true);
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			else
			{
				Activate = false;
				_collideBullet.BackToPool();
				Controller.enabled = false;
				_aimIk.enabled = false;
				SetColliderEnable(false);
				AiTimer.TimerStop();
				base.SoundSource.PlaySE("BossSE", AudioManager.FormatEnum2Name(BossSE.CRI_BOSSSE_BS104_BEE01_STOP.ToString()));
			}
		}
		else
		{
			base.SetActive(isActive);
		}
	}
}
