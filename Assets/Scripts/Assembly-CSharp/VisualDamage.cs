using System;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class VisualDamage : PoolBaseObject
{
	public enum DamageType
	{
		Normal = 0,
		Cri = 1,
		Recover = 2,
		Reduce = 3,
		Miss = 4,
		Resist = 5
	}

	private const float TWEEN_START_VALUE = 0f;

	private const float TWEEN_END_VALUE = 4.5f;

	private const float TWEEN_TIME = 0.5f;

	private const float TWEEN_COLOR_TIME = 0.3f;

	private const float TWEEN_CONST_VALUE = 1f;

	private const float TWEEN_CONST_VALUE_2 = 1.5f;

	private const float TWEEN_DELAY = 0.2f;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Text text;

	[SerializeField]
	private Text textTitle;

	[SerializeField]
	private UIGradient[] uiGradient;

	private Vector2 point = Vector2.zero;

	private float tweenValue = 1f;

	private int tweenUid1 = -1;

	private int tweenUid2 = -1;

	private Color white = Color.white;

	private Color red = new Color(49f / 51f, 11f / 85f, 0.09019608f);

	private Color textColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color[] Color_CRI;

	[SerializeField]
	private Color[] Color_RECOVER;

	[SerializeField]
	private Color[] Color_MISS;

	[SerializeField]
	private Color[] Color_DEF;

	[SerializeField]
	private Color[] Color_RESIST;

	public void Awake()
	{
	}

	public void Setup(Vector2 p_point, int p_value, LayerMask layerMask, DamageType damageType)
	{
		if (canvas.worldCamera == null)
		{
			canvas.worldCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.GetBattleGUICamera()._camera;
		}
		point = p_point;
		base.transform.SetPositionAndRotation(point, Quaternion.identity);
		if (p_value < 0)
		{
			p_value = 0;
		}
		text.text = p_value.ToString();
		UpdateColor(ref layerMask, ref damageType);
		tweenUid1 = LeanTween.value(1f, 0f, 0.3f).setOnUpdate(delegate(float f)
		{
			text.color = new Color(1f, 1f, 1f, f);
			textTitle.color = new Color(1f, 1f, 1f, f);
		}).setDelay(0.2f)
			.setOnComplete((Action)delegate
			{
				tweenUid1 = -1;
			})
			.uniqueId;
		tweenUid2 = LeanTween.value(0f, 4.5f, 0.5f).setOnUpdate(delegate(float f)
		{
			tweenValue = 1f - f / 4.5f;
			base.transform.position = new Vector3(point.x, 1.5f + point.y - tweenValue, 0.01f * f);
		}).setOnComplete((Action)delegate
		{
			tweenUid2 = -1;
			BackToPool();
		})
			.uniqueId;
	}

	public void UpdateColor(ref LayerMask layerMask, ref DamageType damageType)
	{
		bool flag = ((int)layerMask & (int)BulletScriptableObject.Instance.BulletLayerMaskPlayer) != 0;
		switch (damageType)
		{
		case DamageType.Normal:
			if (flag)
			{
				UIGradient[] array = uiGradient;
				foreach (UIGradient obj6 in array)
				{
					obj6.color1 = red;
					obj6.color2 = red;
				}
			}
			else
			{
				UIGradient[] array = uiGradient;
				foreach (UIGradient obj7 in array)
				{
					obj7.color1 = white;
					obj7.color2 = white;
				}
			}
			textTitle.text = string.Empty;
			break;
		case DamageType.Cri:
			if (flag)
			{
				UIGradient[] array = uiGradient;
				foreach (UIGradient obj3 in array)
				{
					obj3.color1 = red;
					obj3.color2 = red;
				}
			}
			else
			{
				UIGradient[] array = uiGradient;
				foreach (UIGradient obj4 in array)
				{
					obj4.color1 = Color_CRI[0];
					obj4.color2 = Color_CRI[1];
				}
			}
			textTitle.text = VisualDamageSystem.BATTLE_TEXT_MSG_CRI;
			break;
		case DamageType.Recover:
		{
			UIGradient[] array = uiGradient;
			foreach (UIGradient obj8 in array)
			{
				obj8.color1 = Color_RECOVER[0];
				obj8.color2 = Color_RECOVER[1];
			}
			textTitle.text = VisualDamageSystem.BATTLE_TEXT_MSG_RECOVER;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySE(base.transform, "BattleSE", "bt_hp02");
			break;
		}
		case DamageType.Reduce:
		{
			UIGradient[] array = uiGradient;
			foreach (UIGradient obj2 in array)
			{
				obj2.color1 = Color_DEF[0];
				obj2.color2 = Color_DEF[1];
			}
			textTitle.text = VisualDamageSystem.BATTLE_TEXT_MSG_DEF;
			break;
		}
		case DamageType.Miss:
		{
			UIGradient[] array = uiGradient;
			foreach (UIGradient obj5 in array)
			{
				obj5.color1 = Color_MISS[0];
				obj5.color2 = Color_MISS[1];
			}
			textTitle.text = VisualDamageSystem.BATTLE_TEXT_MSG_MISS;
			break;
		}
		case DamageType.Resist:
		{
			UIGradient[] array = uiGradient;
			foreach (UIGradient obj in array)
			{
				obj.color1 = Color_RESIST[0];
				obj.color2 = Color_RESIST[1];
			}
			text.text = string.Empty;
			textTitle.text = VisualDamageSystem.BATTLE_TEXT_MSG_RESIST;
			break;
		}
		}
		text.color = white;
		textTitle.color = white;
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, "DamageText");
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref tweenUid1);
		LeanTween.cancel(ref tweenUid2);
	}
}
