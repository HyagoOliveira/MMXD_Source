using System;
using System.Collections.Generic;

public abstract class BossControllerProxyBaseGen1 : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		NONE = -1,
		IDLE = 0,
		DEBUT = 1,
		SEARCH_PLAYER = 2,
		RUN = 3,
		JUMP = 4,
		SKILL_0 = 5,
		SKILL_1 = 6,
		SKILL_2 = 7,
		SKILL_3 = 8,
		SKILL_4 = 9,
		SKILL_5 = 10,
		DIE = 11,
		NEXT_ACTION = 12,
		IDLE_WAIT_NET = 13,
		UPBOUND = 14
	}

	protected enum SubStatus
	{
		NONE = -1,
		PHASE_0 = 0,
		PHASE_1 = 1,
		PHASE_2 = 2,
		PHASE_3 = 3,
		PHASE_4 = 4,
		PHASE_5 = 5,
		PHASE_6 = 6,
		PHASE_7 = 7,
		PHASE_8 = 8,
		PHASE_9 = 9,
		UPBOUND = 10
	}

	private Dictionary<int, int> _animationHashes = new Dictionary<int, int>();

	protected MainStatus CurMainStatus { get; private set; }

	protected SubStatus CurSubStatus { get; private set; }

	protected int CurrentAnimationId { get; private set; }

	protected bool IsInGround
	{
		get
		{
			return Controller.Collisions.below;
		}
	}

	protected bool IsWallTouched
	{
		get
		{
			if (!Controller.Collisions.left)
			{
				return Controller.Collisions.right;
			}
			return true;
		}
	}

	public abstract void UpdateFunc();

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.PHASE_0)
	{
		CurMainStatus = mainStatus;
		CurSubStatus = subStatus;
		OnStatusChanged(CurMainStatus, CurSubStatus);
	}

	protected virtual void OnStatusChanged(MainStatus mainStatus, SubStatus subStatus)
	{
	}

	public sealed override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		OnAIStateUpdated(AiState);
	}

	protected virtual void OnAIStateUpdated(AI_STATE aiState)
	{
	}

	public override void BossIntro(Action callback)
	{
		IntroCallBack = callback;
		_introReady = true;
	}

	protected void RegistorAnimation(int animationId, int animationHash)
	{
		_animationHashes[animationId] = animationHash;
	}

	protected void PlayAnimation(int animationId, int layer = 0, int normalizedTime = 0)
	{
		int value;
		if (_animationHashes.TryGetValue(animationId, out value))
		{
			_animator.Play(value, layer, normalizedTime);
			CurrentAnimationId = animationId;
		}
	}
}
