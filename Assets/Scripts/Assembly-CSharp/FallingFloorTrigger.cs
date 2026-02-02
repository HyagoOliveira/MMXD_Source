using UnityEngine;

[RequireComponent(typeof(FallingFloor))]
[RequireComponent(typeof(Controller2D))]
public class FallingFloorTrigger : MonoBehaviour, ILogicUpdate
{
	public int delayMs = 1000;

	public ParticleSystem mFX;

	private OrangeTimer _delayTimer;

	private Controller2D _controller2D;

	private FallingFloor _fallingFloor;

	private void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	private void Start()
	{
		_delayTimer = OrangeTimerManager.GetTimer();
		_controller2D = GetComponent<Controller2D>();
		_fallingFloor = GetComponent<FallingFloor>();
	}

	public void LogicUpdate()
	{
		if (!_delayTimer.IsStarted())
		{
			if ((bool)_controller2D.ObjectMeeting(0f, 0.25f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer))
			{
				_delayTimer.TimerStart();
				if (mFX != null && !mFX.isPlaying)
				{
					mFX.Play();
				}
			}
		}
		else if (_delayTimer.GetMillisecond() > delayMs)
		{
			if (mFX != null && mFX.isPlaying)
			{
				mFX.Stop();
			}
			_fallingFloor.TriggerFall();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			return;
		}
		_controller2D.Move(VInt3.zero);
	}
}
