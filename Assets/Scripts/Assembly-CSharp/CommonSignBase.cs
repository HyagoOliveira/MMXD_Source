using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;

public class CommonSignBase : MonoBehaviour
{
	public enum SignNameType
	{
		SIGN_SHOW = 1,
		SIGN_HIDE = 2
	}

	[SerializeField]
	private GameObject SignRoot;

	[SerializeField]
	private OrangeText SignText;

	[SerializeField]
	private Image SignImage;

	[SerializeField]
	private Image NewImage;

	[SerializeField]
	private Image SelectedFrameImage;

	[SerializeField]
	private Image BaseImage;

	[SerializeField]
	private Image UsedImage;

	[SerializeField]
	private Image UsedFrameImage;

	private int _id;

	private bool bShowSelected;

	private PlayerCustomizeUI parentPlayerCustomizeUI;

	private void Start()
	{
	}

	private void Update()
	{
		if (bShowSelected && parentPlayerCustomizeUI != null)
		{
			bool flag = parentPlayerCustomizeUI.GetCurrentSelectedSignID() == _id;
			SelectedFrameImage.gameObject.SetActive(flag);
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID != 0)
			{
				UsedFrameImage.gameObject.SetActive(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID == _id && !flag);
			}
		}
	}

	public void Setup(int SignID, bool bShow = false, bool bShowFrame = false)
	{
		int key = 0;
		CUSTOMIZE_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.TryGetValue(SignID, out value))
		{
			key = value.n_GET_VALUE1;
		}
		if (value == null)
		{
			return;
		}
		if (parentPlayerCustomizeUI == null)
		{
			parentPlayerCustomizeUI = GetComponentInParent<PlayerCustomizeUI>();
		}
		_id = SignID;
		bShowSelected = bShowFrame;
		if (parentPlayerCustomizeUI == null)
		{
			return;
		}
		bool flag = parentPlayerCustomizeUI.CheckSignFlag(SignID);
		SignRoot.SetActive(flag);
		BaseImage.gameObject.SetActive(!flag);
		if (!flag)
		{
			return;
		}
		bool active = SignID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID && bShow;
		UsedImage.gameObject.SetActive(active);
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(key))
		{
			bool flag2 = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashSignShowNewHint.Contains(SignID);
			NewImage.gameObject.SetActive(!flag2 && bShow);
		}
		ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[key];
		if (iTEM_TABLE.n_TYPE_X == 1)
		{
			SignText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(iTEM_TABLE.w_NAME);
		}
		else
		{
			SignText.text = "";
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSign(value.s_ICON), value.s_ICON, delegate(Sprite sprite)
		{
			if ((bool)sprite)
			{
				SignImage.sprite = sprite;
			}
		});
	}

	public void SetupSign(int SignID, bool bOwner = false)
	{
		_id = SignID;
		if (bOwner)
		{
			CancelInvoke();
			InvokeRepeating("UpdateSign", 2f, 1f);
		}
		int key = 0;
		CUSTOMIZE_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.TryGetValue(SignID, out value))
		{
			key = value.n_GET_VALUE1;
		}
		if (value == null)
		{
			SignRoot.SetActive(false);
			BaseImage.gameObject.SetActive(SignID > 0 || bOwner);
		}
		else
		{
			if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(key) || SignID <= 0)
			{
				return;
			}
			SignRoot.SetActive(true);
			BaseImage.gameObject.SetActive(false);
			NewImage.gameObject.SetActive(false);
			UsedImage.gameObject.SetActive(false);
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[key];
			if (iTEM_TABLE.n_TYPE_X == 1)
			{
				SignText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(iTEM_TABLE.w_NAME);
			}
			else
			{
				SignText.text = "";
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSign(value.s_ICON), value.s_ICON, delegate(Sprite sprite)
			{
				if ((bool)sprite)
				{
					SignImage.sprite = sprite;
				}
			});
		}
	}

	public void SetupSignFromHUD(string PlayerID)
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUD, delegate(object res)
		{
			if (res is RSGetPlayerHUD)
			{
				SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(((RSGetPlayerHUD)res).PlayerHUD);
				if (socketPlayerHUD != null)
				{
					ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, socketPlayerHUD);
					if (socketPlayerHUD.m_TitleNumber > 0)
					{
						SetupSign(socketPlayerHUD.m_TitleNumber);
					}
					else
					{
						BaseImage.gameObject.SetActive(false);
					}
				}
				else
				{
					BaseImage.gameObject.SetActive(false);
				}
			}
		}, 0, true);
		SignRoot.SetActive(false);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUD(PlayerID));
	}

	public void UpdateSign()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID != _id)
		{
			SetupSign(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID);
		}
	}

	public void OnClick()
	{
		if (parentPlayerCustomizeUI != null)
		{
			parentPlayerCustomizeUI.SetCurrentSelectedSignID(_id);
		}
	}
}
