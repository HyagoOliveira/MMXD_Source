#define RELEASE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvpBarUnit : MonoBehaviour
{
	private struct HP
	{
		public int max;

		public int now;
	}

	private PvpBarUI parent;

	[SerializeField]
	private Image imgLife;

	[SerializeField]
	private Transform imglifeParent;

	[SerializeField]
	private InvertTwoFillImg imgFillBg;

	[SerializeField]
	private InvertTwoFillImg imgFillFg;

	private int lifeMax = 3;

	private int _lifeNow = 3;

	private Image[] arrayImgLife;

	private float imgOffset = 80f;

	private float fillValue = 1f;

	private Dictionary<string, HP> dictHp = new Dictionary<string, HP>();

	private bool self;

	private int nowHp;

	private int maxHp;

	private int lifeNow
	{
		get
		{
			return _lifeNow;
		}
		set
		{
			_lifeNow = value;
			Debug.Log(base.gameObject.name + " lifeNow = " + value);
		}
	}

	public void Init(PvpBarUI p_parent, bool p_self, int p_life, int nowlife = -1)
	{
		parent = p_parent;
		self = p_self;
		lifeMax = p_life;
		if (nowlife == -1)
		{
			lifeNow = lifeMax;
		}
		else
		{
			lifeNow = nowlife;
		}
		if (arrayImgLife == null)
		{
			arrayImgLife = new Image[lifeMax];
			for (int i = 0; i < arrayImgLife.Length; i++)
			{
				Image image = Object.Instantiate(imgLife, imglifeParent, false);
				image.transform.localPosition = new Vector3((float)i * imgOffset, 0f, 0f);
				arrayImgLife[i] = image;
			}
		}
		for (int j = 0; j < arrayImgLife.Length; j++)
		{
			arrayImgLife[j].color = ((lifeNow > j) ? Color.white : Color.clear);
		}
		imgFillFg.SetFValue(1f);
		imgFillBg.SetFValue(1f);
	}

	public void SetHp(string playerID, int now, int max)
	{
		HP p_value = default(HP);
		p_value.now = now;
		p_value.max = max;
		dictHp.ContainsAdd(playerID, p_value);
	}

	private void ReducedLife()
	{
		lifeNow--;
		for (int i = 0; i < arrayImgLife.Length; i++)
		{
			arrayImgLife[i].color = ((lifeNow > i) ? Color.white : Color.clear);
		}
	}

	public void UpdateBar(StageObjBase tSOB)
	{
		OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
		if ((int)orangeCharacter.Hp <= 0)
		{
			for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count; i++)
			{
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId == orangeCharacter.sPlayerID)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nLifePercent = (lifeNow - 1) * 100 / OrangeConst.PVP_1VS1_CONTINUE;
					break;
				}
			}
			orangeCharacter.HurtActions -= UpdateBar;
			if (parent.UpdateSeasonBattleInfo(orangeCharacter))
			{
				ReducedLife();
			}
		}
		HP value = dictHp[orangeCharacter.sPlayerID];
		value.now = orangeCharacter.Hp;
		value.max = orangeCharacter.MaxHp;
		dictHp[orangeCharacter.sPlayerID] = value;
		CalculateFill();
	}

	public void CalculateFill()
	{
		nowHp = 0;
		maxHp = 0;
		foreach (KeyValuePair<string, HP> item in dictHp)
		{
			nowHp += item.Value.now;
			maxHp += item.Value.max;
		}
		if (maxHp == 0)
		{
			UpdateFill(1f);
		}
		else
		{
			UpdateFill((float)nowHp / (float)maxHp);
		}
	}

	private void UpdateFill(float p_val)
	{
		float num = Mathf.Clamp01(p_val);
		imgFillFg.SetFValue(num);
		LeanTween.value(imgFillBg.gameObject, imgFillBg.fValue, num, 0.2f).setOnUpdate(delegate(float v)
		{
			imgFillBg.SetFValue(v);
		});
	}

	public bool IsLiveHpFull()
	{
		nowHp = 0;
		maxHp = 0;
		foreach (KeyValuePair<string, HP> item in dictHp)
		{
			nowHp += item.Value.now;
			maxHp += item.Value.max;
		}
		if (nowHp == maxHp && lifeMax == lifeNow)
		{
			return true;
		}
		return false;
	}

	public int GetHpNow()
	{
		nowHp = 0;
		foreach (KeyValuePair<string, HP> item in dictHp)
		{
			nowHp += item.Value.now;
		}
		return nowHp;
	}

	public int GetBeKillLife()
	{
		return lifeMax - lifeNow;
	}

	public int GetNowLife()
	{
		return lifeNow;
	}
}
