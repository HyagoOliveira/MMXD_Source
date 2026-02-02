#define RELEASE
using System.Collections.Generic;
using CallbackDefs;

public class BossCorpsTool
{
	public enum FightState
	{
		Rest = 0,
		Fighting = 1,
		Dead = 2
	}

	private EnemyControllerBase _enemy;

	private IF_Master _master;

	private IF_ForceExecute _enemyExecute;

	private int _mobID;

	private int _mobHp;

	private List<int> _missionStatus = new List<int>();

	private bool _autoReturn;

	public bool isObedient = true;

	public bool WaitMission = true;

	public bool isBossDead;

	public bool hasDebut;

	public FightState fightState;
    [System.Obsolete]
    public CallbackObjs CorpsHurtCallback;
    [System.Obsolete]
    public CallbackObj CorpsBackCallback;

	public int MobID
	{
		get
		{
			return _mobID;
		}
		set
		{
			_mobID = value;
		}
	}

	public int MobHp
	{
		get
		{
			return _mobHp;
		}
		set
		{
			_mobHp = value;
		}
	}

	public EnemyControllerBase Member
	{
		get
		{
			return _enemy;
		}
	}

	public IF_Master Master
	{
		get
		{
			return _master;
		}
		set
		{
			_master = value;
		}
	}

	public void GoBack()
	{
		ReturnHp(_enemy.Hp);
		if ((int)_enemy.Hp > 0)
		{
			fightState = FightState.Rest;
		}
		RemoveFromList(_mobID);
		if ((int)_enemy.Hp > 0)
		{
			_enemy.BackToPool();
			StageObjParam component = _enemy.GetComponent<StageObjParam>();
			if (component != null && component.nEventID != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = component.nEventID;
				component.nEventID = 0;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}
	}

	public void ComeBack(bool explosion = false, bool returnhp = false)
	{
		if (!_enemy)
		{
			return;
		}
		if (returnhp)
		{
			ReturnHp(_enemy.Hp);
		}
		if ((int)_enemy.Hp > 0)
		{
			fightState = FightState.Rest;
		}
		RemoveFromList(_mobID);
		if (explosion && (int)_enemy.Hp <= 0)
		{
			_enemy.PlaySE(_enemy.ExplodeSE[0], _enemy.ExplodeSE[1]);
			_enemy.Explosion();
			if (_enemy.DeadCallback != null)
			{
				_enemy.DeadCallback();
			}
		}
		_enemy.BackToPool();
		StageObjParam component = _enemy.GetComponent<StageObjParam>();
		if (component != null && component.nEventID != 0)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = component.nEventID;
			component.nEventID = 0;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
		}
	}

	public void ReturnHp(int hp)
	{
		if (hp <= 0)
		{
			fightState = FightState.Dead;
		}
		if (CorpsHurtCallback != null)
		{
			CorpsHurtCallback(_mobID, hp);
		}
	}

	public void RemoveFromList(int ID)
	{
		if (CorpsBackCallback != null)
		{
			CorpsBackCallback(ID);
		}
	}

	public void Automatic(bool isauto = true)
	{
		isObedient = !isauto;
	}

	public BossCorpsTool(EnemyControllerBase enemy = null, int mobhp = 0, bool autoreturn = false)
	{
		_enemy = enemy;
		_mobHp = mobhp;
		_autoReturn = autoreturn;
		hasDebut = false;
		_missionStatus.Clear();
	}

    [System.Obsolete]
    public void SetIDAndCB(int mobid, CallbackObjs hurtcallbackObjs, CallbackObj backcallbackObj)
	{
		_mobID = mobid;
		CorpsHurtCallback = hurtcallbackObjs;
		CorpsBackCallback = backcallbackObj;
	}

	public bool SendMission(List<int> missionList, bool addmission = false, bool force = false)
	{
		if (addmission)
		{
			if (missionList != null)
			{
				_missionStatus.AddRange(missionList);
				if (_missionStatus.Count > 0)
				{
					WaitMission = false;
				}
				return true;
			}
			Debug.LogError("MissionList Could not be null.");
			return false;
		}
		if (WaitMission || force)
		{
			_missionStatus = missionList;
			WaitMission = false;
			return true;
		}
		return false;
	}

	public bool SendMission(int mission, bool addmission = false, bool force = false)
	{
		return SendMission(new List<int> { mission }, addmission, force);
	}

	public bool CheckMissionProgress()
	{
		if (WaitMission)
		{
			return _missionStatus.Count <= 0;
		}
		return false;
	}

	public int ReceiveMission()
	{
		if (_missionStatus.Count > 0)
		{
			int result = _missionStatus[0];
			_missionStatus.RemoveAt(0);
			return result;
		}
		return -1;
	}

	public bool MissionComplete()
	{
		if (_missionStatus.Count > 0)
		{
			return false;
		}
		WaitMission = true;
		if (_autoReturn)
		{
			GoBack();
		}
		return true;
	}

	public void SetDebutOver(bool isover = true)
	{
		hasDebut = isover;
	}

	public bool StopMission()
	{
		if (_enemyExecute == null)
		{
			Debug.LogError("指定的對象成員需要有使用 interface : IF_ForceExecute");
			return false;
		}
		return _enemyExecute.SetStopMission();
	}

	public bool ForceStopMission()
	{
		return ForceExecuteMission(-1);
	}

	public bool ForceExecuteMission(int mission)
	{
		if (_enemyExecute == null)
		{
			Debug.LogError("指定的對象成員需要有使用 interface : IF_ForceExecute");
			return false;
		}
		bool flag = _enemyExecute.SetMission(mission);
		if (!flag)
		{
			return flag;
		}
		return _enemyExecute.ForceExecuteMission();
	}

	public void SetCanForceExecute<EM>(EM enemyExecute) where EM : IF_ForceExecute
	{
		_enemyExecute = enemyExecute;
	}
}
