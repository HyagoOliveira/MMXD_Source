#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CriWare;
using StageLib;
using UnityEngine;

[Serializable]
public class OrangeCriSource : MonoBehaviour
{
	[SerializeField]
	public bool IsVisiable = true;

	[SerializeField]
	public OrangeSSType SourceType;

	[SerializeField]
	public float MaxDistance = 12f;

	[SerializeField]
	public bool StopWhenOutside;

	[SerializeField]
	public bool UseRenderObj;

	[SerializeField]
	public GameObject renderObj;

	[SerializeField]
	public GameObject HookObj;

	[SerializeField]
	public Dictionary<string, OrangeCriPoint> loopSEs = new Dictionary<string, OrangeCriPoint>();

	[SerializeField]
	public List<string> seDic = new List<string>();

	[SerializeField]
	private Renderer renderer;

	public float _currentDis;

	public float f_vol = 1f;

	private float _lastDis;

	private float _vector;

	private bool _pause;

	private CueInfo mCueInfo = new CueInfo();

	private CriAtomExPlayback nullPB;

	private const float fViewHalfDis = 10f;

	public bool InView
	{
		get
		{
			return _currentDis < 10f;
		}
	}

	public bool IsPause
	{
		get
		{
			return _pause;
		}
	}

	private void Awake()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.RegisterSource(this);
	}

	private void Start()
	{
		InitRenderer();
	}

	public void InitRenderer()
	{
		if (!UseRenderObj)
		{
			renderObj = null;
		}
		else if (renderObj == null)
		{
			SkinnedMeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			if (componentsInChildren.Length != 0)
			{
				renderer = componentsInChildren[0];
				return;
			}
			MeshRenderer[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren2.Length != 0)
			{
				renderer = componentsInChildren2[0];
				return;
			}
			LineRenderer[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<LineRenderer>();
			if (componentsInChildren3.Length != 0)
			{
				renderer = componentsInChildren3[0];
			}
			else
			{
				renderer = OrangeGameUtility.AddOrGetRenderer<Renderer>(base.gameObject);
			}
		}
		else
		{
			renderer = renderObj.GetComponent<Renderer>();
			if (renderer == null)
			{
				renderer = renderObj.GetComponent<MeshRenderer>();
			}
		}
	}

	private void Update()
	{
		if (loopSEs.Count > 0 && !(loopSEs.Values.ToList()[0].sourceObj == this))
		{
			Debug.Log("SourceObj LOST!!");
		}
		UpdateDistanceCall();
	}

	public static float CalcHalfVolum(float dis, float maxdis, float halfDis = 0f)
	{
		float num = 10f;
		if (halfDis != 0f)
		{
			num = halfDis;
		}
		float num2 = num - dis;
		if (num2 >= 0f)
		{
			return 1f - dis / num * 0.2f;
		}
		num2 = Mathf.Abs(num2) / (maxdis - num);
		if (num2 >= 1f)
		{
			return 0f;
		}
		return (1f - num2) * 0.8f;
	}

	public static float CalcVolume2MainOC(float maxdis, Transform trans)
	{
		float num = 0f;
		try
		{
			if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera != null)
			{
				Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
				num = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(trans.position.x, trans.position.y));
			}
			else
			{
				num = 0f;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("OrangeCriSource CalcVolume2MainOC : OrangeSceneManager instance error!/n" + ex.Message);
			num = 0f;
		}
		return CalcHalfVolum(num, maxdis);
	}

	public static float GetCameraDist(Transform tc)
	{
		float result = 0f;
		try
		{
			if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera != null)
			{
				Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
				result = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(tc.position.x, tc.position.y));
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("OrangeCriSource : OrangeSceneManager instance error!/n" + ex.Message);
		}
		return result;
	}

	public static float GetPlayerDist(Transform tc)
	{
		float result = 0f;
		Transform mainPlayerTrans = StageUpdate.GetMainPlayerTrans();
		if (mainPlayerTrans != null)
		{
			result = Vector2.Distance(new Vector2(mainPlayerTrans.position.x, mainPlayerTrans.position.y), new Vector2(tc.position.x, tc.position.y));
		}
		return result;
	}

	private float GetMainOCDistance()
	{
		Transform tc = base.transform;
		if (HookObj != null)
		{
			tc = HookObj.transform;
		}
		if (SourceType == OrangeSSType.PLAYER)
		{
			_currentDis = GetPlayerDist(tc);
		}
		else
		{
			_currentDis = GetCameraDist(tc);
		}
		_vector = _currentDis - _lastDis;
		return _currentDis;
	}

	public void UpdateDistanceCall()
	{
		float mainOCDistance = GetMainOCDistance();
		_vector = mainOCDistance - _lastDis;
		float num = MaxDistance;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear && _currentDis <= 28f)
		{
			num = 28f;
			f_vol = CalcHalfVolum(_currentDis, num, num - 8f);
		}
		else
		{
			f_vol = CalcHalfVolum(_currentDis, num);
		}
		foreach (StageUpdate.EnemyCtrlID runEnemy in StageUpdate.runEnemys)
		{
			bool flag = false;
			int n_TYPE = runEnemy.mEnemy.EnemyData.n_TYPE;
			if ((uint)(n_TYPE - 1) <= 1u || n_TYPE == 5)
			{
				flag = true;
				if (base.name == "fxstory_explode_000")
				{
					f_vol = (float)OrangeBattleUtility.Random(80, 100) / 100f;
				}
				else if (base.name == "event_bs089")
				{
					f_vol = (num - _currentDis) / num;
					f_vol = ((f_vol < 0f) ? 0f : f_vol);
					f_vol = (float)Math.Sqrt(f_vol);
					f_vol = ((f_vol > 1f) ? 1f : f_vol);
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (_currentDis <= num && _lastDis > num)
		{
			OnVisiableCall();
		}
		if (_currentDis > num && _lastDis <= num)
		{
			OnInvisiableCall();
		}
		_lastDis = _currentDis;
	}

	private void OnEnable()
	{
		UpdateDistanceCall();
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
		StopAll();
		MonoBehaviourSingleton<AudioManager>.Instance.UnRegisterSource(this);
	}

	public void Initial(OrangeSSType st)
	{
		SourceType = st;
		switch (SourceType)
		{
		case OrangeSSType.HIT:
		case OrangeSSType.ENEMY:
		case OrangeSSType.PET:
		case OrangeSSType.EFFECT:
			_currentDis = (_lastDis = (MaxDistance = 13f));
			break;
		case OrangeSSType.BOSS:
		case OrangeSSType.PVE1:
		case OrangeSSType.PVE2:
		case OrangeSSType.PVE3:
		case OrangeSSType.PVP1:
		case OrangeSSType.PVP2:
		case OrangeSSType.PVP3:
			_currentDis = (_lastDis = (MaxDistance = 16f));
			break;
		case OrangeSSType.PLAYER:
		case OrangeSSType.SYSTEM:
			_currentDis = (_lastDis = 0f);
			MaxDistance = 16f;
			break;
		case OrangeSSType.MAPOBJS:
		case OrangeSSType.STAGEOBJS:
			_currentDis = (_lastDis = (MaxDistance = 13f));
			break;
		}
		UpdateDistanceCall();
	}

	public bool InCameraView(Renderer objRender)
	{
		if ((bool)MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera)
		{
			return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera), objRender.bounds);
		}
		return false;
	}

	public CriAtomExPlayback PlaySE(string s_acb, int cueid, float delay = 0f)
	{
		CriAtomExAcb acb = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(s_acb, "NULL");
		if (acb == null)
		{
			return nullPB;
		}
		CriAtomEx.CueInfo info = default(CriAtomEx.CueInfo);
		if (acb.GetCueInfo(cueid, out info))
		{
			return PlaySE(s_acb, info.name, delay);
		}
		return nullPB;
	}

	private IEnumerator DelayPlay(string s_acb, string cuename, float delay)
	{
		yield return new WaitForSeconds(delay);
		CheckAndPlay(s_acb, cuename);
	}

	private IEnumerator DelayForcePlay(string s_acb, string cuename, float delay)
	{
		yield return new WaitForSeconds(delay);
		IsVisiable = true;
		CheckAndPlay(s_acb, cuename, true);
	}

	public CriAtomExPlayback PlaySE(string s_acb, string cuename, float delay = 0f)
	{
		if (delay > 0f)
		{
			StartCoroutine(DelayPlay(s_acb, cuename, delay));
			return nullPB;
		}
		return CheckAndPlay(s_acb, cuename);
	}

	public CriAtomExPlayback ForcePlaySE(string s_acb, string cuename, float delay = 0f)
	{
		if (delay > 0f)
		{
			StartCoroutine(DelayForcePlay(s_acb, cuename, delay));
			return nullPB;
		}
		IsVisiable = true;
		f_vol = 1f;
		_currentDis = 0f;
		return CheckAndPlay(s_acb, cuename, true);
	}

	public CriAtomExPlayback ForcePlaySE(string s_acb, int cueid, float delay = 0f)
	{
		IsVisiable = true;
		f_vol = 1f;
		_currentDis = 0f;
		return PlaySE(s_acb, cueid, delay);
	}

	public void ActivePlaySE(string s_acb, string cuename)
	{
		OrangeCriPoint orangeCriPoint = null;
		CriAtomExAcb acb = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(s_acb, cuename);
		if (acb == null)
		{
			return;
		}
		mCueInfo.Parse(s_acb, cuename);
		if (mCueInfo.eType == CueType.CT_LOOP)
		{
			if (seDic.FindAll((string a) => a.Contains(mCueInfo.sFullKey)).Count == 0)
			{
				orangeCriPoint = MonoBehaviourSingleton<AudioManager>.Instance.RegLoop(this, mCueInfo);
				if (orangeCriPoint != null)
				{
					orangeCriPoint.CueInfo = mCueInfo;
					orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.SE_LOOP;
					orangeCriPoint.sourceObj = this;
					orangeCriPoint.Player.SetCue(acb, cuename);
					f_vol = 0f;
					orangeCriPoint.Play(s_acb, cuename);
					seDic.Add(mCueInfo.sFullKey);
				}
				else
				{
					seDic.Add(mCueInfo.sFullKey);
				}
			}
		}
		else if (!(_currentDis > MaxDistance) && orangeCriPoint != null)
		{
			orangeCriPoint.CueInfo = mCueInfo;
			orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.NONE;
			orangeCriPoint.sourceObj = this;
			orangeCriPoint.Player.SetCue(acb, cuename);
			orangeCriPoint.Play(s_acb, cuename);
		}
	}

	private CriAtomExPlayback CheckAndPlay(string s_acb, string cuename, bool forceplay = false)
	{
		OrangeCriPoint orangeCriPoint = null;
		CriAtomExAcb acb = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(s_acb, cuename);
		if (acb == null)
		{
			return nullPB;
		}
		mCueInfo.Parse(s_acb, cuename);
		if (mCueInfo.eType == CueType.CT_LOOP)
		{
			if (seDic.FindAll((string a) => a.Contains(mCueInfo.sFullKey)).Count != 0)
			{
				return nullPB;
			}
			orangeCriPoint = MonoBehaviourSingleton<AudioManager>.Instance.RegLoop(this, mCueInfo);
			if (orangeCriPoint != null)
			{
				orangeCriPoint.CueInfo = mCueInfo;
				orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.SE_LOOP;
				orangeCriPoint.sourceObj = this;
				orangeCriPoint.Player.SetCue(acb, cuename);
				if (!MonoBehaviourSingleton<UpdateManager>.Instance.Pause)
				{
					orangeCriPoint.Play(s_acb, cuename);
				}
				seDic.Add(mCueInfo.sFullKey);
				return orangeCriPoint.playerpb;
			}
			seDic.Add(mCueInfo.sFullKey);
			return nullPB;
		}
		if (mCueInfo.eType == CueType.CT_STOP)
		{
			int num = -1;
			string sKey = mCueInfo.sHalfKey;
			orangeCriPoint = null;
			num = seDic.FindIndex((string a) => a.StartsWith(sKey));
			if (num == -1)
			{
				return nullPB;
			}
			seDic.RemoveAt(num);
			if (MonoBehaviourSingleton<AudioManager>.Instance.UnRegLoop(this, mCueInfo))
			{
				orangeCriPoint = MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint();
				orangeCriPoint.CueInfo = mCueInfo;
				orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.NONE;
				orangeCriPoint.sourceObj = this;
				orangeCriPoint.Player.SetCue(acb, cuename);
				if (!MonoBehaviourSingleton<UpdateManager>.Instance.Pause)
				{
					orangeCriPoint.Play(s_acb, cuename);
				}
				return orangeCriPoint.playerpb;
			}
		}
		else if (f_vol > 0f || forceplay)
		{
			orangeCriPoint = MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint();
			if (orangeCriPoint == null)
			{
				return nullPB;
			}
			orangeCriPoint.CueInfo = mCueInfo;
			orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.NONE;
			orangeCriPoint.sourceObj = this;
			orangeCriPoint.Player.SetCue(acb, cuename);
			if (!MonoBehaviourSingleton<UpdateManager>.Instance.Pause)
			{
				orangeCriPoint.Play(s_acb, cuename);
			}
			return orangeCriPoint.playerpb;
		}
		return nullPB;
	}

	public void StopLoop(string s_acb, string cuename)
	{
		if (cuename.EndsWith("_lp"))
		{
			string cuename2 = cuename.Substring(0, cuename.Length - 3) + "_stop";
			CheckAndPlay(s_acb, cuename2);
		}
		else
		{
			CheckAndPlay(s_acb, cuename);
		}
	}

	public void StopAll()
	{
		foreach (string item in seDic)
		{
			string[] array = item.Split(',');
			mCueInfo.Parse(array[0], array[1]);
			MonoBehaviourSingleton<AudioManager>.Instance.UnRegLoop(this, mCueInfo);
		}
		seDic.Clear();
		foreach (KeyValuePair<string, OrangeCriPoint> loopSE in loopSEs)
		{
			loopSE.Key.Split(',');
			loopSE.Value.Reset();
		}
		loopSEs.Clear();
	}

	public void Clear()
	{
		StopAll();
	}

	public void OnVisiableCall()
	{
		IsVisiable = true;
		MonoBehaviourSingleton<AudioManager>.Instance.ReqPlay(this);
		IsVisiable = true;
	}

	public void OnInvisiableCall()
	{
		IsVisiable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.ReqStop(this);
	}

	public void Pause(bool pause)
	{
		if (IsVisiable)
		{
			foreach (string item in seDic)
			{
				string[] array = item.Split(',');
				mCueInfo.Parse(array[0], array[1]);
				MonoBehaviourSingleton<AudioManager>.Instance.PauseLoop(mCueInfo, pause);
			}
		}
		_pause = pause;
	}

	public void AddLoopSE(string acb, string cue, float looptiem, float delay = 0f)
	{
		mCueInfo.Parse(acb, cue);
		if (loopSEs.ContainsKey(mCueInfo.sFullKey))
		{
			return;
		}
		CriAtomExAcb acb2 = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(acb, cue);
		if (acb2 == null)
		{
			return;
		}
		OrangeCriPoint aPoint = MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint();
		if (aPoint != null)
		{
			aPoint.CueInfo = mCueInfo;
			aPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.PG_LOOP;
			aPoint.sourceObj = this;
			aPoint.Timer = OrangeTimerManager.GetTimer();
			aPoint.LoopTime = (long)(looptiem * 1000f);
			aPoint.DelayTime = (long)(delay * 1000f);
			aPoint.Player.SetCue(acb2, cue);
			aPoint.m_acb = acb;
			aPoint.m_cue = cue;
			aPoint.Timer.TimerStart();
			if (delay == 0f)
			{
				aPoint.Play(acb, cue);
			}
			loopSEs.Add(mCueInfo.sFullKey, aPoint);
		}
	}

	public void RemoveLoopSE(string acb, string cue)
	{
		string key = acb + "," + cue;
		OrangeCriPoint value;
		if (loopSEs.TryGetValue(key, out value))
		{
			value.Reset();
			loopSEs.Remove(key);
		}
	}
}
