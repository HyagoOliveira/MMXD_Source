using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AgeConfirmUI : OrangeUIBase
{
	[SerializeField]
	private AgeConfirmUnit selectionUnit;

	[SerializeField]
	private EnhanceScrollView enhanceYear;

	[SerializeField]
	private EnhanceScrollView enhanceMonth;

	[SerializeField]
	private EnhanceScrollView enhanceDay;

	[SerializeField]
	private OrangeText textLimitInfo;

	private int year;

	private int month;

	private int day;

	public void Setup()
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		InitEnhanceYear();
		InitEnhanceMonth();
		InitEnhanceDay();
		textLimitInfo.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("AGE_CONFIRM_CONTENT"), OrangeConst.AGE_PAY_TIER1, OrangeConst.AGE_PAY_TIER2);
	}

	private void InitEnhanceYear()
	{
		List<AgeConfirmUnit> list = new List<AgeConfirmUnit>();
		int num = DateTime.Now.Year;
		int num2 = num - 119;
		for (int num3 = num; num3 >= num2; num3--)
		{
			AgeConfirmUnit ageConfirmUnit = UnityEngine.Object.Instantiate(selectionUnit, enhanceYear.transform);
			ageConfirmUnit.Init(num3, SetYear);
			list.Add(ageConfirmUnit);
		}
		enhanceYear.listEnhanceItems = list.ConvertAll((Converter<AgeConfirmUnit, EnhanceItem>)((AgeConfirmUnit x) => x));
		enhanceYear.Setup();
	}

	private void InitEnhanceMonth()
	{
		List<AgeConfirmUnit> list = new List<AgeConfirmUnit>();
		int num = 1;
		for (int num2 = 12; num2 >= num; num2--)
		{
			AgeConfirmUnit ageConfirmUnit = UnityEngine.Object.Instantiate(selectionUnit, enhanceMonth.transform);
			ageConfirmUnit.Init(num2, SetMonth);
			list.Add(ageConfirmUnit);
		}
		enhanceMonth.listEnhanceItems = list.ConvertAll((Converter<AgeConfirmUnit, EnhanceItem>)((AgeConfirmUnit x) => x));
		enhanceMonth.Setup();
	}

	private void InitEnhanceDay()
	{
		List<AgeConfirmUnit> list = new List<AgeConfirmUnit>();
		int num = 1;
		for (int num2 = 31; num2 >= num; num2--)
		{
			AgeConfirmUnit ageConfirmUnit = UnityEngine.Object.Instantiate(selectionUnit, enhanceDay.transform);
			ageConfirmUnit.Init(num2, SetDay);
			list.Add(ageConfirmUnit);
		}
		enhanceDay.listEnhanceItems = list.ConvertAll((Converter<AgeConfirmUnit, EnhanceItem>)((AgeConfirmUnit x) => x));
		enhanceDay.Setup();
	}

	private void SetYear(int val)
	{
		year = val;
	}

	private void SetMonth(int val)
	{
		month = val;
	}

	private void SetDay(int val)
	{
		day = val;
	}

	public void OnClickOK()
	{
		string text = year.ToString("0000") + month.ToString("00") + day.ToString("00");
		if (ValidateDateTime(text))
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Birth = text;
			base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			OnClickCloseBtn();
		}
		else
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("INVALID_DATE_FORMAT");
		}
	}

	private bool ValidateDateTime(string datetime)
	{
		if (datetime == null || datetime.Length == 0)
		{
			return false;
		}
		try
		{
			DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo();
			dateTimeFormatInfo.FullDateTimePattern = "yyyyMMdd";
			DateTime.ParseExact(datetime, "F", dateTimeFormatInfo);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
