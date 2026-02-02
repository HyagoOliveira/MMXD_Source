using System;
using StageLib;

public class BS074_Controller : BS034_Controller
{
	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		if (AiState == AI_STATE.mob_001)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
		}
	}

	protected override void UpdateRandomState()
	{
		MainStatus mainStatus;
		if (IsCatch)
		{
			mainStatus = MainStatus.PullClaw;
		}
		else if (AiState == AI_STATE.mob_001)
		{
			mainStatus = MainStatus.Jump;
			if (OrangeBattleUtility.Random(0, 100) < 40)
			{
				mainStatus = MainStatus.PushAttack;
			}
		}
		else
		{
			int num = 3;
			if (!CheckCanShootClaw())
			{
				num++;
			}
			mainStatus = (MainStatus)((!(Target != null) || !(Math.Abs(Target._transform.position.x - _transform.position.x) < 2f)) ? OrangeBattleUtility.Random(num, 5) : OrangeBattleUtility.Random(num, 6));
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus);
				SetStatus(MainStatus.IdleWaitNet);
			}
		}
		else
		{
			SetStatus(mainStatus);
		}
	}
}
