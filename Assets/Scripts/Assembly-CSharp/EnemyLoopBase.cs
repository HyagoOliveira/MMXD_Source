using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class EnemyLoopBase : EnemyControllerBase
{
	protected OrangeTimer IdleSETimer;

	[Header("參數")]
	[SerializeField]
	protected int IdleSETime;

	[BoxGroup("Sound")]
	[SerializeField]
	public EnemySE IdleSE;

	[BoxGroup("Sound")]
	[SerializeField]
	public EnemySE02 IdleSE02;

	protected override void Awake()
	{
		base.Awake();
		IdleSETimer = OrangeTimerManager.GetTimer();
	}

	public virtual void UpdateFunc()
	{
		if (Activate && IdleSETimer.GetMillisecond() > IdleSETime)
		{
			if (IdleSE != 0)
			{
				PlaySE(IdleSE);
			}
			if (IdleSE02 != 0)
			{
				PlaySE(IdleSE02);
			}
			IdleSETimer.TimerReset();
			IdleSETimer.TimerStart();
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			IdleSETimer.TimerStart();
		}
		else
		{
			IdleSETimer.TimerStop();
		}
	}

	public void PauseIdleSETime()
	{
		if (IdleSETimer.IsStarted())
		{
			IdleSETimer.TimerPause();
		}
	}

	public void ContinueIdleSETime()
	{
		if (IdleSETimer.IsPaused())
		{
			IdleSETimer.TimerStart();
		}
	}
}
