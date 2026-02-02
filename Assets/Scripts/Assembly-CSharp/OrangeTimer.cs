using System.Diagnostics;

public class OrangeTimer
{
	private readonly Stopwatch _timer = new Stopwatch();

	private static readonly long Frequency = Stopwatch.Frequency;

	private long _offset;

	private TimerMode _mode;

	private int _pause;

	private bool _isTimerStarted;

	public OrangeTimer()
	{
		InitTimer();
	}

	public void InitTimer(TimerMode mode = TimerMode.FRAME)
	{
		_pause = 0;
		_timer.Reset();
		_mode = mode;
		_offset = 0L;
		_isTimerStarted = false;
	}

	public void SetMode(TimerMode mode)
	{
		_mode = mode;
	}

	public void TimerStart()
	{
		if (_pause == 0)
		{
			_timer.Reset();
			_offset = 0L;
		}
		else
		{
			_pause--;
		}
		if (_pause == 0)
		{
			_timer.Start();
			_isTimerStarted = true;
		}
	}

	public void TimerReset()
	{
		_pause = 0;
		_offset = 0L;
		_timer.Reset();
		_isTimerStarted = false;
	}

	public void TimerStop()
	{
		_timer.Reset();
		_offset = 0L;
		_isTimerStarted = false;
	}

	public void TimerPause()
	{
		if (IsStarted())
		{
			_pause++;
			_timer.Stop();
			_isTimerStarted = false;
		}
	}

	public void TimerResume()
	{
		if (IsStarted())
		{
			if (_pause > 0)
			{
				_pause--;
			}
			if (_pause == 0)
			{
				_timer.Start();
				_isTimerStarted = true;
			}
		}
	}

	public void SetOffset(long offset)
	{
		_offset = offset;
	}

	public void SetMillisecondsOffset(long offset)
	{
		_offset = offset * (Frequency / 1000);
	}

	public long GetMillisecond()
	{
		return (_timer.ElapsedTicks + _offset) / (Frequency / 1000);
	}

	public long GetTicks(int destMode = -1)
	{
		destMode = ((destMode == -1) ? ((int)_mode) : destMode);
		switch (destMode)
		{
		case 0:
			return (_timer.ElapsedTicks + _offset) / (Frequency / 1000);
		case 1:
			return (long)((float)(_timer.ElapsedTicks + _offset) / ((float)Frequency / OrangeBattleUtility.FPS));
		default:
			return _timer.ElapsedTicks + _offset;
		}
	}

	public bool IsStarted()
	{
		if (_isTimerStarted)
		{
			return _isTimerStarted;
		}
		return _timer.ElapsedTicks != 0;
	}

	public bool IsPaused()
	{
		return _pause != 0;
	}
}
