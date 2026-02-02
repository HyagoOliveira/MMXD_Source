using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DebugDevAudioText : MonoBehaviour
{
	private int n_maxList = 100;

	private Text textPlaying;

	[SerializeField]
	private List<string> listPlaying = new List<string>();

	private int showCnt;

	private List<string> listFilterStr = new List<string>();

	private void Awake()
	{
		textPlaying = GetComponent<Text>();
		textPlaying.text = string.Empty;
		listPlaying.Clear();
		RemoveOld();
	}

	private void RemoveOld()
	{
		LeanTween.value(base.gameObject, 0f, 1f, 2f).setOnComplete((Action)delegate
		{
			if (showCnt > 0)
			{
				showCnt--;
				UpdateText();
			}
			RemoveOld();
		});
	}

	public void PushPlayingSE(string str)
	{
	}

	private void UpdateText()
	{
		textPlaying.text = string.Empty;
		int num = 0;
		int num2 = listPlaying.Count - 1;
		while (num2 >= 0 && num < showCnt)
		{
			string[] array = Regex.Split(listPlaying[num2], "_Vol:", RegexOptions.IgnoreCase);
			if (array.Length > 1)
			{
				array[1] = "</color><color=#00ff00>:v:" + array[1];
				listPlaying[num2] = array[0] + array[1];
			}
			bool flag = listFilterStr.Count == 0;
			for (int i = 0; i < listFilterStr.Count; i++)
			{
				if (listPlaying[num2].StartsWith(listFilterStr[i]))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (listPlaying[num2].StartsWith("ht"))
				{
					Text text = textPlaying;
					text.text = text.text + "<color=#ff0000>" + listPlaying[num2] + "</color>\n";
				}
				else if (listPlaying[num2].StartsWith("sys"))
				{
					Text text2 = textPlaying;
					text2.text = text2.text + "<color=#eeee00>" + listPlaying[num2] + "</color>\n";
				}
				else if (listPlaying[num2].StartsWith("wep"))
				{
					Text text3 = textPlaying;
					text3.text = text3.text + "<color=#5cacee>" + listPlaying[num2] + "</color>\n";
				}
				else if (listPlaying[num2].StartsWith("em"))
				{
					Text text4 = textPlaying;
					text4.text = text4.text + "<color=#f4a460>" + listPlaying[num2] + "</color>\n";
				}
				else if (listPlaying[num2].StartsWith("bgm"))
				{
					Text text5 = textPlaying;
					text5.text = text5.text + "<color=#a500cc>" + listPlaying[num2] + "</color>\n";
				}
				else
				{
					Text text6 = textPlaying;
					text6.text = text6.text + "<color=#00ff00>" + listPlaying[num2] + "</color>\n";
				}
				num++;
			}
			num2--;
		}
	}

	public void OnAudioFilterValChange(InputField input)
	{
	}
}
