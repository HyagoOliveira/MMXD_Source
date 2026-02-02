using System.Collections.Generic;

public class OrangeTimerManager : MonoBehaviourSingleton<OrangeTimerManager>
{
	private static readonly List<OrangeTimer> RunningTimerList = new List<OrangeTimer>();

	private static readonly List<OrangeTimer> StaticTimerList = new List<OrangeTimer>();

	private static readonly Queue<OrangeTimer> IdleTimerQueue = new Queue<OrangeTimer>();

	private static int _maxTimerUsed = 400;

	public void Awake()
	{
		GenerateTimer(_maxTimerUsed);
	}

	protected override void OnDestroy()
	{
		RunningTimerList.Clear();
		IdleTimerQueue.Clear();
		StaticTimerList.Clear();
		base.OnDestroy();
	}

	private static void GenerateTimer(int count)
	{
		for (int i = 0; i < count; i++)
		{
			OrangeTimer item = new OrangeTimer();
			IdleTimerQueue.Enqueue(item);
		}
	}

	public static OrangeTimer GetTimer(TimerMode mode = TimerMode.FRAME)
	{
		OrangeTimer orangeTimer = ((IdleTimerQueue.Count == 0) ? new OrangeTimer() : IdleTimerQueue.Dequeue());
		RunningTimerList.Add(orangeTimer);
		orangeTimer.InitTimer(mode);
		return orangeTimer;
	}

	public static OrangeTimer GetStaticTimer(TimerMode mode = TimerMode.FRAME)
	{
		OrangeTimer orangeTimer = new OrangeTimer();
		StaticTimerList.Add(orangeTimer);
		orangeTimer.InitTimer(mode);
		return orangeTimer;
	}

	public static void ReturnTimer(OrangeTimer timer)
	{
		timer.TimerReset();
		RunningTimerList.Remove(timer);
		IdleTimerQueue.Enqueue(timer);
	}

	public static void ReturnAll()
	{
		foreach (OrangeTimer runningTimer in RunningTimerList)
		{
			runningTimer.TimerReset();
			IdleTimerQueue.Enqueue(runningTimer);
		}
		RunningTimerList.Clear();
	}

	public static void PauseAll()
	{
		foreach (OrangeTimer runningTimer in RunningTimerList)
		{
			runningTimer.TimerPause();
		}
		foreach (OrangeTimer staticTimer in StaticTimerList)
		{
			staticTimer.TimerPause();
		}
	}

	public static void ResumeAll()
	{
		foreach (OrangeTimer runningTimer in RunningTimerList)
		{
			runningTimer.TimerResume();
		}
		foreach (OrangeTimer staticTimer in StaticTimerList)
		{
			staticTimer.TimerResume();
		}
	}
}
