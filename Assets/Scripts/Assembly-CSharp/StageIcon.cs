using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageIcon : MonoBehaviour
{
	public enum STAGE_STATUS
	{
		LOCKED = 0,
		OPEN = 1,
		NEWSTAGE = 2,
		PERFECT = 3
	}

	[Header("Status Icon")]
	[SerializeField]
	private Image m_cleared;

	[SerializeField]
	private Image m_newStage;

	[SerializeField]
	private Image m_red;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Transform m_boss;

	[SerializeField]
	private OrangeText m_stageNumber;

	[SerializeField]
	private Transform m_starsRoot;

	private bool m_bIsBossStage;

	private STAGE_TABLE m_stageTable;

	private Image m_glowObject;

	private List<int> m_tweenIdList = new List<int>();

	private STAGE_STATUS m_stageStatus;

	private void Start()
	{
		Setup();
	}

	public void Setup()
	{
		SetStatus(m_stageStatus);
	}

	public void SetBossStage(bool bIsBossStage)
	{
		m_bIsBossStage = bIsBossStage;
		m_boss.gameObject.SetActive(m_bIsBossStage);
	}

	public void SetStageInfo(STAGE_TABLE stageTable)
	{
		m_stageTable = stageTable;
		StageInfo value = null;
		StopTweenAnim();
		m_stageNumber.text = stageTable.n_MAIN + "-" + stageTable.n_SUB;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconStageBg, stageTable.s_ICON, delegate(Sprite obj)
		{
			m_background.gameObject.SetActive(false);
			if (obj != null)
			{
				m_background.gameObject.SetActive(true);
				m_background.sprite = obj;
			}
		});
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageTable.n_ID, out value))
		{
			int starAmount = ManagedSingleton<StageHelper>.Instance.GetStarAmount(value.netStageInfo.Star);
			SetStarNum(starAmount);
			if (starAmount >= 3)
			{
				SetStatus(STAGE_STATUS.PERFECT);
			}
			else
			{
				SetStatus(STAGE_STATUS.OPEN);
			}
			return;
		}
		SetStarNum(0);
		if (stageTable.s_PRE == "null")
		{
			SetStatus(STAGE_STATUS.OPEN);
			return;
		}
		SetStatus(STAGE_STATUS.LOCKED);
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(int.Parse(stageTable.s_PRE), out value) && value.netStageInfo.ClearCount > 0)
		{
			SetStatus(STAGE_STATUS.OPEN);
		}
	}

	private void SetStarNum(int num)
	{
		StarClearComponent component = GetComponent<StarClearComponent>();
		if ((bool)component)
		{
			component.SetActiveStar(num);
		}
	}

	public void SetStatus(STAGE_STATUS status)
	{
		m_cleared.gameObject.SetActive(false);
		m_newStage.gameObject.SetActive(false);
		m_red.gameObject.SetActive(false);
		m_starsRoot.gameObject.SetActive(status != STAGE_STATUS.LOCKED);
		m_background.gameObject.SetActive(status != STAGE_STATUS.LOCKED);
		switch (status)
		{
		case STAGE_STATUS.OPEN:
			m_cleared.gameObject.SetActive(true);
			break;
		case STAGE_STATUS.NEWSTAGE:
			PlayNewStageEffect();
			break;
		case STAGE_STATUS.PERFECT:
			m_cleared.gameObject.SetActive(true);
			break;
		}
		m_stageStatus = status;
	}

	public STAGE_STATUS GetStatus()
	{
		return m_stageStatus;
	}

	public bool IsUnlocked()
	{
		return m_stageStatus != STAGE_STATUS.LOCKED;
	}

	public void PlayEnableEffect(float waitTime = 0f)
	{
		StartCoroutine(EffectPlayback(waitTime));
	}

	private void PlayNewStageEffect(float time = 1f)
	{
		int loopCount = -1;
		float num = 1.8f;
		m_newStage.gameObject.SetActive(true);
		if (m_glowObject == null)
		{
			m_glowObject = UnityEngine.Object.Instantiate(m_newStage, m_newStage.transform.parent);
		}
		m_glowObject.gameObject.SetActive(true);
		m_glowObject.color = Color.white;
		m_glowObject.transform.localScale = Vector3.one;
		m_glowObject.transform.SetSiblingIndex(m_newStage.transform.GetSiblingIndex() + 1);
		m_tweenIdList.Add(LeanTween.value(1f, -0.5f, time).setOnUpdate(delegate(float val)
		{
			m_glowObject.color = new Color(1f, 1f, 1f, val);
		}).setLoopCount(loopCount)
			.uniqueId);
			m_tweenIdList.Add(LeanTween.scale(m_glowObject.gameObject, new Vector3(num, num, 1f), time).setLoopCount(loopCount).setOnComplete((Action)delegate
			{
				m_glowObject.gameObject.SetActive(false);
			})
				.uniqueId);
			}

			private IEnumerator EffectPlayback(float waitTime)
			{
				float fadeInTime = 0.2f;
				CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
				canvasGroup.alpha = 0f;
				yield return new WaitForSeconds(waitTime);
				m_tweenIdList.Add(LeanTween.value(0f, 1f, fadeInTime).setOnUpdate(delegate(float val)
				{
					canvasGroup.alpha = val;
				}).uniqueId);
				yield return null;
			}

			public void OnClickStage()
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChallengePopup", delegate(UI_ChallengePopup uiChallenge)
				{
					uiChallenge.Setup(m_stageTable);
					uiChallenge.closeCB = delegate
					{
					};
				});
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

			private void OnDestroy()
			{
				StopTweenAnim();
			}
		}
