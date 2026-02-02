using System.Collections.Generic;
using UnityEngine;

public class VisualDamageSystem : MonoBehaviour
{
	public const string DAMAGE_ITEM_NAME = "DamageText";

	private bool visible;

	private Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);

	public static string BATTLE_TEXT_MSG_CRI = string.Empty;

	public static string BATTLE_TEXT_MSG_RECOVER = string.Empty;

	public static string BATTLE_TEXT_MSG_MISS = string.Empty;

	public static string BATTLE_TEXT_MSG_DEF = string.Empty;

	public static string BATTLE_TEXT_MSG_RESIST = string.Empty;

	private List<Vector2> visualDamageList = new List<Vector2>();

	private int nowTime;

	private int removeTime;

	private readonly float offset = 0.2f;

	private void Awake()
	{
		BATTLE_TEXT_MSG_CRI = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_TEXT_MSG_CRI");
		BATTLE_TEXT_MSG_RECOVER = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_TEXT_MSG_RECOVER");
		BATTLE_TEXT_MSG_MISS = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_TEXT_MSG_MISS");
		BATTLE_TEXT_MSG_DEF = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_TEXT_MSG_DEF");
		BATTLE_TEXT_MSG_RESIST = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_TEXT_MSG_RESIST");
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SETTING, UpdateSetting);
		Init();
	}

	private void Init()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<VisualDamage>("prefab/damagetext", "DamageText", 30, delegate
		{
			visible = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.DmgVisible == 1;
			UpdateDmgVisible();
		});
	}

	private void OnDisable()
	{
		if (visible)
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<Vector2, int, LayerMask, VisualDamage.DamageType>(EventManager.ID.SHOW_DAMAGE, ShowDamage);
		}
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SETTING, UpdateSetting);
	}

	private void ShowDamage(Vector2 p_point, int p_value, LayerMask layerMask, VisualDamage.DamageType damageType)
	{
		if (visualDamageList.Contains(p_point))
		{
			p_point -= new Vector2(0f, offset);
			while (visualDamageList.Contains(p_point))
			{
				p_point -= new Vector2(0f, offset);
			}
		}
		nowTime = 0;
		removeTime = 1;
		visualDamageList.Add(p_point);
		VisualDamage poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<VisualDamage>("DamageText");
		poolObj.transform.localScale = scale;
		poolObj.Setup(p_point, p_value, layerMask, damageType);
	}

	private void Update()
	{
		if (visualDamageList.Count > 0)
		{
			if (nowTime > removeTime)
			{
				visualDamageList.Clear();
			}
			nowTime++;
		}
	}

	public void UpdateSetting()
	{
		bool flag = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.DmgVisible == 1;
		if (visible != flag)
		{
			visible = flag;
			UpdateDmgVisible();
		}
	}

	private void UpdateDmgVisible()
	{
		if (visible)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<Vector2, int, LayerMask, VisualDamage.DamageType>(EventManager.ID.SHOW_DAMAGE, ShowDamage);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<Vector2, int, LayerMask, VisualDamage.DamageType>(EventManager.ID.SHOW_DAMAGE, ShowDamage);
		}
	}
}
