#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using DragonBones;
using UnityEngine;
using UnityEngine.UI;

public class TurtorialUI : OrangeUIBase
{
	protected static TurtorialUI _instance;

	protected Better.Dictionary<string, TutorialLockUIHelper> LockUIHelperDict = new Better.Dictionary<string, TutorialLockUIHelper>();

	private static readonly string nullStr = "null";

	public GameObject LockInput;

	public Button LockInputBtn;

	public GameObject LockImg;

	public GameObject LoadingLock;

	public GameObject UnLockBtn;

	public Text HintMsgText;

	public GameObject DownArrowImg;

	public GameObject UpArrowImg;

	private bool bLockWait;

	private bool bLockCheck;

	private bool bIsLockBtn;

	private bool bUseUnLockTime;

	private float fOpenUIErrorUnLockTime;

	private Button[] lockBtns;

	private IconBase[] lockIconBases;

	private Toggle[] locktoggles;

	private bool bCloseTuto;

	private Coroutine MoveHintMsgCoroutine;

	private Text InstanceHintMsgText;

	private Coroutine tFollowCoroutine;

	private Coroutine tRunTurtorialCoroutine;

	private Coroutine tCheckTurtorialCoroutine;

	private float UIPosToRealPos = 5f / 96f;

	private float UIPosToRealPosHalf = 5f / 192f;

	private Vector2 CameraSize = Vector2.zero;

	public static List<int> requiredTutorialItemID = new List<int>();

	public static event Action OnTutorialFinishedEvent;

	protected override void Awake()
	{
		base.Awake();
		LockInputBtn.onClick.AddListener(ButtonPressCB);
		LockInput.gameObject.SetActive(false);
		LockImg.SetActive(false);
		LoadingLock.SetActive(false);
		UnLockBtn.SetActive(false);
		HintMsgText.transform.parent.gameObject.SetActive(false);
		DownArrowImg.SetActive(false);
		UpArrowImg.SetActive(false);
		InstanceHintMsgText = UnityEngine.Object.Instantiate(HintMsgText.transform.parent.gameObject, HintMsgText.transform.parent.parent).transform.Find("HintMsgText").GetComponent<Text>();
		InstanceHintMsgText.transform.parent.gameObject.SetActive(false);
		Canvas component = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent.parent.GetComponent<Canvas>();
		UIPosToRealPos = component.transform.localScale.x;
		UIPosToRealPosHalf = UIPosToRealPos * 0.5f;
		CameraSize = ((RectTransform)component.transform).sizeDelta;
	}

	public static void CheckTurtorialStageEnd(int nStageID, Callback cb)
	{
		if (_instance != null)
		{
			_instance.StartCoroutine(_instance.WaitLockAndCheckStageEnd(nStageID, cb));
			return;
		}
		string checkname = "stageid" + nStageID;
		IEnumerable<KeyValuePair<int, TUTORIAL_TABLE>> source = ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.Where((KeyValuePair<int, TUTORIAL_TABLE> obj) => obj.Value.s_TRIGGER == checkname);
		if (source.Count() == 1)
		{
			KeyValuePair<int, TUTORIAL_TABLE> keyValuePair = source.ElementAt(0);
			ManagedSingleton<PlayerNetManager>.Instance.TurtorialFlagRq(keyValuePair.Value.n_SAVE, cb);
		}
		else
		{
			cb();
		}
	}

	private IEnumerator WaitLockAndCheckStageEnd(int nStageID, Callback cb)
	{
		float fTimeOut = 10f;
		while (bLockCheck)
		{
			Debug.LogWarning("WaitLockAndCheckStageEnd bLockCheck " + fTimeOut);
			yield return CoroutineDefine._waitForEndOfFrame;
			fTimeOut -= Time.deltaTime;
			if (fTimeOut <= 0f)
			{
				break;
			}
		}
		string checkname = "stageid" + nStageID;
		IEnumerable<KeyValuePair<int, TUTORIAL_TABLE>> source = ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.Where((KeyValuePair<int, TUTORIAL_TABLE> obj) => obj.Value.s_TRIGGER == checkname);
		if (source.Count() == 1)
		{
			KeyValuePair<int, TUTORIAL_TABLE> keyValuePair = source.ElementAt(0);
			Debug.LogWarning("WaitLockAndCheckStageEnd TurtorialFlagRq " + keyValuePair.Value.n_SAVE);
			ManagedSingleton<PlayerNetManager>.Instance.TurtorialFlagRq(keyValuePair.Value.n_SAVE, cb);
		}
		else
		{
			cb();
		}
	}

	public static void CheckTurtorialLastUI(Action cb = null, bool bHasBackRound = false)
	{
		UnityEngine.Transform uI_Parent = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent;
		int childCount = uI_Parent.childCount;
		int num = 2;
		if (childCount >= num)
		{
			UnityEngine.Transform child = uI_Parent.GetChild(childCount - num);
			if (child.GetComponent<OrangeBgExt>() != null)
			{
				child = uI_Parent.GetChild(childCount - num - 1);
			}
			CheckTurtorialTriggerName(child.gameObject.name, cb);
		}
	}

