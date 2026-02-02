using System;
using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class GuildMainSceneNPCController : MonoBehaviour
{
	public enum FxName
	{
		fx_guild_teleport_in = 0,
		fx_guild_teleport_out = 1
	}

	public float DeployStartDelay = 5f;

	public int DeployCheckIntervalMin = 3;

	public int DeployCheckIntervalMax = 10;

	public float TeleportInDelay = 2f;

	public float TeleportOutDelay = 2f;

	[SerializeField]
	private PathCreator[] _pathCreators;

	[SerializeField]
	private GuildMainSceneNPCHelper[] _npcHelpers;

	private List<PathCreator> _pathCreatorEnabled;

	private List<PathCreator> _pathCreatorDisabled;

	private List<GuildMainSceneNPCHelper> _npcHelperEnabled;

	private List<GuildMainSceneNPCHelper> _npcHelperDisabled;

	private Coroutine _deployCoroutine;

	private bool _isPaused;

	public bool IsPaused
	{
		get
		{
			return _isPaused;
		}
		set
		{
			_isPaused = value;
			GuildMainSceneNPCHelper[] npcHelpers = _npcHelpers;
			for (int i = 0; i < npcHelpers.Length; i++)
			{
				npcHelpers[i].IsPaused = value;
			}
		}
	}

	public void Start()
	{
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		GuildMainSceneNPCHelper[] npcHelpers = _npcHelpers;
		foreach (GuildMainSceneNPCHelper obj in npcHelpers)
		{
			obj.OnReachPathEndPointEvent += OnReachPathEndPoint;
			obj.enabled = false;
			obj.gameObject.SetActive(false);
		}
		_pathCreatorEnabled = new List<PathCreator>();
		_pathCreatorDisabled = new List<PathCreator>(_pathCreators);
		_npcHelperEnabled = new List<GuildMainSceneNPCHelper>();
		_npcHelperDisabled = new List<GuildMainSceneNPCHelper>(_npcHelpers);
		_deployCoroutine = StartCoroutine(DeployNPCCoroutine());
	}

	public void OnDestroy()
	{
		if (_deployCoroutine != null)
		{
			StopCoroutine(_deployCoroutine);
		}
		GuildMainSceneNPCHelper[] npcHelpers = _npcHelpers;
		foreach (GuildMainSceneNPCHelper obj in npcHelpers)
		{
			obj.OnReachPathEndPointEvent -= OnReachPathEndPoint;
			obj.enabled = false;
			obj.gameObject.SetActive(false);
		}
		_pathCreatorEnabled.Clear();
		_pathCreatorDisabled.Clear();
		_npcHelperEnabled.Clear();
		_npcHelperDisabled.Clear();
	}

	private void OnReachPathEndPoint(PathCreator pathCreator, GuildMainSceneNPCHelper pathFollower)
	{
		SetTeleportOut(pathCreator, pathFollower);
	}

	private void SetTeleportIn(PathCreator pathCreator, GuildMainSceneNPCHelper pathFollower)
	{
		_pathCreatorEnabled.Add(pathCreator);
		_pathCreatorDisabled.Remove(pathCreator);
		_npcHelperEnabled.Add(pathFollower);
		_npcHelperDisabled.Remove(pathFollower);
		pathFollower.gameObject.SetActive(true);
		pathFollower.enabled = true;
	}

	private void SetTeleportOut(PathCreator pathCreator, GuildMainSceneNPCHelper pathFollower)
	{
		pathFollower.enabled = false;
		pathFollower.gameObject.SetActive(false);
		_pathCreatorEnabled.Remove(pathCreator);
		_pathCreatorDisabled.Add(pathCreator);
		_npcHelperEnabled.Remove(pathFollower);
		_npcHelperDisabled.Add(pathFollower);
	}

	private IEnumerator DeployNPCCoroutine()
	{
		yield return new WaitForSeconds(DeployStartDelay);
		while (true)
		{
			if (_pathCreatorDisabled.Count > 0 && _npcHelperDisabled.Count > 0)
			{
				int index = OrangeBattleUtility.Random(0, _pathCreatorDisabled.Count);
				PathCreator pathCreator = _pathCreatorDisabled[index];
				int index2 = OrangeBattleUtility.Random(0, _npcHelperDisabled.Count);
				GuildMainSceneNPCHelper guildMainSceneNPCHelper = _npcHelperDisabled[index2];
				int num = OrangeBattleUtility.Random(0, 10000);
				guildMainSceneNPCHelper.SetPathCreator(pathCreator, TeleportInDelay, TeleportOutDelay, (num < 5000) ? true : false);
				SetTeleportIn(pathCreator, guildMainSceneNPCHelper);
			}
			while (IsPaused)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return new WaitForSeconds(OrangeBattleUtility.Random(DeployCheckIntervalMin, DeployCheckIntervalMax));
		}
	}
}
