using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MessageDialogUI : MonoBehaviour
{
	[SerializeField]
	private Text _Title;

	[SerializeField]
	private Button _SendButton;

	[SerializeField]
	private Button _EmotionSelect;

	[SerializeField]
	private Text _InputMsg;

	[Header("MsgNote Template")]
	[SerializeField]
	private MessageNote _MsgNote;

	[SerializeField]
	private LoopVerticalScrollRect _ScrollView;

	public List<EMOTICONS_TABLE> iconList = new List<EMOTICONS_TABLE>();

	public int m_currentPkgId = 1;

	public int m_currentTextureCount;

	[Header("SelectEmotionSub")]
	[SerializeField]
	private GameObject selectEmotionSub;

	[SerializeField]
	private GameObject leftArrow;

	[SerializeField]
	private GameObject rightArrow;

	[SerializeField]
	private LoopHorizontalScrollRect groupScrollView;

	[SerializeField]
	private LoopHorizontalScrollRect emotionScrollView;

	private Transform gContent;

	private OrangeScrollSePlayerHorizontal scrollSePlayer;

	private float m_LastSampleTime;

	public string Title
	{
		get
		{
			return _Title.text;
		}
		set
		{
			_Title.text = value;
		}
	}

	public void Setup()
	{
	}

	public void Start()
	{
		gContent = groupScrollView.content.transform;
	}

	public void OnClickSelectEmotion()
	{
		ChannelUI componentInChildren = MonoBehaviourSingleton<UIManager>.Instance.GetComponentInChildren<ChannelUI>();
		if (componentInChildren.delaySend)
		{
			componentInChildren.ShowDelayMessage();
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		m_currentPkgId = 1;
		iconList.Clear();
		iconList = (from p in ManagedSingleton<OrangeDataManager>.Instance.EMOTICONS_TABLE_DICT
			where p.Value.n_GROUP == m_currentPkgId && p.Value.n_PRESET == 1
			select p.Value).ToList();
		selectEmotionSub.SetActive(true);
		UpdatePkgScroll();
		UpdateEmotionIcons();
		int count = iconList.Count;
		int num = 0;
	}

	private void checkShowArrow()
	{
		bool active = true;
		if (groupScrollView.totalCount <= 9)
		{
			active = false;
		}
		leftArrow.SetActive(active);
		rightArrow.SetActive(active);
	}

	public void OnColseEmotionSelect()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ColseEmotionSelect();
	}

	public void ColseEmotionSelect()
	{
		emotionScrollView.ClearCells();
		groupScrollView.ClearCells();
		selectEmotionSub.SetActive(false);
	}

	private void clearAllMessage()
	{
		_ScrollView.ClearCells();
	}

	public void SelectEmotionIcon(int iconID)
	{
		MonoBehaviourSingleton<UIManager>.Instance.GetComponentInChildren<ChannelUI>().SendEmotionMsg(m_currentPkgId, iconID);
	}

	private EmotionPkgCell FindPKG(int pid)
	{
		for (int i = 0; i < gContent.childCount; i++)
		{
			EmotionPkgCell component = gContent.GetChild(i).GetComponent<EmotionPkgCell>();
			if (component.pkgID == pid)
			{
				return component;
			}
		}
		return null;
	}

	public void SelectPkgIcon(EmotionPkgCell cell)
	{
		if (m_currentPkgId != cell.pkgID)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			EmotionPkgCell emotionPkgCell = FindPKG(m_currentPkgId);
			if (emotionPkgCell != null)
			{
				emotionPkgCell.SetCellGray(true);
			}
			cell.SetCellGray(false);
			m_currentPkgId = cell.pkgID;
			UpdateEmotionIcons();
		}
	}

	private void UpdatePkgScroll()
	{
		groupScrollView.totalCount = ManagedSingleton<OrangeDataManager>.Instance.EMOTICONS_GROUP_TABLE_DICT.Count;
		groupScrollView.RefillCells();
		checkShowArrow();
	}

	public void OnClickLeftButton()
	{
		int currentPkgId = m_currentPkgId;
		int num = 1;
	}

	public void OnClickRightButton()
	{
		int currentPkgId = m_currentPkgId;
		int totalCount = groupScrollView.totalCount;
	}

	public void UpdateEmotionIcons()
	{
		iconList.Clear();
		iconList = (from p in ManagedSingleton<OrangeDataManager>.Instance.EMOTICONS_TABLE_DICT
			where p.Value.n_GROUP == m_currentPkgId && p.Value.n_PRESET == 1
			select p.Value).ToList();
		emotionScrollView.totalCount = iconList.Count;
		emotionScrollView.RefillCells();
	}
}
