using System.IO;

public class UpdateTimer
{
	private float fNowTime;

	private bool bStarted;

	public static UpdateTimer operator +(UpdateTimer a, float b)
	{
		a.fNowTime += b;
		return a;
	}

	public static UpdateTimer operator -(UpdateTimer a, float b)
	{
		a.fNowTime -= b;
		return a;
	}

	public static bool operator !=(UpdateTimer a, UpdateTimer b)
	{
		if (a.fNowTime == b.fNowTime)
		{
			return a.bStarted != b.bStarted;
		}
		return true;
	}

	public static bool operator ==(UpdateTimer a, UpdateTimer b)
	{
		if (a.fNowTime == b.fNowTime)
		{
			return a.bStarted == b.bStarted;
		}
		return false;
	}

	public void TimerStart()
	{
		bStarted = true;
		fNowTime = 0f;
	}

	public void TimerStop()
	{
		bStarted = false;
	}

	public long GetMillisecond()
	{
		return (long)fNowTime;
	}

	public bool IsStarted()
	{
		return bStarted;
	}

	public long GetTicks()
	{
		return (long)(fNowTime * 1000f / OrangeBattleUtility.FPS);
	}

	public void Write(BinaryWriter bw)
	{
		bw.WriteExFloat(fNowTime);
		bw.Write(bStarted);
	}

	public void Read(BinaryReader br)
	{
		fNowTime = br.ReadExFloat();
		bStarted = br.ReadBoolean();
	}

	public float GetTime()
	{
		return fNowTime;
	}

	public void SetTime(float p_fNowTime)
	{
		fNowTime = p_fNowTime;
	}

	public void SetStarted(bool p_bStarted)
	{
		bStarted = p_bStarted;
	}
}
