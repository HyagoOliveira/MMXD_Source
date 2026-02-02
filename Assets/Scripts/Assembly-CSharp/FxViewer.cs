using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FxViewer : MonoBehaviour
{
	private readonly string textNameFormat = "Name:{0}";

	private readonly string textTotalAmountFormat = "{0}/{1}";

	private readonly string textPsAmountFormat = "Count:{0},Average:{1}";

	private readonly string textPsAmountMaxFormat = "Max:{0}";

	private readonly string textPsMemoryFormat = "Memory:{0} MB";

	private readonly string textPsTimeFormat = "Time:{0}";

	[SerializeField]
	private Text textName;

	[SerializeField]
	private Text textTotalAmount;

	[SerializeField]
	private Text textPsAmount;

	[SerializeField]
	private Text textPsAmountMax;

	[SerializeField]
	private Text textPsMemory;

	[SerializeField]
	private Text textPsTime;

	[SerializeField]
	private string[] fxPath = new string[2] { "Assets/Main/Art/Fx/ZD", "Assets/Main/Art/Fx/XAC" };

	[SerializeField]
	private string[] Ignores = new string[1] { "OBJ" };

	private GameObject nowGo;

	private ParticleSystem[] goChilds;

	private int nowIdx;

	private List<GameObject> list = new List<GameObject>();

	private int countMax;

	private float total;

	private int updateCount;

	private float time;

	private float avg;

	private Color orange = new Color(1f, 0.36078432f, 0.015686275f);

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
	}

	public void LoadFxList(string path)
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			OnClickAdd(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			OnClickAdd(1);
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			OnClickAgain();
		}
		if (goChilds != null)
		{
			bool flag = false;
			int num = 0;
			for (int i = 0; i < goChilds.Length; i++)
			{
				if (null != goChilds[i])
				{
					num += goChilds[i].particleCount;
					if (goChilds[i].particleCount > 0 && goChilds[i].time > time)
					{
						time = goChilds[i].time;
						flag = true;
					}
				}
			}
			if (flag)
			{
				updateCount++;
				total += num;
				if (time >= 1f)
				{
					avg = total / (float)updateCount;
				}
				else
				{
					avg = total / (float)updateCount;
				}
			}
			if (num >= countMax)
			{
				countMax = num;
				if (countMax >= 500)
				{
					textPsAmountMax.color = Color.red;
				}
				else if (countMax >= 100)
				{
					textPsAmountMax.color = Color.yellow;
				}
				else
				{
					textPsAmountMax.color = Color.green;
				}
			}
			textPsAmount.text = string.Format(textPsAmountFormat, num, avg.ToString("F0"));
			textPsAmountMax.text = string.Format(textPsAmountMaxFormat, countMax);
			textPsTime.text = string.Format(textPsTimeFormat, time);
		}
		else
		{
			textPsAmount.text = string.Format(textPsAmountFormat, 0, 0);
			textPsAmountMax.text = string.Format(textPsAmountMaxFormat, 0);
			textPsTime.text = string.Format(textPsTimeFormat, 0);
		}
	}

	private void UpdateIdx(int add)
	{
		DestroyTarget();
		int num = nowIdx + add;
		if (num >= list.Count)
		{
			nowIdx = 0;
		}
		else if (nowIdx + add < 0)
		{
			nowIdx = list.Count - 1;
		}
		else
		{
			nowIdx = num;
		}
		InstanceGo();
		textTotalAmount.text = string.Format(textTotalAmountFormat, nowIdx, list.Count - 1);
	}

	private void InstanceGo()
	{
		textName.text = string.Format(textNameFormat, list[nowIdx].name);
		nowGo = Object.Instantiate(list[nowIdx]);
		nowGo.name = list[nowIdx].name;
		nowGo.SetActive(true);
		goChilds = nowGo.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = goChilds;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play(false);
		}
		UpdateMemoryInfo();
	}

	private void UpdateMemoryInfo()
	{
		if (goChilds.Length >= 12)
		{
			textPsMemory.color = Color.red;
		}
		else if (goChilds.Length >= 8)
		{
			textPsMemory.color = orange;
		}
		else if (goChilds.Length >= 4)
		{
			textPsMemory.color = Color.yellow;
		}
		else
		{
			textPsMemory.color = Color.green;
		}
		textPsMemory.text = string.Format(textPsMemoryFormat, ((float)goChilds.Length * 50f / 1024f).ToString("F2"));
	}

	private void DestroyTarget()
	{
		Object.Destroy(nowGo);
		nowGo = null;
		goChilds = null;
	}

	public void OnClickAdd(int add)
	{
		UpdateIdx(add);
		countMax = 0;
		total = 0f;
		time = 0f;
		updateCount = 0;
	}

	public void OnClickAgain()
	{
		countMax = 0;
		total = 0f;
		time = 0f;
		updateCount = 0;
		goChilds = nowGo.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = goChilds;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play(false);
		}
		UpdateMemoryInfo();
	}
}
