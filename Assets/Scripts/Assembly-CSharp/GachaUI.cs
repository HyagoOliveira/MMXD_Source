#define RELEASE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CallbackDefs;
using KtxUnity;
using NaughtyAttributes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GachaUI : OrangeUIBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadBasis_003Ed__33 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public byte[] p_param;

		public string textureName;

		private NativeArray<byte> _003Cna_003E5__2;

		private TaskAwaiter<TextureResult> _003C_003Eu__1;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			try
			{
				TaskAwaiter<TextureResult> awaiter;
				if (num != 0)
				{
					byte[] array = p_param;
					_003Cna_003E5__2 = new NativeArray<byte>(array, Allocator.Persistent);
					awaiter = new BasisUniversalTexture().LoadFromBytes(_003Cna_003E5__2, MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance).GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter;
						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
				}
				else
				{
					awaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(TaskAwaiter<TextureResult>);
					num = (_003C_003E1__state = -1);
				}
				TextureResult result = awaiter.GetResult();
				if (result.errorCode == ErrorCode.Success)
				{
					MonoBehaviourSingleton<LocalizationManager>.Instance.dictCacheTexture[textureName] = result.texture;
				}
				_003Cna_003E5__2.Dispose();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const int GACHA_TUTORIAL_ID = 1;

	private const int GACHA_TYPE = 1;

	private readonly string nullStr = "null";

	[SerializeField]
	private Transform unitParent;

	[SerializeField]
	private GachaBtnUnit unit;

	[SerializeField]
	private L10nRawImage Fg;

	[SerializeField]
	private GameObject leftArrow;

	[SerializeField]
	private GameObject rightArrow;

	[SerializeField]
	private Image imgDateBg;

	[SerializeField]
	private OrangeText textDate;

	[SerializeField]
	private Transform uiPageDotParent;

	[SerializeField]
	private GachaUIPageDot uiPageDot;

	[SerializeField]
	private RectTransform content;

	[BoxGroup("Ceiling")]
	[SerializeField]
	private Canvas GachaCeilingObj;

	[BoxGroup("Ceiling")]
	[SerializeField]
	private OrangeText textGachaAmount;

	[BoxGroup("Ceiling")]
	[SerializeField]
	private GameObject GachaCeilingMaxObj;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_shopSE;

	private List<GACHALIST_TABLE> gachaListAll = new List<GACHALIST_TABLE>();

	private Dictionary<int, List<GACHALIST_TABLE>> dictGachaList;

	private List<int> listGroupKey = new List<int>();

	private List<GachaBtnUnit> listGachaBtn = new List<GachaBtnUnit>();

	private List<GachaUIPageDot> listGachaUIPageDot = new List<GachaUIPageDot>();

	private bool goNext = true;

	private int maxIdx;

	private static int nowIdx;

	private OrangeL10nRawBg bg;

	private int gachaGroupId = -1;

	private List<string> listPreloadImg = new List<string>();

	private int UpdatePageRetryCount;

	private List<GACHALIST_TABLE> nowGachaList = new List<GACHALIST_TABLE>();

	private List<int> preList = new List<int>();

	private int nowAdd = 1;

	private float offsetLast = 250f;

	private float offsetNext = -250f;

	private Vector2 offset = new Vector2(0f, 0f);

	private List<GACHA_TABLE> listExchange;

	public void SetupByGachaGroupId(int p_groupId)
	{
		gachaGroupId = p_groupId;
		Setup();
	}

	public void Setup(Callback p_cb = null)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 35);
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveGachaRecordReq(delegate
		{
			if (IsTutorial())
			{
				return;
			}
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			gachaListAll = ManagedSingleton<ExtendDataHelper>.Instance.GetGachaListTableByOepening(serverUnixTimeNowUTC, 1);
			gachaListAll.Remove(ManagedSingleton<ExtendDataHelper>.Instance.GACHALIST_TABLE_DICT[1]);
			gachaListAll.RemoveAll((GACHALIST_TABLE x) => CheckLimitOnce(x));
			IEnumerable<GACHALIST_TABLE> enumerable = gachaListAll.Where((GACHALIST_TABLE x) => x.s_IMG != "null").Distinct((GACHALIST_TABLE x) => x.s_IMG);
			int preloadCount = enumerable.Count() * 2;
			string[] array = new string[2];
			foreach (GACHALIST_TABLE item in enumerable)
			{
				array[0] = item.s_IMG;
				array[1] = item.s_IMG + "_BG";
				string[] array2 = array;
				foreach (string textureName in array2)
				{
					if (MonoBehaviourSingleton<LocalizationManager>.Instance.dictCacheTexture.ContainsKey(textureName))
					{
						preloadCount--;
						if (preloadCount <= 0)
						{
							PreloadDataComplete(p_cb);
						}
					}
					else
					{
						MonoBehaviourSingleton<LocalizationManager>.Instance.GetL10nRawImage(L10nRawImage.ImageType.Texture, OrangeWebRequestLoad.LoadType.BASIS_L10N_TEXTURE, true, textureName, delegate(byte[] p_param0, string p_param1)
						{
							preloadCount--;
							SavePreloadBasisData(textureName, p_param0);
							if (preloadCount <= 0)
							{
								PreloadDataComplete(p_cb);
							}
						});
					}
				}
			}
		});
	}

	public void Start()
	{
		closeCB = (Callback)Delegate.Combine(closeCB, (Callback)delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		});
	}

	private void SavePreloadData(string textureName, byte[] p_param)
	{
		if (p_param != null)
		{
			Texture2D texture2D = new Texture2D(1, 1);
			if (texture2D.LoadImage(p_param, true))
			{
				listPreloadImg.Add(textureName);
				MonoBehaviourSingleton<LocalizationManager>.Instance.dictCacheTexture[textureName] = texture2D;
			}
		}
	}

	private void SavePreloadBasisData(string textureName, byte[] p_param)
	{
		try
		{
			if (p_param != null)
			{
				LoadBasis(textureName, p_param);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Unknow exception problem " + ex.Message);
		}
	}

	[AsyncStateMachine(typeof(_003CLoadBasis_003Ed__33))]
	private void LoadBasis(string textureName, byte[] p_param)
	{
		_003CLoadBasis_003Ed__33 stateMachine = default(_003CLoadBasis_003Ed__33);
		stateMachine.textureName = textureName;
		stateMachine.p_param = p_param;
		stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
		stateMachine._003C_003E1__state = -1;
		AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
		_003C_003Et__builder.Start(ref stateMachine);
	}

	private void PreloadDataComplete(Callback p_cb = null)
	{
		dictGachaList = (from x in gachaListAll
			orderby x.n_GROUP
			group x by x.n_GROUP).ToDictionary((IGrouping<int, GACHALIST_TABLE> x) => x.Key, (IGrouping<int, GACHALIST_TABLE> x) => x.ToList());
		listGroupKey = dictGachaList.Keys.ToList();
		maxIdx = listGroupKey.Count - 1;
		if (gachaGroupId > 0)
		{
			nowIdx = 0;
			for (int i = 0; i < listGroupKey.Count; i++)
			{
				if (listGroupKey[i] == gachaGroupId)
				{
					nowIdx = i;
					break;
				}
			}
		}
		for (int j = 0; j < listGroupKey.Count; j++)
		{
			GachaUIPageDot gachaUIPageDot = UnityEngine.Object.Instantiate(uiPageDot, uiPageDotParent);
			gachaUIPageDot.Setup(j);
			listGachaUIPageDot.Add(gachaUIPageDot);
		}
		UpdatePage(nowIdx);
		if (maxIdx == 0)
		{
			leftArrow.SetActive(false);
			rightArrow.SetActive(false);
		}
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(p_cb);
	}

	private bool IsTutorial(Callback p_cb = null)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicGacha.Count == 0)
		{
			gachaListAll = new List<GACHALIST_TABLE>();
			gachaListAll.Add(ManagedSingleton<ExtendDataHelper>.Instance.GACHALIST_TABLE_DICT[1]);
			dictGachaList = (from x in gachaListAll
				group x by x.n_GROUP).ToDictionary((IGrouping<int, GACHALIST_TABLE> x) => x.Key, (IGrouping<int, GACHALIST_TABLE> x) => x.ToList());
			listGroupKey = dictGachaList.Keys.ToList();
			maxIdx = listGroupKey.Count - 1;
			UpdatePage(0);
			if (maxIdx == 0)
			{
				leftArrow.SetActive(false);
				rightArrow.SetActive(false);
			}
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(p_cb);
			return true;
		}
		return false;
	}

	private void UpdatePage(int goPage)
	{
		if (!goNext)
		{
			return;
		}
		goNext = false;
		if (goPage > maxIdx)
		{
			nowIdx = 0;
		}
		else if (goPage < 0)
		{
			nowIdx = maxIdx;
		}
		else
		{
			nowIdx = goPage;
		}
		foreach (GachaUIPageDot item in listGachaUIPageDot)
		{
			item.SetNowPage(nowIdx);
		}
		ClearOldUnit();
		int key = listGroupKey[nowIdx];
		nowGachaList.Clear();
		bool flag = false;
		string s_IMG = nullStr;
		if (dictGachaList.TryGetValue(key, out nowGachaList))
		{
			preList.Clear();
			if (nowGachaList.Count > 0)
			{
				if (nowGachaList[0].s_BEGIN_TIME == "null")
				{
					imgDateBg.color = Color.clear;
					textDate.text = string.Empty;
				}
				else
				{
					imgDateBg.color = Color.white;
					textDate.text = OrangeGameUtility.DisplayDatePeriod(nowGachaList[0].s_BEGIN_TIME, nowGachaList[0].s_END_TIME);
				}
			}
			nowGachaList = (from x in nowGachaList
				orderby x.n_SORT, x.n_ID
				select x).ToList();
			foreach (GACHALIST_TABLE nowGacha in nowGachaList)
			{
				if (nowGacha.s_IMG != nullStr)
				{
					s_IMG = nowGacha.s_IMG;
					flag = true;
				}
				if (CheckPre(nowGacha) && !CheckLimit(nowGacha))
				{
					GachaBtnUnit gachaBtnUnit = UnityEngine.Object.Instantiate(unit, base.transform);
					gachaBtnUnit.Setup(nowGacha);
					listGachaBtn.Add(gachaBtnUnit);
				}
			}
			if (listGachaBtn.Count == 0 && UpdatePageRetryCount < 100)
			{
				goNext = true;
				UpdatePageRetryCount++;
				OnClickArrowBtn(nowAdd);
				return;
			}
			UpdatePageRetryCount = 0;
			foreach (GachaBtnUnit item2 in listGachaBtn)
			{
				if (preList.Contains(item2.gachaListTable.n_ID))
				{
					item2.gameObject.SetActive(false);
				}
				else
				{
					item2.transform.SetParent(unitParent);
				}
			}
			UpdateCeilingInfo();
		}
		if (flag)
		{
			Fg.Init(L10nRawImage.ImageType.Texture, s_IMG, delegate
			{
				goNext = true;
			}, L10nRawImage.ImageEffect.Fade);
			bg = (OrangeL10nRawBg)Background;
			bg.UpdateImg(s_IMG + "_BG", L10nRawImage.ImageEffect.Fade);
		}
		else
		{
			goNext = true;
		}
	}

	private bool CheckPre(GACHALIST_TABLE p_table)
	{
		if (p_table.n_PRE == 0)
		{
			return true;
		}
		GACHALIST_TABLE value = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.GACHALIST_TABLE_DICT.TryGetValue(p_table.n_PRE, out value) && CheckLimit(value))
		{
			preList.Add(p_table.n_PRE);
			return true;
		}
		return false;
	}

	private bool CheckLimit(GACHALIST_TABLE p_table)
	{
		if (p_table.n_LIMIT == 0)
		{
			return false;
		}
		GachaInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicGacha.TryGetValue(p_table.n_ID, out value) && value.netGachaEventRecord != null)
		{
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(value.netGachaEventRecord.LastDrawTime, (ResetRule)p_table.n_RESET_RULE))
			{
				return false;
			}
			if (p_table.n_LIMIT <= value.netGachaEventRecord.DrawCount)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckLimitOnce(GACHALIST_TABLE p_table)
	{
		if (p_table.n_LIMIT == 0)
		{
			return false;
		}
		if (ManagedSingleton<ExtendDataHelper>.Instance.IsCeilingExist(p_table.n_GROUP))
		{
			return false;
		}
		GachaInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicGacha.TryGetValue(p_table.n_ID, out value) && value.netGachaEventRecord != null && (short)p_table.n_RESET_RULE == 0 && p_table.n_LIMIT <= value.netGachaEventRecord.DrawCount)
		{
			return true;
		}
		return false;
	}

	private void ClearOldUnit()
	{
		foreach (GachaBtnUnit item in listGachaBtn)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		listGachaBtn.Clear();
	}

	public void OnClickArrowBtn(int add)
	{
		nowAdd = add;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		UpdatePage(nowIdx + add);
	}

	public override void OnClickCloseBtn()
	{
		if (!goNext)
		{
			return;
		}
		foreach (string item in listPreloadImg)
		{
			MonoBehaviourSingleton<LocalizationManager>.Instance.ClearSingleTextureCache(item);
		}
		base.OnClickCloseBtn();
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	public void RefreashGacha()
	{
		UpdatePage(nowIdx);
	}

	public void OnShopClick()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_shopSE);
			ui.Setup(ShopTopUI.ShopSelectTab.item_shop);
		});
	}

	public void OnClickGharaInfo()
	{
		if (goNext && nowGachaList.Count >= 1)
		{
			GACHALIST_TABLE gACHALIST_TABLE = nowGachaList[0];
			string urlLanguage = MonoBehaviourSingleton<LocalizationManager>.Instance.GetUrlLanguage();
			string text = AesCrypto.Encode(gACHALIST_TABLE.n_GROUP.ToString());
			Debug.Log(text.ToString());
			text = Uri.EscapeDataString(text);
			Debug.Log(text.ToString());
			string url = string.Format("{0}/notice/gacha/{1}/{2}", ManagedSingleton<ServerConfig>.Instance.ServerSetting.Notice, urlLanguage, text);
			CtcWebView webView = null;
			CtcWebView.Create<CtcWebView>(out webView, url);
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SHOP, RefreashGacha);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SHOP, RefreashGacha);
	}

	public void OnScrollChange()
	{
		if (goNext)
		{
			offset = content.offsetMin;
			if (offset.x > offsetLast)
			{
				OnClickArrowBtn(-1);
			}
			else if (offset.x < offsetNext)
			{
				OnClickArrowBtn(1);
			}
		}
	}

	public void UpdateCeilingInfo()
	{
		if (nowGachaList.Count >= 1)
		{
			listExchange = null;
			if (!ManagedSingleton<ExtendDataHelper>.Instance.IsCeilingExist(nowGachaList[0].n_GACHAID_1, out listExchange))
			{
				GachaCeilingObj.enabled = false;
				return;
			}
			GachaCeilingObj.enabled = true;
			UpdateDrawCount();
		}
	}

	public void OnClickOpenGachaCeilingUI()
	{
		if (nowGachaList.Count > 0 && listExchange != null && listExchange.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GachaCeiling", delegate(GachaCeilingUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.Setup(nowGachaList, listExchange);
			});
		}
	}

	private void UpdateDrawCount()
	{
		int gachaDrawCount = ManagedSingleton<ExtendDataHelper>.Instance.GetGachaDrawCount(nowGachaList);
		textGachaAmount.text = gachaDrawCount + "/" + OrangeConst.GACHA_SELECT_MAX;
		GachaCeilingMaxObj.gameObject.SetActive((gachaDrawCount >= OrangeConst.GACHA_SELECT_MAX) ? true : false);
	}
}
