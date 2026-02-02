using System.Collections;
using CallbackDefs;
using UnityEngine;

public class EquipGetPopup : OrangeUIBase
{
	[SerializeField]
	private EquipIcon m_equipIcon;

	[SerializeField]
	private OrangeText m_dialogText;

	[SerializeField]
	private Transform m_defenseBar;

	[SerializeField]
	private Transform m_lifeBar;

	[SerializeField]
	private Transform m_luckBar;

	[SerializeField]
	private OrangeText m_defenseNum;

	[SerializeField]
	private OrangeText m_lifeNum;

	[SerializeField]
	private OrangeText m_luckNum;

	[SerializeField]
	private StarClearComponent m_starRootDefense;

	[SerializeField]
	private StarClearComponent m_starRootLife;

	[SerializeField]
	private StarClearComponent m_starRootLuck;

	[SerializeField]
	private Transform m_naviPos;

	private Callback m_closeCallback;

	protected NAVI_MENU resultVoice;

	public void Setup(NetEquipmentInfo equipmentInfo, Callback closeCallback = null)
	{
		m_closeCallback = closeCallback;
		if (m_naviPos.GetComponentInChildren<StandNaviDb>() == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_2"), "ch_navi_2_db", delegate(GameObject obj)
			{
				StandNaviDb component = Object.Instantiate(obj, m_naviPos, false).GetComponent<StandNaviDb>();
				if ((bool)component)
				{
					component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
				}
			});
		}
		SetEquipInfo(equipmentInfo);
		StartCoroutine(PlayVoice());
	}

	public void OnClickScreen()
	{
		m_closeCallback.CheckTargetToInvoke();
		OnClickCloseBtn();
	}

	private void SetEquipInfo(NetEquipmentInfo equipmentInfo)
	{
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(equipmentInfo.EquipItemID, out equip))
		{
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(equipmentInfo);
			m_equipIcon.SetStarAndLv(equipRank[3], equip.n_LV);
			m_equipIcon.SetRare(equip.n_RARE);
			m_equipIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON);
			m_defenseNum.text = string.Format("{0}", equipmentInfo.DefParam);
			m_lifeNum.text = string.Format("{0}", equipmentInfo.HpParam);
			m_luckNum.text = string.Format("{0}", equipmentInfo.LukParam);
			float x = (float)(equipmentInfo.DefParam - equip.n_DEF_MIN) / (float)(equip.n_DEF_MAX - equip.n_DEF_MIN);
			float x2 = (float)(equipmentInfo.HpParam - equip.n_HP_MIN) / (float)(equip.n_HP_MAX - equip.n_HP_MIN);
			float x3 = (float)(equipmentInfo.LukParam - equip.n_LUK_MIN) / (float)(equip.n_LUK_MAX - equip.n_LUK_MIN);
			m_defenseBar.localScale = new Vector3(x, 1f, 1f);
			m_lifeBar.localScale = new Vector3(x2, 1f, 1f);
			m_luckBar.localScale = new Vector3(x3, 1f, 1f);
			m_starRootDefense.SetActiveStar(equipRank[0]);
			m_starRootLife.SetActiveStar(equipRank[1]);
			m_starRootLuck.SetActiveStar(equipRank[2]);
			if (equipRank[3] == 3)
			{
				m_dialogText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_MAKE_NAVI_3");
				resultVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU12;
			}
			else if (equipRank[3] == 2)
			{
				m_dialogText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_MAKE_NAVI_2");
				resultVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU29;
			}
			else if (equipRank[3] == 1)
			{
				m_dialogText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_MAKE_NAVI_1");
				resultVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU13;
			}
			else if (equipRank[3] == 0)
			{
				m_dialogText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_MAKE_NAVI_0");
				resultVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU21;
			}
		}
	}

	private IEnumerator PlayVoice()
	{
		yield return new WaitForSeconds(0.5f);
		MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", (int)resultVoice);
	}
}
