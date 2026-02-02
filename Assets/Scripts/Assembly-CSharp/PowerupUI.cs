using DragonBones;
using UnityEngine;

public class PowerupUI : OrangeUIBase, IManagedUpdateBehavior
{
	private const int MAX_DIGITS = 7;

	private readonly string animationName = "newAnimation";

	private readonly string event_update_number = "update_number";

	private readonly string event_update_number2 = "update_number2";

	private string[] slotLBnbName = new string[7] { "LBnb1", "LBnb2", "LBnb3", "LBnb4", "LBnb5", "LBnb6", "LBnb7" };

	private string[] slotYnbName = new string[7] { "Ynb1", "Ynb11", "Ynb3", "Ynb4", "Ynb5", "Ynb6", "Ynb7" };

	[SerializeField]
	private UnityArmatureComponent armatureComp;

	private Slot[] LBnb;

	private Slot[] Ynb;

	private int newValue;

	private int addValue;

	private int i;

	public void Play(int p_newValue, int p_addValue)
	{
		newValue = p_newValue;
		addValue = p_addValue;
		int num = slotLBnbName.Length;
		LBnb = new Slot[num];
		Ynb = new Slot[num];
		for (int i = 0; i < num; i++)
		{
			LBnb[i] = armatureComp._armature.GetSlot(slotLBnbName[i]);
			Ynb[i] = armatureComp._armature.GetSlot(slotYnbName[num - 1 - i]);
		}
		armatureComp.AddEventListener("frameEvent", FrameEvent);
		armatureComp.AddEventListener("complete", PlayComplete);
		armatureComp.animation.Play(animationName, 1);
		if ((!MonoBehaviourSingleton<UIManager>.Instance.IsLoading || MonoBehaviourSingleton<UIManager>.Instance.ForcePowerUPSE) && MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene == "hometop")
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW01);
			MonoBehaviourSingleton<UIManager>.Instance.ForcePowerUPSE = false;
		}
	}

	private void FrameEvent(string type, EventObject e)
	{
		if (e.name == event_update_number)
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		}
		else if (e.name == event_update_number2)
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			SetCurrentPower();
		}
	}

	private void PlayComplete(string type, EventObject eventObject)
	{
		armatureComp.RemoveEventListener("complete", PlayComplete);
		OnClickCloseBtn();
	}

	private void SetCurrentPower()
	{
		int[] array = ToSpecialArray(newValue);
		int[] array2 = ToSpecialArray(addValue, false);
		for (int i = 0; i < LBnb.Length; i++)
		{
			LBnb[i].displayIndex = array[i];
			Ynb[i].displayIndex = array2[LBnb.Length - 1 - i];
		}
	}

	private int[] ToSpecialArray(int num, bool allowZero = true)
	{
		num = Mathf.Abs(num);
		int[] array = new int[7];
		int num2 = 0;
		if (allowZero)
		{
			num2 = num.ToString().PadLeft(7, '0').Length;
		}
		else
		{
			num2 = num.ToString().Length;
			for (int num3 = 6; num3 >= num2; num3--)
			{
				array[num3] = -1;
			}
		}
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			array[num4] = 9 - num % 10;
			num /= 10;
		}
		return array;
	}

	public void UpdateFunc()
	{
		for (i = 0; i < LBnb.Length; i++)
		{
			LBnb[i].displayIndex = Random.Range(0, 10);
		}
	}
}
