using OrangeSocket;
using StageLib;
using UnityEngine;
using cb;

[RequireComponent(typeof(OrangeConsoleCharacter))]
public class LockStepController : MonoBehaviour
{
	public static int LockStepTargetTimeFrame = 300000;

	private OrangeTimer _lockStepTimer;

	private bool _lockStepStarted;

	private static int _lockStepPause = 0;

	private readonly int _lockStepPhase = 2;

	public static bool LockStepPause
	{
		get
		{
			return _lockStepPause > 0;
		}
	}

	public void InitLockStepTimer()
	{
		_lockStepTimer = new OrangeTimer();
		_lockStepTimer.SetMode(TimerMode.MILLISECOND);
		_lockStepTimer.TimerStart();
		_lockStepStarted = true;
	}

	protected void OnEnable()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTLockStepMoveForward, OnNTLockStepMoveForward);
		MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLockStepMoveForward(false));
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTLockStepMoveForward, OnNTLockStepMoveForward);
		MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLockStepMoveForward(true));
		_lockStepStarted = false;
		_lockStepPause = 0;
	}

	public void OncePauseSync()
	{
		MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLockStepMoveForward(true));
		_lockStepStarted = false;
		_lockStepPause = 0;
	}

	private void OnNTLockStepMoveForward(object res)
	{
		_lockStepPause--;
		if (_lockStepPause <= 0)
		{
			_lockStepPause = 0;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
		}
		if (!StageUpdate.gbStageReady)
		{
			return;
		}
		if (_lockStepTimer != null)
		{
			_lockStepTimer.TimerStart();
			if (_lockStepPause > 0)
			{
				_lockStepTimer.SetMillisecondsOffset(-LockStepTargetTimeFrame);
			}
			_lockStepStarted = true;
		}
		else
		{
			InitLockStepTimer();
		}
	}

	private void Update()
	{
		if (_lockStepStarted && MonoBehaviourSingleton<CBSocketClient>.Instance.Connected() && StageUpdate.gbStageReady)
		{
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLockStepMoveForward(false));
			}
			if (_lockStepTimer != null && _lockStepTimer.GetMillisecond() > LockStepTargetTimeFrame)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
				_lockStepPause = _lockStepPhase;
			}
		}
	}
}
