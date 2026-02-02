#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class EventManager : MonoBehaviourSingleton<EventManager>
{
	public enum ID
	{
		NONE = 0,
		SWITCH_SCENE = 1,
		SCENE_INIT = 2,
		UPDATE_RENDER_WEAPON = 3,
		UPDATE_RENDER_CHARACTER = 4,
		SHOW_DAMAGE = 5,
		UPDATE_DOWNLOAD_BAR = 6,
		CAMERA_SHAKE = 7,
		LOCAL_PLAYER_SPWAN = 8,
		LOCAL_PLAYER_DESTROY = 9,
		BATTLE_START = 10,
		LOCK_RANGE = 11,
		STAGE_PLAYER_SPWAN_ED = 12,
		STAGE_PLAYER_DESTROY_ED = 13,
		STAGE_FALLDOWN_PLAYER = 14,
		STAGE_FALLDOWN_ENEMY = 15,
		STAGE_REBORNEVENT = 16,
		STAGE_END_REPORT = 17,
		STAGE_UPDATE_HOST = 18,
		STAGE_TELEPORT = 19,
		STAGE_RESTART = 20,
		BATTLE_INFO_UPDATE = 21,
		STAGE_OBJ_TOUCHCB = 22,
		STAGE_CAMERA_FOCUS = 23,
		STAGE_DELETE_CHECK = 24,
		STAGE_OBJ_CTRL = 25,
		STAGE_EVENT_CALL = 26,
		STAGE_EVENT_WARING = 27,
		STAGE_SHOW_ENEMYINFO = 28,
		STAGE_GENERATE_PVE_PLAYER = 29,
		STAGE_REGISTER_PVP_SPAWNPOS = 30,
		UI_UPDATEMAINSUBWEAPON = 31,
		UI_UPDATESTAGEREWARD = 32,
		STAGE_ALLOK = 33,
		PLAYERBUILD_PLAYER_SPAWN = 34,
		PLAYERBUILD_PLAYER_NETCTRLON = 35,
		UPDATE_TOPBAR_DATA = 36,
		STAGE_PLAYBGM = 37,
		UI_CHARACTERINFO_CHARACTER_CHANGE = 38,
		RT_UPDATE_WEAPON = 39,
		UPDATE_BATTLE_POWER = 40,
		UPDATE_HOMETOP_HINT = 41,
		UPDATE_PLAYER_BOX = 42,
		UPDATE_PLAYER_EQUIPMENT = 43,
		STAGE_SKILL_ATK_TARGET = 44,
		UI_RANKING_CHARACTER_CHANGE = 45,
		UI_PLAYER_INFO_MAIN_WEAPON_CHANGE = 46,
		UI_PLAYER_INFO_SUB_WEAPON_CHANGE = 47,
		UPDATE_SHOP = 48,
		STAGE_UPDATE_PLAYER_LIST = 49,
		UPDATE_LOADING_PROGRESS = 50,
		UPDATE_LOADING_EFT = 51,
		UPDATE_FULL_LOADING_PROGRESS = 52,
		GACHA_SKIP = 53,
		UPDATE_HOMETOP_RENDER = 54,
		STAGE_CONTINUE_PLATER = 55,
		STAGE_BULLET_REGISTER = 56,
		STAGE_BULLET_UNREGISTER = 57,
		STAGE_PLAYER_INFLAG_RANGE = 58,
		WEATHER_SYSTEM_INIT = 59,
		WEATHER_SYSTEM_CTRL = 60,
		UPDATE_SETTING = 61,
		UPDATE_SCENE_PROGRESS = 62,
		REGISTER_STAGE_PARAM = 63,
		LOGIN_FAILED = 64,
		PLAYER_LEVEL_UP = 65,
		BACK_TO_HOMETOP = 66,
		CHARACTER_RT_VISIBLE = 67,
		GACHA_PRIZE_START = 68,
		UI_CHARACTER_POS = 69,
		CHARGE_STAMINA = 70,
		UI_CHARACTERINFO_BONUS_COUNT = 71,
		UPDATE_PLAYER_IDENTIFY = 72,
		CHANGE_DAY = 73,
		UPDATE_MAILBOX = 74,
		LOGIN_CANCEL = 75,
		UI_RESEARCH_COMPLETE_VOICE = 76,
		UPDATE_STAGE_RES_PROGRESS = 77,
		UPDATE_BANNER = 78,
		SD_HOME_BGM = 79,
		SD_BACK_BGM = 80,
		LIBRARY_UPDATE_MAIN_UI = 81,
		CLOSE_FX = 82,
		STAGE_TIMESCALE_CHANGE = 83,
		GAME_PAUSE = 84,
		CHARACTER_RT_SUNSHINE = 85,
		STAGE_OBJ_CTRL_SYNC_HP = 86,
		STAGE_OBJ_CTRL_PET_ACTION = 87,
		STAGE_OBJ_CTRL_ENEMY_ACTION = 88,
		STAGE_OBJ_CTRL_PLAYSHOWANI = 89,
		CHARACTER_RT_DIALOG = 90,
		CAMERA_EFFECT_CTRL = 91,
		PATCH_CHANGE = 92,
		RT_UPDATE_CAMERA_FOV = 93,
		TOGGLE_GUILD_SCENE_RENDER = 94,
		UPDATE_HOMETOP_CANVAS = 95,
		UPDATE_GUILD_HINT = 96,
		UPDATE_RESOLUTION = 97,
		UPDATE_FULLSCREEN = 98,
		SOCKET_NOTIFY_NEW_CHATMESSAGE = 99,
		GUILD_ID_CHANGED = 100,
		STAGE_OBJ_CTRL_BULLET_ACTION = 101,
		ENTER_OR_LEAVE_RIDE_ARMOR = 102,
		REMOVE_DEAD_AREA_EVENT = 103,
		LOCK_WALL_JUMP = 104,
		STAGE_PLAYBGM_BEAT_SYNC = 105
	}

	public class RegisterStageParam
	{
		public int nMode;

		public int nStageSecert;
	}

	public class LockRangeParam
	{
		public int nMode;

		public float fMinX;

		public float fMaxX;

		public float fMinY;

		public float fMaxY;

		public int? nNoBack;

		public float? fSpeed;

		public bool? bSetFocus;

		public Vector3? vDir;

		public float? fOY;

		public bool bSlowWhenMove;
	}

	public class StageSkillAtkTargetParam
	{
		public Transform tTrans;

		public int nSkillID;

		public bool bAtkNoCast;

		public Vector3 tPos;

		public Vector3 tDir;

		public bool bBuff;

		public LayerMask tLM = BulletScriptableObject.Instance.BulletLayerMaskPlayer;
	}

	public class StageGeneratePlayer
	{
		public int nMode;

		public int nID;

		public int nSkinID;

		public string sPlayerID = "";

		public Vector3 vPos = Vector3.zero;

		public int nHP;

		public bool bLookDir;

		public int nCharacterID;

		public int WeaponCurrent;

		public int nMeasureNow;

		public bool bUsePassiveskill;

		public int HealHp;

		public int DmgHp;
	}

	public class StageCameraFocus
	{
		public bool bLock;

		public bool bRightNow;

		public bool bUnRange;

		public int nMode;

		public Vector3 roominpos;

		public float fRoomInTime;

		public float fRoomOutTime;

		public float fRoomInFov;

		public bool bDontPlayMotion;

		public bool bCallStageEnd = true;
	}

	public class StageEventCall
	{
		public int nID;

		public STAGE_EVENT nStageEvent;

		public Transform tTransform;
	}

	public class RemoveDeadAreaEvent
	{
		public OrangeCharacter tOC;
	}

	public class BattleInfoUpdate
	{
		public int nType;
	}

    [Obsolete]
    private System.Collections.Generic.Dictionary<ID, List<CallbackObjs>> dictEvents = new Better.Dictionary<ID, List<CallbackObjs>>();

	[Obsolete("This API is expected to be removed and is not recommended.")]
	public void AttachEvent(ID p_eventId, CallbackObjs p_cb)
	{
		Debug.Log("AttachEvent:" + p_eventId);
		List<CallbackObjs> value;
		if (!dictEvents.TryGetValue(p_eventId, out value))
		{
			value = new List<CallbackObjs>();
			dictEvents.Add(p_eventId, value);
		}
		value.Add(p_cb);
	}

	[Obsolete("This API is expected to be removed and is not recommended.")]
	public void DetachEvent(ID p_eventId, CallbackObjs p_cb)
	{
		Debug.Log("DetachEvent:" + p_eventId);
		List<CallbackObjs> value;
		if (dictEvents.TryGetValue(p_eventId, out value))
		{
			CallbackObjs callbackObjs = value.Find((CallbackObjs x) => x == p_cb);
			if (callbackObjs != null)
			{
				value.Remove(callbackObjs);
				callbackObjs = null;
			}
		}
		else
		{
			Debug.LogWarning(string.Concat("DetachEvent:", p_eventId, "Failed, No Event Attached"));
		}
	}

	[Obsolete("This API is expected to be removed and is not recommended.")]
	public void NotifyEvent(ID p_eventId, params object[] p_param)
	{
		Debug.Log("NotifyEvent:" + p_eventId);
		List<CallbackObjs> value;
		if (dictEvents.TryGetValue(p_eventId, out value))
		{
			foreach (CallbackObjs item in value.ToList())
			{
				item.CheckTargetToInvoke(p_param);
			}
			return;
		}
		Debug.LogWarning(string.Concat("NotifyEvent:", p_eventId, "Failed, No Event Attached"));
	}

    [Obsolete]
    public void DetachAllEvent()
	{
		foreach (List<CallbackObjs> value in dictEvents.Values)
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i] = null;
			}
			value.Clear();
		}
		dictEvents.Clear();
	}

	protected override void OnDestroy()
	{
		DetachAllEvent();
		base.OnDestroy();
	}

	private void OnDisable()
	{
		DetachAllEvent();
	}
}
