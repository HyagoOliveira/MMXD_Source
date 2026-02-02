using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class MissionSubButton : MonoBehaviour
{
	public enum ImageType
	{
		MAIN = 0,
		METAL = 1
	}

	[SerializeField]
	public Button button;

	[SerializeField]
	public Text text;

	[SerializeField]
	public Text title;

	[SerializeField]
	public Image image;

	[SerializeField]
	public Image hint;

	private string m_bundleName = "texture/2d/ui/ui_missionach";

	public MissionSubType type;

	public Action<MissionSubType> clickCb;

	public void SetImage(ImageType type, string fileName)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(m_bundleName, fileName, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				if (type == ImageType.MAIN)
				{
					button.GetComponent<Image>().sprite = obj;
				}
				else
				{
					image.sprite = obj;
				}
			}
		});
	}

	public void OnClick()
	{
		if (clickCb != null)
		{
			clickCb(type);
		}
	}

	public void UpdateContent()
	{
		int activityValue = ManagedSingleton<MissionHelper>.Instance.GetActivityValue(MissionType.Achievement, type);
		int subTypeMissionTotalActivity = ManagedSingleton<MissionHelper>.Instance.GetSubTypeMissionTotalActivity(type);
		text.text = activityValue.ToString();
		if (subTypeMissionTotalActivity != 0)
		{
			int num = 1;
			float num2 = (float)activityValue / (float)subTypeMissionTotalActivity;
			if (num2 >= (float)OrangeConst.ACHIEVEMENT_STEP1 / 100f)
			{
				num++;
			}
			if (num2 >= (float)OrangeConst.ACHIEVEMENT_STEP2 / 100f)
			{
				num++;
			}
			string fileName = string.Format("UI_missionACH_ICON_0{0}", num);
			SetImage(ImageType.METAL, fileName);
		}
		hint.gameObject.SetActive(ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionType.Achievement, (int)type));
	}
}
