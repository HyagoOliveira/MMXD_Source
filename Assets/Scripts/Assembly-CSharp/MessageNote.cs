#define RELEASE
using System;
using System.Collections;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class MessageNote : ScrollIndexCallback
{
	[Header("Area Array")]
	[SerializeField]
	private GameObject[] _UseLR;

	[SerializeField]
	private GameObject[] _UseMsgLR;

	[SerializeField]
	private GameObject[] _UseEmotionLR;

	[SerializeField]
	private OrangeChatText[] _MessageTextLR;

	[SerializeField]
	private Image[] _EmotionImgLR;

	[SerializeField]
	private OrangeText[] _DateTimeLR;

	[SerializeField]
	private OrangeText _UserName;

	[SerializeField]
	private Image _UserImage;

	[SerializeField]
	private Button _UserIcon;

	[Header("System Message")]
	[SerializeField]
	private GameObject _UseSystem;

	[SerializeField]
	private OrangeText _SystemName;

	[SerializeField]
	private OrangeText _SystemTime;

	[SerializeField]
	private OrangeChatText _SystemMsg;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	[SerializeField]
	private GameObject _guildObject;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private Text _guildName;

	[SerializeField]
	private GuildPrivilegeHelper _guildPrivilege;

	private ChannelUI parentChannelUI;

	private SocketChatLogInfo chatInfo;

	private MessageStruct outMsg;

	private ChatChannel ch;

	private int m_idx;

	private int nShow;

	private string targetImageName;

	public void SetPlayerSignIcon(int n_ID = 0, bool bOwner = false)
	{
		if (PlayerSignRoot != null && SignObject != null)
		{
			int childCount = PlayerSignRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(PlayerSignRoot.transform.GetChild(i).gameObject);
			}
			if (n_ID > 0)
			{
				GameObject obj = UnityEngine.Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(PlayerSignRoot);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<CommonSignBase>().SetupSign(n_ID, bOwner);
			}
		}
	}

	public void Setup(string username, string msg)
	{
	}

	public void OnClickCharacterIcon()
	{
		Debug.Log("OnClickCharacterIcon");
		RectTransform component = GetComponent<RectTransform>();
		parentChannelUI.ShowSubMenu(chatInfo.PlayerID, outMsg.NickName, component.anchoredPosition);
	}

	private void SetMsg()
	{
		if (ch != ChatChannel.SystemChannel)
		{
			ChatChannel ch2 = ch;
			int num = 7;
		}
	}

	private void SetEmotion()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ResetCell()
	{
		int num = 0;
		_UseLR[num].SetActive(false);
		_UseMsgLR[num].SetActive(false);
		_UseEmotionLR[num].SetActive(false);
		num = 1;
		_UseLR[num].SetActive(false);
		_UseMsgLR[num].SetActive(false);
		_UseEmotionLR[num].SetActive(false);
		_UseSystem.SetActive(false);
	}

	public override void BackToPool()
	{
		ResetCell();
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (parentChannelUI == null)
		{
			parentChannelUI = GetComponentInParent<ChannelUI>();
		}
		ch = parentChannelUI.OnGetCurrentChannel();
		m_idx = Math.Abs(p_idx);
		chatInfo = parentChannelUI.SocketChatLogCache[m_idx];
		if (!JsonHelper.TryDeserialize<MessageStruct>(chatInfo.MessageInfo, out outMsg))
		{
			Debug.LogError("Failed to Deserialize MessageStruct from string [" + chatInfo.MessageInfo + "]");
			return;
		}
		nShow = ((chatInfo.PlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) ? 1 : 0);
		string message = ((outMsg == null) ? string.Empty : _SystemMsg.ProgLanguage(outMsg.RichText));
		if (chatInfo.PlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			_guildObject.SetActive(false);
		}
		else if (!GuildUIHelper.SetCommunitySocketGuildInfo(chatInfo.PlayerID, _guildBadge, _guildName, _guildPrivilege))
		{
			_guildObject.SetActive(false);
		}
		else
		{
			_guildObject.SetActive(true);
		}
		float preferredHeight;
		if (ch == ChatChannel.SystemChannel)
		{
			_UseSystem.SetActive(true);
			_SystemTime.text = chatInfo.UpdateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
			_SystemMsg.alignByGeometry = false;
			_SystemMsg.text = message;
			preferredHeight = _SystemMsg.preferredHeight;
			RectTransform component = GetComponent<RectTransform>();
			preferredHeight = preferredHeight + 8f + 50f;
			component.sizeDelta = new Vector2(component.sizeDelta.x, preferredHeight);
			return;
		}
		_UseLR[nShow].SetActive(true);
		OrangeDataReader.Instance.BlurChatMessage(ref message);
		_DateTimeLR[nShow].text = chatInfo.UpdateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
		if (outMsg.EmotionPkgID > 0 && outMsg.EmotionIconID > 0)
		{
			int emotionIconID = outMsg.EmotionIconID;
			string assetName = string.Format(AssetBundleScriptableObject.Instance.m_chat_emotion_icon_format, outMsg.EmotionPkgID, emotionIconID);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetEmotionPkgBundle(outMsg.EmotionPkgID), assetName, delegate(Sprite obj)
			{
				if ((bool)obj)
				{
					_EmotionImgLR[nShow].sprite = obj;
				}
			});
			_UseEmotionLR[nShow].SetActive(true);
			preferredHeight = 250f;
		}
		else
		{
			_UseMsgLR[nShow].SetActive(true);
			_MessageTextLR[nShow].alignByGeometry = false;
			_MessageTextLR[nShow].text = message;
			Canvas.ForceUpdateCanvases();
			preferredHeight = _MessageTextLR[nShow].preferredHeight;
			RectTransform component2 = _UseMsgLR[nShow].GetComponent<RectTransform>();
			component2.sizeDelta = new Vector2(component2.sizeDelta.x, preferredHeight);
			preferredHeight += 50f;
		}
		if (nShow == 0)
		{
			_UserName.text = outMsg.NickName;
			setIcon("icon_ch_000_000");
			if (!chatInfo.RequestedHUD)
			{
				chatInfo.RequestedHUD = true;
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUD(chatInfo.PlayerID));
			}
			StartCoroutine(CheckoutImageFileName());
			if (preferredHeight < 175f)
			{
				preferredHeight = 175f;
			}
		}
		RectTransform component3 = GetComponent<RectTransform>();
		component3.sizeDelta = new Vector2(component3.sizeDelta.x, preferredHeight);
	}

	private IEnumerator CheckoutImageFileName()
	{
		int count = 0;
		while (true)
		{
			SocketPlayerHUD value;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(chatInfo.PlayerID, out value))
			{
				int num = value.m_IconNumber;
				if (num < 900001 || !ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(num))
				{
					CHARACTER_TABLE value2;
					if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(value.m_StandbyCharID, out value2))
					{
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value2.s_ICON), "icon_" + value2.s_ICON, delegate(Sprite obj)
						{
							if ((bool)obj)
							{
								_UserImage.sprite = obj;
							}
						});
						break;
					}
					num = 900001;
				}
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num];
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter(iTEM_TABLE.s_ICON), iTEM_TABLE.s_ICON, delegate(Sprite sprite)
				{
					if ((bool)sprite)
					{
						_UserImage.sprite = sprite;
					}
				});
				SetPlayerSignIcon(value.m_TitleNumber);
				break;
			}
			int num2 = count + 1;
			count = num2;
			if (num2 <= 3)
			{
				yield return new WaitForSeconds(1f);
				continue;
			}
			break;
		}
	}

	public void setIcon(string filename)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter(filename), filename, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				_UserImage.sprite = obj;
			}
		});
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
