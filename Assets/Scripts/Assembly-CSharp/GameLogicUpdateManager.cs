using System.Collections.Generic;
using UnityEngine;

public class GameLogicUpdateManager : MonoBehaviourSingleton<GameLogicUpdateManager>
{
	public static Fixed64 g_fixFrameLen = Fixed64.FromRaw(200L);

	public static float m_fFrameLen = (float)g_fixFrameLen;

	public static float m_fFrameLenMS = (float)g_fixFrameLen * 1000f;

	public static VInt g_fixFrameLenFP = new VInt(m_fFrameLen);

	public static int GameFrame = 0;

	public bool isStart;

	public bool isPause;

	private LogicTrigger logicTrigger;

	private List<ILogicUpdate> listGameUpdate = new List<ILogicUpdate>();

	public void Init()
	{
		GameFrame = 0;
		logicTrigger = new LogicTrigger();
		isStart = true;
	}

	public void Stop()
	{
		GameFrame = 0;
		isStart = false;
		listGameUpdate.Clear();
	}

	public bool CheckUpdateContain(ILogicUpdate logicUpdate)
	{
		if (listGameUpdate.Contains(logicUpdate))
		{
			return true;
		}
		return false;
	}

	public int TimeToLogicFrame(float time)
	{
		return (int)(time / m_fFrameLen);
	}

	public void AddUpdate(ILogicUpdate logicUpdate)
	{
		if (!listGameUpdate.Contains(logicUpdate))
		{
			listGameUpdate.Add(logicUpdate);
		}
	}

	public void RemoveUpdate(ILogicUpdate logicUpdate)
	{
		if (listGameUpdate.Contains(logicUpdate))
		{
			listGameUpdate.Remove(logicUpdate);
		}
	}

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BATTLE_START, Init);
	}

	private void Update()
	{
		if (isStart && !isPause && logicTrigger.IsTrigger(Time.deltaTime, ref GameFrame))
		{
			for (int i = 0; i < listGameUpdate.Count; i++)
			{
				listGameUpdate[i].LogicUpdate();
			}
		}
	}
}
