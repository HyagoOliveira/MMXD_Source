using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class StoryStageSelectUI : OrangeUIBase
{
	public enum DIFFICULTY_TYPE
	{
		NORMAL = 1,
		HARD = 2
	}

	[SerializeField]
	private Toggle m_toggleNormal;

	[SerializeField]
	private Toggle m_toggleHard;

	[SerializeField]
	private OrangeText m_areaName;

	[SerializeField]
	private Image m_stageThumbnail;

	[SerializeField]
	private Transform m_stagePathRoot;

	[SerializeField]
	private Transform m_stageIconRoot;

	[SerializeField]
	private Transform m_starConditionRoot;

	[SerializeField]
	private Button m_nextAreaBtn;

	[SerializeField]
	private Button m_previousAreaBtn;

	[SerializeField]
	private Transform m_normalDifficultyFX;

	[SerializeField]
	private Transform m_hardDifficultyFX;

	[Header("Stage Setting Info")]
	private const int fixedSize = 6;

	public string[] m_stageSetting = new string[6];

	private const int m_maxStageIconNum = 6;

	private const int m_maxPathNum = 6;

	private StageIcon[] m_stageIconArray = new StageIcon[6];

	private MapPathUI[] m_pathArray = new MapPathUI[6];

	private GameObject m_starConditionGameObject;

	private OrangeBgExt m_bgExt;

	private SaveData m_saveData;

	private bool m_bSaveLastArea = true;

	private int m_currentArea = 1;

	private DIFFICULTY_TYPE m_currentDifficulty = DIFFICULTY_TYPE.NORMAL;

	private int m_targetArea = 1;

	private DIFFICULTY_TYPE m_targetDifficulty = DIFFICULTY_TYPE.NORMAL;

	private bool IgnoreFristSE = true;

	private bool CanPlayLineSE = true;

	private List<int> m_tweenIdList = new List<int>();

	[Header("快速切換章節")]
	[SerializeField]
	private GameObject m_areaSelectList;

	[SerializeField]
	private LoopVerticalScrollRect areaScrollRect;

	[SerializeField]
	private AreaSelectUnit areaSelectUnit;

	public int TargetArea
	{
		set
		{
			m_targetArea = value;
		}
	}

	public DIFFICULTY_TYPE TargetDifficulty
	{
		set
		{
			m_targetDifficulty = value;
		}
	}

	public int CurrentArea
	{
		get
		{
			return m_currentArea;
		}
	}

	public DIFFICULTY_TYPE CurrentDifficulty
	{
		get
		{
			return m_currentDifficulty;
		}
	}

	public void Setup(bool bRestoreLastArea = true)
	{
		m_bgExt = Background as OrangeBgExt;
		m_saveData = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData;
		m_bSaveLastArea = bRestoreLastArea;
		if (m_stagePathRoot.childCount > 0)
		{
			Transform child = m_stagePathRoot.GetChild(0);
			m_pathArray[0] = child.GetComponent<MapPathUI>();
			for (int i = 1; i < 6; i++)
			{
				Transform transform = UnityEngine.Object.Instantiate(child, m_stagePathRoot);
				m_pathArray[i] = transform.GetComponent<MapPathUI>();
				m_pathArray[i].gameObject.name = string.Format("Path{0}", i);
			}
		}
		if (bRestoreLastArea)
		{
			if (m_saveData.HowToGetStageID != 0)
			{
				m_targetArea = m_saveData.HowToGetStageID;
				m_targetDifficulty = (DIFFICULTY_TYPE)m_saveData.HowToGetStageDifficulty;
			}
			else
			{
				m_targetArea = m_saveData.LastSelectedStageID;
				m_targetDifficulty = (DIFFICULTY_TYPE)m_saveData.LastSelectedStageDifficulty;
			}
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "StageIcon", "StageIcon", delegate(GameObject obj)
		{
			for (int j = 0; j < 6; j++)
			{
				Vector3 localPosition = new Vector3(-600f + (float)j * 250f, 0f, 0f);
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, m_stageIconRoot);
				gameObject.transform.localPosition = localPosition;
				m_stageIconArray[j] = gameObject.GetComponent<StageIcon>();
			}
			if (m_targetDifficulty == DIFFICULTY_TYPE.NORMAL)
			{
				m_toggleNormal.isOn = true;
				IgnoreFristSE = false;
			}
			else
			{
				m_toggleHard.isOn = true;
			}
			UpdateStageLayout();
		});
		backToHometopCB = delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_STOP);
		};
	}

	private void InitStarCondition(int stageID, int difficulty)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/StarConditionComp", "StarConditionComp", delegate(GameObject obj)
		{
			if (m_starConditionGameObject != null)
			{
				UnityEngine.Object.Destroy(m_starConditionGameObject);
				m_starConditionGameObject = null;
			}
			m_starConditionGameObject = UnityEngine.Object.Instantiate(obj, m_starConditionRoot);
			m_starConditionGameObject.GetComponent<StarConditionComponent>().Setup(1001, stageID, difficulty);
		});
	}

	private void UpdateStageLayout()
	{
		List<STAGE_TABLE> listStageByTypeAreaDifficulty = GetListStageByTypeAreaDifficulty(StageType.Scenario, m_targetArea, 2);
		m_toggleNormal.interactable = true;
		m_toggleHard.gameObject.SetActive(listStageByTypeAreaDifficulty.Count != 0);
		List<STAGE_TABLE> listStageByTypeAreaDifficulty2 = GetListStageByTypeAreaDifficulty(StageType.Scenario, m_targetArea, (int)m_targetDifficulty);
		if (listStageByTypeAreaDifficulty2.Count == 0)
		{
			return;
		}
		InitStarCondition(listStageByTypeAreaDifficulty2[0].n_MAIN, (int)m_targetDifficulty);
		string p_key = string.Format("MAIN_STAGE_{0}", listStageByTypeAreaDifficulty2[0].n_MAIN);
		m_areaName.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(p_key);
		m_bgExt.ChangeBackground(listStageByTypeAreaDifficulty2[0].s_BG);
		string assetName = string.Format("{0}_{1}", listStageByTypeAreaDifficulty2[0].s_BG, "ori");
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_stageBg, assetName, delegate(Sprite obj)
		{
			if (obj != null)
			{
				m_stageThumbnail.sprite = obj;
			}
		});
		List<STAGE_TABLE> listStageByTypeAreaDifficulty3 = GetListStageByTypeAreaDifficulty(StageType.Scenario, m_targetArea + 1, (int)m_targetDifficulty);
		m_nextAreaBtn.gameObject.SetActive(listStageByTypeAreaDifficulty3.Count != 0);
		listStageByTypeAreaDifficulty3 = GetListStageByTypeAreaDifficulty(StageType.Scenario, m_targetArea - 1, (int)m_targetDifficulty);
		m_previousAreaBtn.gameObject.SetActive(listStageByTypeAreaDifficulty3.Count != 0);
		for (int i = 0; i < 6; i++)
		{
			StageIcon stageIcon = m_stageIconArray[i];
			if (i < listStageByTypeAreaDifficulty2.Count)
			{
				stageIcon.gameObject.SetActive(true);
				if (i < 5)
				{
					m_pathArray[i].gameObject.SetActive(true);
				}
				stageIcon.SetBossStage(i == listStageByTypeAreaDifficulty2.Count - 1);
				stageIcon.SetStageInfo(listStageByTypeAreaDifficulty2[i]);
				stageIcon.PlayEnableEffect((float)i * 0.1f);
				string text = listStageByTypeAreaDifficulty2[i].s_PATH;
				if (text == "null")
				{
					text = GetDebugStageInfo(i);
				}
				string[] array = text.Split(',');
				if (array.Length >= 5)
				{
					stageIcon.transform.localPosition = new Vector3(float.Parse(array[0]), float.Parse(array[1]), 0f);
					int num = int.Parse(array[2]);
					Array.Resize(ref m_pathArray[i].pointArray, num);
					m_pathArray[i].transform.localPosition = new Vector3(float.Parse(array[3]), float.Parse(array[4]), 1f);
					for (int j = 0; j < num; j++)
					{
						m_pathArray[i].pointArray[j].rotation = float.Parse(array[5 + j * 2]);
						m_pathArray[i].pointArray[j].length = float.Parse(array[6 + j * 2]);
						m_pathArray[i].m_slider = 0f;
						m_pathArray[i].ForceValidate();
					}
				}
				if (i > 0 && stageIcon.IsUnlocked())
				{
					StageInfo value = null;
					ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(listStageByTypeAreaDifficulty2[i].n_ID, out value);
					if (value == null)
					{
						NewStageUnlocked(i, 1f);
					}
					else
					{
						m_pathArray[i - 1].m_slider = 1f;
					}
				}
				float delay = 0.5f;
				float time = 0.5f;
				CanvasGroup canvasGroup = m_stagePathRoot.GetComponent<CanvasGroup>();
				canvasGroup.alpha = 0f;
				m_tweenIdList.Add(LeanTween.value(0f, 1f, time).setOnUpdate(delegate(float val)
				{
					canvasGroup.alpha = val;
				}).setDelay(delay)
					.uniqueId);
				}
				else
				{
					stageIcon.gameObject.SetActive(false);
					if (i < 5)
					{
						m_pathArray[i].gameObject.SetActive(false);
					}
				}
			}
			m_currentArea = m_targetArea;
			m_currentDifficulty = m_targetDifficulty;
			if (m_bSaveLastArea && m_saveData.HowToGetStageID == 0)
			{
				m_saveData.LastSelectedStageDifficulty = (int)m_currentDifficulty;
				m_saveData.LastSelectedStageID = m_currentArea;
			}
			m_saveData.HowToGetStageID = 0;
			m_saveData.HowToGetStageDifficulty = 0;
		}

		private string GetDebugStageInfo(int index)
		{
			switch (index)
			{
			case 0:
				return "-600,235,4,-603,138,0,284.49,90,71.4,180,150.9,90,48.6";
			case 1:
				return "-378,0,3,-272,0,90,35.7,180,95.4,90,31.5";
			case 2:
				return "-100,90,5,4,98,90,67.9,0,183.6,-90,80,0,149.3,90,47.8";
			case 3:
				return "150,-227,5,259,-224,90,80,180,260,-90,123.5,180,133.7,90,77.2";
			case 4:
				return "400,162,3,400,64,0,409.8,90,253.8,180,102.1";
			case 5:
				return "650,-143,1,0,0,0,0";
			default:
				return "";
			}
		}

		private List<STAGE_TABLE> GetListStageByTypeAreaDifficulty(StageType p_type, int mainAreaID, int difficulty)
		{
			int type = (int)p_type;
			return ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == type && x.n_MAIN == mainAreaID && x.n_DIFFICULTY == difficulty).ToList();
		}

		public void OnClickDifficultyNormal(Toggle button)
		{
			if (m_currentDifficulty != DIFFICULTY_TYPE.NORMAL)
			{
				CanPlayLineSE = false;
				StopTweenAnim();
				m_targetDifficulty = DIFFICULTY_TYPE.NORMAL;
				UpdateStageLayout();
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
				if (m_normalDifficultyFX != null && m_hardDifficultyFX != null)
				{
					m_normalDifficultyFX.gameObject.SetActive(true);
					m_hardDifficultyFX.gameObject.SetActive(false);
				}
			}
		}

		public void OnClickDifficultyHard(Toggle button)
		{
			if (m_currentDifficulty != DIFFICULTY_TYPE.HARD)
			{
				CanPlayLineSE = false;
				StopTweenAnim();
				m_targetDifficulty = DIFFICULTY_TYPE.HARD;
				UpdateStageLayout();
				if (!IgnoreFristSE)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
				}
				else
				{
					IgnoreFristSE = !IgnoreFristSE;
				}
				if (m_normalDifficultyFX != null && m_hardDifficultyFX != null)
				{
					m_normalDifficultyFX.gameObject.SetActive(false);
					m_hardDifficultyFX.gameObject.SetActive(true);
				}
			}
		}

		private void StopTweenAnim()
		{
			foreach (int tweenId in m_tweenIdList)
			{
				int uniqueId = tweenId;
				LeanTween.cancel(ref uniqueId, true);
			}
			m_tweenIdList.Clear();
		}

		private void ChangeArea(int targetArea, DIFFICULTY_TYPE targetDifficulty)
		{
			if (GetListStageByTypeAreaDifficulty(StageType.Scenario, targetArea, (int)targetDifficulty).Count != 0)
			{
				m_targetArea = targetArea;
				m_targetDifficulty = targetDifficulty;
				CanPlayLineSE = false;
				StopTweenAnim();
				float num = 0.3f;
				float num2 = num / 3f;
				CanvasGroup canvasGroup = m_stageIconRoot.GetComponent<CanvasGroup>();
				Vector3 to = ((targetArea <= m_currentArea) ? new Vector3(1920f, 0f, 0f) : new Vector3(-1920f, 0f, 0f));
				m_nextAreaBtn.gameObject.SetActive(false);
				m_previousAreaBtn.gameObject.SetActive(false);
				m_toggleNormal.interactable = false;
				m_toggleHard.interactable = false;
				m_tweenIdList.Add(LeanTween.value(1f, 0f, num).setOnUpdate(delegate(float val)
				{
					canvasGroup.alpha = val;
				}).uniqueId);
				m_tweenIdList.Add(LeanTween.moveLocal(m_stageIconRoot.gameObject, to, num).setOnComplete((Action)delegate
				{
					m_toggleNormal.interactable = true;
					m_toggleHard.interactable = true;
					UpdateStageLayout();
					m_stageIconRoot.localPosition = Vector3.zero;
					canvasGroup.alpha = 1f;
				}).uniqueId);
			}
		}

		public void OnClickPreviousArea()
		{
			ChangeArea(m_currentArea - 1, m_currentDifficulty);
		}

		public void OnClickNextArea()
		{
			ChangeArea(m_currentArea + 1, m_currentDifficulty);
		}

		public void NewStageUnlocked(int index, float animDelay = 0f)
		{
			float time = 1.5f;
			if (index >= m_stageIconArray.Length)
			{
				return;
			}
			for (int i = 0; i <= index; i++)
			{
				if (i != index)
				{
					continue;
				}
				m_stageIconArray[i].SetStatus(StageIcon.STAGE_STATUS.NEWSTAGE);
				if (i <= 0)
				{
					continue;
				}
				int tempStageIndex = i - 1;
				Invoke("delaySE", animDelay);
				CanPlayLineSE = true;
				int uniqueId = LeanTween.value(0f, 1f, time).setOnUpdate(delegate(float val)
				{
					m_pathArray[tempStageIndex].m_slider = val;
					m_pathArray[tempStageIndex].ForceValidate();
				}).setDelay(animDelay)
					.setOnComplete((Action)delegate
					{
						if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading && base.IsVisible && base.enabled && CanPlayLineSE)
						{
							MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OPEN01);
						}
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_STOP);
					})
					.uniqueId;
				m_tweenIdList.Add(uniqueId);
			}
		}

		public void delaySE()
		{
			if (CanPlayLineSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_LP);
			}
		}

		private void OnEnable()
		{
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
		}

		private void OnDisable()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
		}

		private void Clear()
		{
			OnClickCloseBtn();
		}

		private void OnDestroy()
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			StopTweenAnim();
		}

		public override void OnClickCloseBtn()
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_STOP);
			base.OnClickCloseBtn();
		}

		public void OnClickAreaSelectList()
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			areaScrollRect.ClearCells();
			m_areaSelectList.SetActive(true);
			List<int> list = new List<int>();
			int type = 1;
			foreach (STAGE_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == type && x.n_DIFFICULTY == (int)m_targetDifficulty).ToList())
			{
				if (!list.Contains(item.n_MAIN))
				{
					list.Add(item.n_MAIN);
				}
			}
			areaScrollRect.OrangeInit(areaSelectUnit, list.Count, list.Count);
			areaScrollRect.verticalScrollbar.value = ((float)CurrentArea - 1f) / ((float)list.Count - 0.9f);
		}

		public void OnClickAreaSelectUnit(int targetarea)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			ChangeArea(targetarea, CurrentDifficulty);
			m_areaSelectList.SetActive(false);
		}
	}