	public static void LoadTurtorialUIByAB(Action cb = null)
	{
		if (_instance != null)
		{
			if (cb != null)
			{
				cb();
			}
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "UI_Turtorial", "UI_Turtorial", delegate(GameObject tui)
		{
			if (tui == null)
			{
				if (cb != null)
				{
					cb();
				}
			}
			else
			{
				UnityEngine.Transform canvasUI = MonoBehaviourSingleton<UIManager>.Instance.CanvasUI;
				if (canvasUI != null)
				{
					if (_instance == null)
					{
						GameObject obj = UnityEngine.Object.Instantiate(tui, canvasUI, false);
						obj.GetComponent<CanvasGroup>().alpha = 1f;
						_instance = obj.GetComponent<TurtorialUI>();
						_instance.transform.SetSiblingIndex(canvasUI.Find("ConfirmUIParent").GetSiblingIndex() - 1);
					}
					if (cb != null)
					{
						cb();
					}
				}
				else if (cb != null)
				{
					cb();
				}
			}
		});
	}

	public static void CheckTurtorialTriggerName(string sTriggerName, Action cb = null)
	{
		if (sTriggerName == "UI_Hometop" && _instance != null)
		{
			int trutorialNonLinear = ManagedSingleton<PlayerHelper>.Instance.GetTrutorialNonLinear(sTriggerName);
			if (!requiredTutorialItemID.Contains(trutorialNonLinear))
			{
				TUTORIAL_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(trutorialNonLinear, out value) && value.n_ITEMID != 0 && value.n_ITEMCOUNT != 0)
				{
					_instance.LoadingLock.SetActive(true);
					requiredTutorialItemID.Add(trutorialNonLinear);
					ManagedSingleton<PlayerNetManager>.Instance.RetrieveTutorialItemReq(trutorialNonLinear, delegate
					{
						CheckTurtorialTriggerName(sTriggerName, cb);
					});
					return;
				}
			}
		}
		if (_instance == null)
		{
			if (sTriggerName != "UI_Hometop")
			{
				return;
			}
			LoadTurtorialUIByAB(delegate
			{
				if (_instance == null)
				{
					if (cb != null)
					{
						cb();
					}
				}
				else
				{
					TUTORIAL_TABLE value3 = null;
					int currentTurtorialID2 = ManagedSingleton<PlayerHelper>.Instance.GetCurrentTurtorialID();
					ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(currentTurtorialID2, out value3);
					if (value3 != null && GetTriggerName(value3.s_TRIGGER) == sTriggerName)
					{
						_instance.CheckTurtorial(currentTurtorialID2, cb, sTriggerName);
					}
					else
					{
						currentTurtorialID2 = ManagedSingleton<PlayerHelper>.Instance.GetTrutorialNonLinear(sTriggerName);
						value3 = null;
						ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(currentTurtorialID2, out value3);
						if (value3 != null && (GetTriggerName(value3.s_TRIGGER) == sTriggerName || _instance.CheckHomeTop(value3.s_TRIGGER)))
						{
							_instance.CheckTurtorial(currentTurtorialID2, cb, sTriggerName);
						}
						else if (!_instance.bLockCheck)
						{
							_instance.LoadingLock.SetActive(false);
						}
					}
				}
			});
			return;
		}
		TUTORIAL_TABLE value2 = null;
		int currentTurtorialID = ManagedSingleton<PlayerHelper>.Instance.GetCurrentTurtorialID();
		ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(currentTurtorialID, out value2);
		if (value2 != null && GetTriggerName(value2.s_TRIGGER) == sTriggerName)
		{
			_instance.CheckTurtorial(currentTurtorialID, cb, sTriggerName);
			return;
		}
		currentTurtorialID = ManagedSingleton<PlayerHelper>.Instance.GetTrutorialNonLinear(sTriggerName);
		value2 = null;
		ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(currentTurtorialID, out value2);
		if (value2 != null && (GetTriggerName(value2.s_TRIGGER) == sTriggerName || _instance.CheckHomeTop(value2.s_TRIGGER)))
		{
			_instance.CheckTurtorial(currentTurtorialID, cb, sTriggerName);
		}
		else if (!_instance.bLockCheck)
		{
			_instance.LoadingLock.SetActive(false);
		}
	}

	private bool CheckHomeTop(string s_TRIGGER)
	{
		if (GetTriggerName(s_TRIGGER) == "UI_Hometop")
		{
			return true;
		}
		return false;
	}

	public static string GetTriggerName(string s_TRIGGER)
	{
		if (s_TRIGGER.Length > 0 && s_TRIGGER[0] == '!')
		{
			string[] array = s_TRIGGER.Substring(1).Split('/');
			if (array.Length > 1)
			{
				return array[1];
			}
			return nullStr;
		}
		return s_TRIGGER;
	}

	private string GetSkipName(string s_TRIGGER)
	{
		if (s_TRIGGER.Length > 0 && s_TRIGGER[0] == '!')
		{
			return s_TRIGGER.Substring(1).Split('/')[0];
		}
		return nullStr;
	}

	public static void CheckTurtorialID(int nTurtorialID, Action cb = null)
	{
		if (_instance == null)
		{
			LoadTurtorialUIByAB(delegate
			{
				if (_instance == null)
				{
					if (cb != null)
					{
						cb();
					}
				}
				else
				{
					_instance.CheckTurtorial(nTurtorialID, cb);
				}
			});
		}
		else
		{
			_instance.CheckTurtorial(nTurtorialID, cb);
		}
	}

	public static void CheckStageTurtorialID(int nScenarioID, UnityEngine.Transform tTrans, bool bLock, Action cb = null)
	{
		if (_instance == null)
		{
			LoadTurtorialUIByAB(delegate
			{
				if (_instance == null)
				{
					if (cb != null)
					{
						cb();
					}
				}
				else
				{
					_instance.RunStageHintMsg(nScenarioID, tTrans, bLock, cb);
				}
			});
		}
		else
		{
			_instance.RunStageHintMsg(nScenarioID, tTrans, bLock, cb);
		}
	}

	private void RunStageHintMsg(int nScenarioID, UnityEngine.Transform tTrans, bool bLock, Action cb = null)
	{
		if (bLockCheck)
		{
			if (cb != null)
			{
				cb();
			}
			return;
		}
		bLockCheck = true;
		SCENARIO_TABLE value;
		if (nScenarioID != 0 && ManagedSingleton<OrangeDataManager>.Instance.SCENARIO_TABLE_DICT.TryGetValue(nScenarioID, out value))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_instance.HintMsgText.transform.parent.gameObject, tTrans);
			gameObject.SetActive(true);
			Text component = gameObject.transform.Find("HintMsgText").GetComponent<Text>();
			component.text = ManagedSingleton<OrangeTextDataManager>.Instance.SCENARIOTEXT_TABLE_DICT.GetL10nValue(value.w_CONTENT);
			component.transform.parent.localScale = new Vector3(0f, 1f, 1f);
			gameObject.transform.localPosition = new Vector3(0f, 200f, 0f);
			component.StartCoroutine(StageHintMsgTextCoroutine(gameObject.transform));
			if (bLock)
			{
				LockInput.gameObject.SetActive(true);
				LockInput.transform.localScale = new Vector3(2f, 2f, 1f);
				LockInput.transform.localPosition = tTrans.localPosition;
				LockInputBtn.enabled = true;
				LockInputBtn.targetGraphic.raycastTarget = true;
				LockInputBtn.onClick.RemoveAllListeners();
				LockInputBtn.onClick.AddListener(delegate
				{
					if (cb != null)
					{
						cb();
					}
					LockInputBtn.onClick.RemoveAllListeners();
					LockInputBtn.onClick.AddListener(ButtonPressCB);
					LockInputBtn.enabled = false;
					LockInputBtn.targetGraphic.raycastTarget = false;
					LockInput.gameObject.SetActive(false);
					bLockCheck = false;
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
				});
			}
			else
			{
				if (cb != null)
				{
					cb();
				}
				bLockCheck = false;
			}
		}
		else
		{
			if (cb != null)
			{
				cb();
			}
			bLockCheck = false;
		}
	}

	private IEnumerator StageHintMsgTextCoroutine(UnityEngine.Transform tTrans)
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		if (!tTrans.gameObject.activeSelf)
		{
			yield break;
		}
		Vector2 sizeDelta = ((RectTransform)tTrans).sizeDelta;
		Vector3 position = tTrans.position;
		tTrans.Find("HintMsgText").GetComponent<Text>().alignByGeometry = false;
		if (position.x + sizeDelta.x * UIPosToRealPosHalf > CameraSize.x * UIPosToRealPosHalf)
		{
			position.x = CameraSize.x * UIPosToRealPosHalf - sizeDelta.x * UIPosToRealPosHalf;
		}
		else if (position.x - sizeDelta.x * UIPosToRealPosHalf < (0f - CameraSize.x) * UIPosToRealPosHalf)
		{
			position.x = (0f - CameraSize.x) * UIPosToRealPosHalf + sizeDelta.x * UIPosToRealPosHalf;
		}
		tTrans.position = position;
		float fD = 0f;
		while (true)
		{
			fD += 0.05f;
			if (fD >= 1f)
			{
				fD = 1f;
			}
			tTrans.localScale = new Vector3(fD, 1f, 1f);
			if (!(fD >= 1f))
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				continue;
			}
			break;
		}
	}

	private void CheckTurtorial(int nTurtorialID, Action cb = null, string sTriggerName = "")
	{
		if (bLockCheck)
		{
			if (cb != null)
			{
				cb();
			}
			return;
		}
		if (sTriggerName == "UI_Hometop" && MonoBehaviourSingleton<UIManager>.Instance.UI_Parent.Find("UI_Hometop") != null)
		{
			HometopUI component = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent.Find("UI_Hometop").GetComponent<HometopUI>();
			if (component != null)
			{
				component.OnClickBoard_To_Main();
			}
		}
		bLockCheck = true;
		LoadingLock.SetActive(true);
		if (tCheckTurtorialCoroutine != null)
		{
			StopCoroutine(tCheckTurtorialCoroutine);
		}
		tCheckTurtorialCoroutine = StartCoroutine(CheckTurtorialCoroutine(nTurtorialID, cb));
	}

	private IEnumerator WaitUIManagerLoading()
	{
		while (MonoBehaviourSingleton<UIManager>.Instance.IsLoading || MonoBehaviourSingleton<UIManager>.Instance.bLockTurtorial)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private IEnumerator WaitingUILoading(string p_waitingUI)
	{
		while (!MonoBehaviourSingleton<UIManager>.Instance.IsActive(p_waitingUI))
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private void RunTurtorial(int nTutoID, Action cb)
	{
		if (tRunTurtorialCoroutine != null)
		{
			StopCoroutine(tRunTurtorialCoroutine);
		}
		tRunTurtorialCoroutine = StartCoroutine(RunTurtorialCoroutine(nTutoID, cb));
	}

	private IEnumerator CheckTurtorialCoroutine(int nTurtorialID, Action cb)
	{
		yield return WaitUIManagerLoading();
		TUTORIAL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(nTurtorialID, out value))
		{
			string triggerName = GetTriggerName(value.s_TRIGGER);
			if (triggerName != nullStr)
			{
				UnityEngine.Transform uI_Parent = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent;
				int childCount = uI_Parent.childCount;
				bool flag = false;
				for (int num = childCount - 1; num >= 0; num--)
				{
					UnityEngine.Transform child = uI_Parent.GetChild(num);
					if (triggerName == child.gameObject.name)
					{
						LoadingLock.SetActive(true);
						tCheckTurtorialCoroutine = null;
						RunTurtorial(nTurtorialID, cb);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					bLockCheck = false;
					LoadingLock.SetActive(false);
					tCheckTurtorialCoroutine = null;
					if (cb != null)
					{
						cb();
					}
				}
			}
			else
			{
				LoadingLock.SetActive(true);
				tCheckTurtorialCoroutine = null;
				RunTurtorial(nTurtorialID, cb);
			}
		}
		else
		{
			bLockCheck = false;
			LoadingLock.SetActive(false);
			tCheckTurtorialCoroutine = null;
			if (cb != null)
			{
				cb();
			}
		}
	}

	public void CloseTuto()
	{
		bCloseTuto = true;
	}

	public static void ForceCloseTutorial()
	{
		if (!(_instance == null))
		{
			_instance.CloseTuto();
			ClearTutorialFlag();
		}
	}

	private IEnumerator RunTurtorialCoroutine(int nTutoID, Action cb)
	{
		TUTORIAL_TABLE tTUTORIAL_TABLE;
		while (ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(nTutoID, out tTUTORIAL_TABLE) && !tTUTORIAL_TABLE.s_TRIGGER.Contains("stageid") && !bCloseTuto)
		{
			bool bNeedSkip = false;
			if (tTUTORIAL_TABLE.s_TRIGGER != nullStr)
			{
				UnityEngine.Transform RootObj = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent;
				string sTriggerName = GetTriggerName(tTUTORIAL_TABLE.s_TRIGGER);
				do
				{
					yield return WaitUIManagerLoading();
					int childCount = RootObj.childCount;
					UnityEngine.Transform child = RootObj.GetChild(childCount - 1);
					if (GetSkipName(tTUTORIAL_TABLE.s_TRIGGER) == child.gameObject.name)
					{
						bNeedSkip = true;
						break;
					}
					if (child.gameObject.name == "UI_powerup")
					{
						yield return CoroutineDefine._waitForEndOfFrame;
					}
					else
					{
						if (!(sTriggerName != nullStr) || !(child.gameObject.name != sTriggerName))
						{
							break;
						}
						bLockWait = true;
						if (!LockGameObjInUIByName(child, "UI_TopResident(Clone)/Bg/TopImage/CloseBtn", 999, 0))
						{
							yield return CoroutineDefine._waitForEndOfFrame;
							bLockWait = false;
							bCloseTuto = true;
						}
					}
					while (bLockWait && !bCloseTuto)
					{
						yield return CoroutineDefine._1sec;
					}
				}
				while (!bCloseTuto);
			}
			if (bCloseTuto)
			{
				break;
			}
			if (bNeedSkip)
			{
				nTutoID = GetNextID(nTutoID);
				continue;
			}
			if (tTUTORIAL_TABLE.s_WAIT != nullStr)
			{
				LoadingLock.SetActive(false);
				yield return WaitingUILoading(tTUTORIAL_TABLE.s_WAIT);
				yield return WaitUIManagerLoading();
				if (tTUTORIAL_TABLE.s_MASK != nullStr)
				{
					LoadingLock.SetActive(true);
				}
			}
			if (tTUTORIAL_TABLE.s_SORT != "null" && tTUTORIAL_TABLE.s_SORT.Length > 0)
			{
				string[] array = tTUTORIAL_TABLE.s_SORT.Split(',');
				if (array.Length != 0)
				{
					int i = 1;
					switch (array[0])
					{
					case "1":
						for (; i < array.Length; i++)
						{
							ManagedSingleton<EquipHelper>.Instance.listCharacterCompelled.Add(int.Parse(array[i]));
						}
						break;
					case "2":
						for (; i < array.Length; i++)
						{
							ManagedSingleton<EquipHelper>.Instance.listWeaponCompelled.Add(int.Parse(array[i]));
						}
						break;
					case "3":
						for (; i < array.Length; i++)
						{
							ManagedSingleton<EquipHelper>.Instance.listChipCompelled.Add(int.Parse(array[i]));
						}
						break;
					}
				}
			}
			bool bNeedIsTale = false;
			bool bNeedRunLock = true;
			if (tTUTORIAL_TABLE.s_MASK != nullStr)
			{
				if (tTUTORIAL_TABLE.s_TRIGGER == "UI_StoryStageSelect" && tTUTORIAL_TABLE.s_MASK == "BtnNext" && OrangeSceneManager.FindObjectOfTypeCustom<StoryStageSelectUI>().CurrentArea >= tTUTORIAL_TABLE.n_Index + 2)
				{
					bNeedRunLock = false;
				}
				if (bNeedRunLock)
				{
					StartCoroutine(TryLockObjCoroutine(tTUTORIAL_TABLE.s_MASK, tTUTORIAL_TABLE.n_Index, tTUTORIAL_TABLE.n_SCENARIO));
					bLockWait = true;
				}
			}
			else if (tTUTORIAL_TABLE.n_SCENARIO != 0)
			{
				bNeedIsTale = true;
				bLockWait = true;
				bUseUnLockTime = true;
				fOpenUIErrorUnLockTime = 3f;
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Dialog", delegate(DialogUI ui)
				{
					bUseUnLockTime = false;
					ui.Setup(tTUTORIAL_TABLE.n_SCENARIO, ContinuePlay);
					LoadingLock.SetActive(false);
					ui.SetCanvas(true);
					LockInput.gameObject.SetActive(false);
					LockImg.SetActive(false);
				});
			}
			while (bLockWait && !bCloseTuto)
			{
				yield return CoroutineDefine._1sec;
				if (bUseUnLockTime)
				{
					fOpenUIErrorUnLockTime -= 1f;
					if (fOpenUIErrorUnLockTime <= 0f)
					{
						bUseUnLockTime = false;
						bLockWait = false;
					}
				}
				if (!bIsLockBtn)
				{
					continue;
				}
				bool flag = false;
				if (lockBtns != null)
				{
					Button[] array2 = lockBtns;
					for (int j = 0; j < array2.Length; j++)
					{
						if (array2[j].interactable)
						{
							flag = true;
						}
					}
				}
				if (lockIconBases != null && lockIconBases.Length != 0)
				{
					flag = true;
				}
				if (locktoggles != null && locktoggles.Length != 0)
				{
					flag = true;
				}
				if (!flag)
				{
					ContinuePlay();
					bCloseTuto = true;
				}
			}
			LoadingLock.SetActive(true);
			if (tTUTORIAL_TABLE.s_TRIGGER == "UI_StoryStageSelect" && tTUTORIAL_TABLE.s_MASK == "BtnNext" && bNeedRunLock)
			{
				yield return CoroutineDefine._1sec;
				yield return CoroutineDefine._1sec;
			}
			if (bCloseTuto)
			{
				break;
			}
			if (tTUTORIAL_TABLE.n_SAVE != 0 && tTUTORIAL_TABLE.n_SAVE != -1)
			{
				bLockWait = true;
				ManagedSingleton<PlayerNetManager>.Instance.TurtorialFlagRq(tTUTORIAL_TABLE.n_SAVE, ContinuePlay);
			}
			while (bLockWait)
			{
				yield return CoroutineDefine._1sec;
			}
			yield return WaitUIManagerLoading();
			if (tTUTORIAL_TABLE.n_SAVE == -1)
			{
				break;
			}
			nTutoID = GetNextID(nTutoID);
			if (bNeedIsTale)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			else
			{
				yield return CoroutineDefine._1sec;
			}
		}
		bLockCheck = false;
		bIsLockBtn = false;
		LoadingLock.SetActive(false);
		LockInput.gameObject.SetActive(false);
		LockImg.SetActive(false);
		InstanceHintMsgText.transform.parent.gameObject.SetActive(false);
		UnLockBtn.SetActive(false);
		bCloseTuto = false;
		bLockWait = false;
		RemoveBtnLinks();
		if (tFollowCoroutine != null)
		{
			StopCoroutine(tFollowCoroutine);
			tFollowCoroutine = null;
		}
		if (MoveHintMsgCoroutine != null)
		{
			StopCoroutine(MoveHintMsgCoroutine);
			MoveHintMsgCoroutine = null;
		}
		Action onTutorialFinishedEvent = TurtorialUI.OnTutorialFinishedEvent;
		if (onTutorialFinishedEvent != null)
		{
			onTutorialFinishedEvent();
		}
		TurtorialUI.OnTutorialFinishedEvent = null;
		if (cb != null)
		{
			cb();
		}
	}

	public static int GetNextID(int nNowID)
	{
		System.Collections.Generic.Dictionary<int, TUTORIAL_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Key == nNowID)
			{
				if (!enumerator.MoveNext())
				{
					break;
				}
				return enumerator.Current.Key;
			}
		}
		return 0;
	}

	private void RemoveBtnLinks()
	{
		if (lockBtns != null)
		{
			for (int i = 0; i < lockBtns.Length; i++)
			{
				lockBtns[i].onClick.RemoveListener(ButtonPressCB);
			}
			lockBtns = null;
		}
		if (lockIconBases != null)
		{
			for (int j = 0; j < lockIconBases.Length; j++)
			{
				lockIconBases[j].callback -= IconBasePressCB;
			}
			lockIconBases = null;
		}
		if (locktoggles != null)
		{
			for (int k = 0; k < locktoggles.Length; k++)
			{
				locktoggles[k].onValueChanged.RemoveListener(TogglePressCB);
			}
			locktoggles = null;
		}
	}

	private void ContinuePlay()
	{
		RemoveBtnLinks();
		LoadingLock.SetActive(true);
		bLockWait = false;
		LockInput.gameObject.SetActive(false);
		LockImg.SetActive(false);
		InstanceHintMsgText.transform.parent.gameObject.SetActive(false);
		DownArrowImg.SetActive(false);
		UpArrowImg.SetActive(false);
		bIsLockBtn = false;
		if (tFollowCoroutine != null)
		{
			StopCoroutine(tFollowCoroutine);
			tFollowCoroutine = null;
		}
		if (MoveHintMsgCoroutine != null)
		{
			StopCoroutine(MoveHintMsgCoroutine);
			MoveHintMsgCoroutine = null;
		}
	}

	private IEnumerator TryLockObjCoroutine(string name, int nMaxIndex, int nScenario)
	{
		int nTryCount = 0;
		bool bLock = false;
		yield return WaitUIManagerLoading();
		for (; nTryCount < 5; nTryCount++)
		{
			if (LockGameObjInUIByName(null, name, nMaxIndex, nScenario))
			{
				bLock = true;
				break;
			}
			yield return CoroutineDefine._1sec;
		}
		if (!bLock)
		{
			ContinuePlay();
		}
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	private IEnumerator MoveHintMsgTextCoroutine()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		if (!InstanceHintMsgText.transform.parent.gameObject.activeSelf)
		{
			MoveHintMsgCoroutine = null;
			yield break;
		}
		Vector3 tLockPos = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector2 vSize = Vector2.zero;
		Vector2 vSizeArrow = ((RectTransform)UpArrowImg.transform).sizeDelta;
		float fD3 = 0f;
		while (true)
		{
			if (vSize != ((RectTransform)InstanceHintMsgText.transform.parent).sizeDelta || tLockPos != LockInput.transform.position)
			{
				vSize = ((RectTransform)InstanceHintMsgText.transform.parent).sizeDelta;
				Vector3 position;
				tLockPos = (position = LockInput.transform.position);
				Vector3 vector = position;
				float num = LockInput.transform.localScale.y * 100f * UIPosToRealPosHalf;
				fD3 = (vSize.y + 60f) * UIPosToRealPosHalf + num;
				vector.y += fD3;
				float num2 = CameraSize.y * UIPosToRealPosHalf;
				if (vector.y + vSize.y * UIPosToRealPosHalf > num2)
				{
					vector.y -= 2f * fD3;
					DownArrowImg.SetActive(false);
					UpArrowImg.SetActive(false);
				}
				else
				{
					DownArrowImg.SetActive(false);
					UpArrowImg.SetActive(false);
				}
				if (vector.x + vSize.x * UIPosToRealPosHalf > CameraSize.x * UIPosToRealPosHalf)
				{
					vector.x = CameraSize.x * UIPosToRealPosHalf - vSize.x * UIPosToRealPosHalf;
				}
				else if (vector.x - vSize.x * UIPosToRealPosHalf < (0f - CameraSize.x) * UIPosToRealPosHalf)
				{
					vector.x = (0f - CameraSize.x) * UIPosToRealPosHalf + vSize.x * UIPosToRealPosHalf;
				}
				Vector3 vector2 = vector;
				InstanceHintMsgText.transform.parent.position = vector2;
				UpArrowImg.transform.position = vector2 + new Vector3(0f, (vSize.y + vSizeArrow.y) * UIPosToRealPosHalf, 0f);
				DownArrowImg.transform.position = vector2 + new Vector3(0f, (0f - (vSize.y + vSizeArrow.y)) * UIPosToRealPosHalf, 0f);
				InstanceHintMsgText.transform.parent.localScale = new Vector3(0f, 1f, 1f);
				fD3 = 0f;
			}
			else
			{
				fD3 += 0.05f;
				if (fD3 >= 1f)
				{
					fD3 = 1f;
				}
				InstanceHintMsgText.transform.parent.localScale = new Vector3(fD3, 1f, 1f);
				if (fD3 >= 1f)
				{
					break;
				}
			}
			InstanceHintMsgText.alignByGeometry = false;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (true)
		{
			if (vSize != ((RectTransform)InstanceHintMsgText.transform.parent).sizeDelta || tLockPos != LockInput.transform.position)
			{
				vSize = ((RectTransform)InstanceHintMsgText.transform.parent).sizeDelta;
				Vector3 position;
				tLockPos = (position = LockInput.transform.position);
				Vector3 vector = position;
				float num = LockInput.transform.localScale.y * 100f * UIPosToRealPosHalf;
				fD3 = (vSize.y + 60f) * UIPosToRealPosHalf + num;
				vector.y += fD3;
				float num3 = CameraSize.y * UIPosToRealPosHalf;
				if (vector.y + vSize.y * UIPosToRealPosHalf > num3)
				{
					vector.y -= 2f * fD3;
				}
				if (vector.x + vSize.x * UIPosToRealPosHalf > CameraSize.x * UIPosToRealPosHalf)
				{
					vector.x = CameraSize.x * UIPosToRealPosHalf - vSize.x * UIPosToRealPosHalf;
				}
				else if (vector.x - vSize.x * UIPosToRealPosHalf < (0f - CameraSize.x) * UIPosToRealPosHalf)
				{
					vector.x = (0f - CameraSize.x) * UIPosToRealPosHalf + vSize.x * UIPosToRealPosHalf;
				}
				Vector3 vector2 = vector;
				InstanceHintMsgText.transform.parent.position = vector2;
				UpArrowImg.transform.position = vector2 + new Vector3(0f, (vSize.y + vSizeArrow.y) * UIPosToRealPosHalf, 0f);
				DownArrowImg.transform.position = vector2 + new Vector3(0f, (0f - (vSize.y + vSizeArrow.y)) * UIPosToRealPosHalf, 0f);
			}
			InstanceHintMsgText.alignByGeometry = false;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private void EffectComplete(string type, EventObject eventObject)
	{
	}

	private IEnumerator FollowCoroutine(RectTransform tRectTransform)
	{
		Vector2 vSize = tRectTransform.rect.size;
		Vector3 tLockPos = tRectTransform.position;
		tLockPos.x -= 1000f;
		vSize.x -= 1000f;
		while (true)
		{
			if (vSize != tRectTransform.rect.size)
			{
				vSize = tRectTransform.rect.size;
				LockInput.transform.localScale = new Vector3(vSize.x / 100f, vSize.y / 100f, 1f);
			}
			if (tRectTransform.position != tLockPos)
			{
				tLockPos = tRectTransform.position;
				Vector3 position = tLockPos;
				position.x -= (tRectTransform.pivot.x - 0.5f) * vSize.x * UIPosToRealPos;
				position.y -= (tRectTransform.pivot.y - 0.5f) * vSize.y * UIPosToRealPos;
				LockInput.transform.position = position;
				LockImg.transform.position = position;
				if (position.x + 350f * UIPosToRealPos > CameraSize.x * UIPosToRealPosHalf)
				{
					LockImg.transform.localScale = new Vector3(-1f, 1f, 1f);
				}
				else
				{
					LockImg.transform.localScale = Vector3.one;
				}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private IEnumerator FollowCoroutine(TutorialLockUIHelper lockHelper)
	{
		Camera uICamera = lockHelper.UICamera;
		RectTransform component = lockHelper.GetComponent<RectTransform>();
		RectTransform component2 = MonoBehaviourSingleton<UIManager>.Instance.CanvasUI.GetComponent<RectTransform>();
		Vector3 vector = uICamera.WorldToScreenPoint(component.position);
		Vector2 vector2 = new Vector2(vector.x / (float)Screen.width, vector.y / (float)Screen.height);
		vector2 -= component.pivot;
		Vector2 vector3 = new Vector2(vector2.x * component2.rect.width, vector2.y * component2.rect.height);
		LockInput.transform.localPosition = vector3;
		LockInput.transform.localScale = component.rect.size / 100f;
		LockImg.transform.localPosition = vector3;
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	private bool LockGameObjInUIByName(UnityEngine.Transform tTrans, string name, int nMaxIndex, int nScenario)
	{
		if (LockInput == null)
		{
			return false;
		}
		TutorialLockUIHelper value;
		UnityEngine.Transform transform;
		if (LockUIHelperDict.TryGetValue(name, out value))
		{
			transform = value.transform;
			LockInput.gameObject.SetActive(true);
			LockImg.SetActive(true);
			LoadTutorialClick();
			if (tFollowCoroutine != null)
			{
				StopCoroutine(tFollowCoroutine);
				tFollowCoroutine = null;
			}
			tFollowCoroutine = StartCoroutine(FollowCoroutine(value));
		}
		else
		{
			if (tTrans == null)
			{
				GameObject gameObject = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent.parent.gameObject;
				if (gameObject == null)
				{
					return false;
				}
				tTrans = gameObject.transform;
			}
			List<UnityEngine.Transform> list = FindGameObj(tTrans, name.Split('/'));
			if (list == null || list.Count == 0)
			{
				return false;
			}
			int num = nMaxIndex;
			if (num >= list.Count)
			{
				if (nMaxIndex != 999)
				{
					return false;
				}
				num = list.Count - 1;
			}
			transform = list[num];
			RectTransform rectTransform = list[num] as RectTransform;
			if (rectTransform == null)
			{
				return false;
			}
			LockInput.gameObject.SetActive(true);
			LockImg.SetActive(true);
			LoadTutorialClick();
			if (tFollowCoroutine != null)
			{
				StopCoroutine(tFollowCoroutine);
				tFollowCoroutine = null;
			}
			tFollowCoroutine = StartCoroutine(FollowCoroutine(rectTransform));
		}
		SCENARIO_TABLE value2;
		if (nScenario != 0 && ManagedSingleton<OrangeDataManager>.Instance.SCENARIO_TABLE_DICT.TryGetValue(nScenario, out value2))
		{
			InstanceHintMsgText.transform.parent.gameObject.SetActive(true);
			DownArrowImg.SetActive(false);
			UpArrowImg.SetActive(false);
			InstanceHintMsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.SCENARIOTEXT_TABLE_DICT.GetL10nValue(value2.w_CONTENT);
			InstanceHintMsgText.transform.parent.localScale = new Vector3(0f, 1f, 1f);
			if (MoveHintMsgCoroutine == null)
			{
				MoveHintMsgCoroutine = StartCoroutine(MoveHintMsgTextCoroutine());
			}
		}
		lockBtns = transform.GetComponentsInChildren<Button>();
		lockIconBases = transform.GetComponentsInChildren<IconBase>();
		locktoggles = transform.GetComponentsInChildren<Toggle>();
		bool flag = false;
		if ((lockBtns != null && lockBtns.Length != 0) || (lockIconBases != null && lockIconBases.Length != 0) || (locktoggles != null && locktoggles.Length != 0))
		{
			if (lockBtns != null)
			{
				for (int i = 0; i < lockBtns.Length; i++)
				{
					if (lockBtns[i].interactable)
					{
						lockBtns[i].onClick.AddListener(ButtonPressCB);
						flag = true;
					}
				}
			}
			if (lockIconBases != null)
			{
				flag = true;
				for (int j = 0; j < lockIconBases.Length; j++)
				{
					lockIconBases[j].callback += IconBasePressCB;
				}
			}
			if (locktoggles != null)
			{
				flag = true;
				for (int k = 0; k < locktoggles.Length; k++)
				{
					locktoggles[k].onValueChanged.AddListener(TogglePressCB);
				}
			}
			LockInputBtn.enabled = false;
			LockInputBtn.targetGraphic.raycastTarget = false;
			bIsLockBtn = true;
		}
		if (!flag)
		{
			LockInputBtn.enabled = true;
			LockInputBtn.targetGraphic.raycastTarget = true;
		}
		LoadingLock.SetActive(false);
		return true;
	}

	private void LoadTutorialClick()
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("dragonbones/TutoClick", "TutoClick", OnTutorialClickLoaded);
	}

	private void OnTutorialClickLoaded(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (LockImg.transform.childCount > 0)
		{
			for (int num = LockImg.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(LockImg.transform.GetChild(num).gameObject);
			}
		}
		UnityEngine.Object.Instantiate(obj, LockImg.transform).GetComponent<UnityArmatureComponent>();
	}

	private void ButtonPressCB()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
		ContinuePlay();
	}

	private void TogglePressCB(bool bOn)
	{
		if (bOn)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
			ContinuePlay();
		}
	}

	private void IconBasePressCB(int idx)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
		ContinuePlay();
	}

	private List<UnityEngine.Transform> FindGameObj(UnityEngine.Transform tTrans, string[] names)
	{
		List<UnityEngine.Transform> list = new List<UnityEngine.Transform>();
		if (names.Length > 1)
		{
			UnityEngine.Transform transform = tTrans;
			int num = names.Length - 1;
			while (transform != null && num >= 0 && transform.gameObject.name == names[num])
			{
				num--;
				transform = transform.parent;
			}
			if (num == -1)
			{
				list.Add(tTrans);
			}
		}
		else if (tTrans.gameObject.name == names[0])
		{
			list.Add(tTrans);
		}
		int childCount = tTrans.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Transform transform = tTrans.GetChild(i);
			if (transform.gameObject.activeSelf)
			{
				List<UnityEngine.Transform> collection = FindGameObj(transform, names);
				list.AddRange(collection);
			}
		}
		return list;
	}

	public static bool IsTutorialing()
	{
		if (_instance == null)
		{
			return false;
		}
		if (!_instance.bLockCheck)
		{
			return _instance.bLockWait;
		}
		return true;
	}

	public static void ClearTutorialFlag()
	{
		if (!(_instance == null))
		{
			_instance.bLockCheck = false;
			_instance.bIsLockBtn = false;
			_instance.LoadingLock.SetActive(false);
			_instance.LockInput.gameObject.SetActive(false);
			_instance.LockImg.SetActive(false);
			_instance.InstanceHintMsgText.transform.parent.gameObject.SetActive(false);
			_instance.UnLockBtn.SetActive(false);
			_instance.bCloseTuto = false;
			_instance.bLockWait = false;
			_instance.RemoveBtnLinks();
			if (_instance.tFollowCoroutine != null)
			{
				_instance.StopCoroutine(_instance.tFollowCoroutine);
				_instance.tFollowCoroutine = null;
			}
			if (_instance.MoveHintMsgCoroutine != null)
			{
				_instance.StopCoroutine(_instance.MoveHintMsgCoroutine);
				_instance.MoveHintMsgCoroutine = null;
			}
			if (_instance.tCheckTurtorialCoroutine != null)
			{
				_instance.StopCoroutine(_instance.tCheckTurtorialCoroutine);
				_instance.tCheckTurtorialCoroutine = null;
			}
			if (_instance.tRunTurtorialCoroutine != null)
			{
				_instance.StopCoroutine(_instance.tRunTurtorialCoroutine);
				_instance.tRunTurtorialCoroutine = null;
			}
			TurtorialUI.OnTutorialFinishedEvent = null;
			UnityEngine.Object.Destroy(_instance.gameObject);
			_instance = null;
		}
	}

	public static void RegisterLockUIName(string path, TutorialLockUIHelper lockHelper)
	{
		if (_instance != null)
		{
			_instance.LockUIHelperDict[path] = lockHelper;
		}
	}

	public static void UnregisterLockUIName(string path)
	{
		if (_instance != null && _instance.LockUIHelperDict.ContainsKey(path))
		{
			_instance.LockUIHelperDict.Remove(path);
		}
	}
}
