public class Em073Controller : Em015Controller
{
	public override void LogicUpdate()
	{
		logicFrameNow = GameLogicUpdateManager.GameFrame;
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		switch (emState)
		{
		case EmState.INIT:
			_velocity = VInt3.zero;
			emState = EmState.BORN;
			break;
		case EmState.BORN:
			_velocity = VInt3.zero;
			emState = EmState.IDLE_ARMOR;
			break;
		case EmState.IDLE_ARMOR:
			_velocity = VInt3.zero;
			bombMesh.enabled = true;
			_animator.Play(HASH_RUN, 0, 0f);
			emState = EmState.RUN;
			UpdateLogicToNext(logicRun);
			break;
		case EmState.IDLE_NULL:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE_ARMOR;
			}
			break;
		case EmState.RUN:
			if (logicFrameNow > logicToNext)
			{
				_animator.Play(HASH_IDLE_ARMOR, 0, 0f);
				emState = EmState.PRE_SKL_01;
				UpdateLogicToNext(logicWaitSkl);
			}
			break;
		case EmState.PRE_SKL_01:
			if (logicFrameNow > logicToNext)
			{
				Fire();
			}
			break;
		case EmState.SKL_01:
			if (logicFrameNow > logicToNext)
			{
				_animator.Play(HASH_IDLE_NULL, 0);
				emState = EmState.IDLE_NULL;
				UpdateLogicToNext(logicIdle);
			}
			break;
		case EmState.HURT:
			break;
		}
	}
}
