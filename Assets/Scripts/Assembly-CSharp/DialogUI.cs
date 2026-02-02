#define RELEASE
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class DialogUI : OrangeUIBase
{
	public enum LogState
	{
		LOADING = 0,
		TYPING = 1,
		IGNORE = 2,
		WAITING = 3,
		SKIP = 4
	}

	private const float eftStartValue = 0f;

	private const float eftEndValue = 1f;

	private const float eftAnimTime = 0.3f;

	public LogState logState;

	[SerializeField]
	private OrangeText textScript;

	[SerializeField]
	private OrangeText textScriptChk;

	[SerializeField]
	private Transform[] transHeads;

	[SerializeField]
	private Transform[] transSt;

	[SerializeField]
	private Image imgArrow;

	private RectTransform textScriptRt;

	private readonly int heightMax = 3;

	private Font font;

	private WaitForSeconds waitForSec;

	private Queue<SCENARIO_TABLE> scenarioQueue;

	private List<string> cutStrList = new List<string>();

	private Callback m_cb;

	private string[] nowPlaySE = new string[3]
	{
		string.Empty,
		string.Empty,
		string.Empty
	};

	private bool isSeparateCharacter;

	private string[] allowTriggerDialogKeys;

	private float dialogTimer;

	private bool bSkiped;

	private GameObject face;

	protected override void Awake()
	{
		base.Awake();
		textScript.font = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		textScriptRt = textScript.GetComponent<RectTransform>();
		if (imgArrow != null)
		{
			imgArrow.color = Color.clear;
		}
	}

	public void Setup(int idx, Callback p_cb = null)
	{
		logState = LogState.LOADING;
		m_cb = p_cb;
		font = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		Language language = LocalizationScriptableObject.Instance.m_Language;
		if ((uint)(language - 4) <= 1u)
		{
			isSeparateCharacter = true;
			waitForSec = new WaitForSeconds(0.033f);
		}
		else
		{
			isSeparateCharacter = false;
			waitForSec = new WaitForSeconds(0.05f);
		}
		scenarioQueue = new Queue<SCENARIO_TABLE>(ManagedSingleton<OrangeTableHelper>.Instance.GetScenarioGroupData(idx));
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP02);
		if (scenarioQueue.Count > 0)
		{
			LoadDialogInfo();
		}
		else
		{
			OnClickCloseBtn();
		}
		allowTriggerDialogKeys = new string[3]
		{
			ButtonId.SHOOT.ToString(),
			ButtonId.JUMP.ToString(),
			ButtonId.DASH.ToString()
		};
	}

	private void LoadDialogInfo()
	{
		imgArrow.color = Color.clear;
		textScript.alignByGeometry = false;
		cutStrList.Clear();
		string text = string.Empty;
		int fontSize = textScript.fontSize;
		FontStyle fontStyle = textScript.fontStyle;
		SCENARIO_TABLE scenario = scenarioQueue.Dequeue();
		nowPlaySE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(scenario.s_VOICE);
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SCENARIOTEXT_TABLE_DICT.GetL10nValue(scenario.w_CONTENT);
		font.RequestCharactersInTexture(l10nValue, fontSize, fontStyle);
		font.RequestCharactersInTexture(" ", fontSize, fontStyle);
		float textScriptRect = GetTextScriptRect(scenario.n_LOCATION);
		float num = 0f;
		if (isSeparateCharacter)
		{
			string[] array = new string[0];
			l10nValue = l10nValue.Replace("\n", " \n");
			array = l10nValue.Split(' ');
			float num2 = 0f;
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				text2 += " ";
				if (text2.StartsWith("\n") ? true : false)
				{
					cutStrList.Add(text + "\n");
					string text3 = text2.Replace("\n", "");
					text = text3;
					num2 = SeparateWidth(text3);
					continue;
				}
				num2 += SeparateWidth(text2);
				if (num2 <= textScriptRect)
				{
					text += text2;
					continue;
				}
				cutStrList.Add(text + "\n");
				text = text2;
				num2 = SeparateWidth(text2);
			}
		}
		else
		{
			for (int j = 0; j < l10nValue.Length; j++)
			{
				char c = l10nValue[j];
				UnityEngine.CharacterInfo info;
				font.GetCharacterInfo(c, out info, fontSize, fontStyle);
				bool flag = c == '\n';
				if (!flag && num + (float)info.advance <= textScriptRect)
				{
					num += (float)info.advance;
					text += c;
				}
				else
				{
					cutStrList.Add(text + (flag ? "" : "\n"));
					num = info.advance;
					text = c.ToString();
				}
			}
		}
		if (text.Length >= 0)
		{
			cutStrList.Add(text);
		}
		string empty = string.Empty;
		empty = ((scenario.n_HEAD_TYPE != 0) ? AssetBundleScriptableObject.Instance.m_texture_2d_stand_st : AssetBundleScriptableObject.Instance.m_texture_scenario);
		string[] array2 = scenario.s_HEAD.Split(',');
		string empty2 = string.Empty;
		int num3 = 0;
		if (array2.Length > 1)
		{
			empty2 = array2[1];
			num3 = 1;
		}
		else
		{
			empty2 = array2[0];
			num3 = 0;
		}
		if (face != null)
		{
			if (face.gameObject.name == array2[num3])
			{
				UpdateFaceState(scenario);
				StartCoroutine(OnStartTypingText());
				return;
			}
			Object.Destroy(face.gameObject);
			face = null;
		}
		if (array2[0] == "null")
		{
			StartCoroutine(OnStartTypingText());
			return;
		}
		string bundleName = string.Format(empty, array2[0]);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetGameObjectAndAsyncLoad(bundleName, empty2, delegate(GameObject obj)
		{
			if (obj != null)
			{
				face = obj;
				UpdateFaceState(scenario);
				UIAnimationHelper.Play(AnimationGroup[scenario.n_LOCATION], false, delegate
				{
					StartCoroutine(OnStartTypingText());
				});
			}
			else
			{
				StartCoroutine(OnStartTypingText());
			}
		});
	}

	private float SeparateWidth(string chkString)
	{
		textScriptChk.text = chkString;
		LayoutRebuilder.ForceRebuildLayoutImmediate(textScriptChk.rectTransform);
		return textScriptChk.preferredWidth;
	}

	private float GetTextScriptRect(int n_LOCATION)
	{
		return textScriptRt.rect.width;
	}

	private void UpdateFaceState(SCENARIO_TABLE scenario)
	{
		if (scenario.n_HEAD_TYPE == 0)
		{
			face.transform.SetParent(transHeads[scenario.n_LOCATION], false);
			int num = scenario.n_LOCATION;
			if (num == 1 && scenario.n_FLIP == 1)
			{
				num = 0;
			}
			Quaternion quaternion = ((num == 0) ? Quaternion.Euler(0f, 0f, 0f) : (quaternion = Quaternion.Euler(0f, 180f, 0f)));
			face.transform.localRotation = quaternion;
			face.GetComponent<FaceController>().UpdateState(scenario.n_EYE);
		}
		else
		{
			face.transform.SetParent(transSt[scenario.n_LOCATION], false);
		}
	}

	private IEnumerator OnStartTypingText()
	{
		int i = 0;
		int count = cutStrList.Count;
		int length = ((heightMax < count) ? heightMax : count);
		logState = LogState.TYPING;
		yield return waitForSec;
		if (bSkiped)
		{
			yield break;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Play(nowPlaySE[0], nowPlaySE[1]);
		string compStr = string.Empty;
		textScript.text = string.Empty;
		yield return waitForSec;
		for (; i < length; i++)
		{
			compStr += cutStrList[i];
			int lineLength = cutStrList[i].Length;
			for (int j = 0; j < lineLength; j++)
			{
				textScript.text += cutStrList[i][j];
				if (logState == LogState.TYPING)
				{
					yield return waitForSec;
					continue;
				}
				textScript.text = compStr;
				break;
			}
		}
		cutStrList.RemoveRange(0, length);
		MonoBehaviourSingleton<AudioManager>.Instance.Play(nowPlaySE[0], nowPlaySE[2]);
		yield return waitForSec;
		if (logState == LogState.TYPING && cutStrList.Count > 0)
		{
			textScript.text = string.Empty;
			StartCoroutine(OnStartTypingText());
		}
		else
		{
			logState = LogState.WAITING;
			imgArrow.color = Color.white;
		}
	}

	public void OnPlayerClickScreen()
	{
		switch (logState)
		{
		case LogState.TYPING:
			logState = LogState.IGNORE;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR03);
			break;
		case LogState.WAITING:
			textScript.text = string.Empty;
			if (cutStrList.Count > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR03);
				StartCoroutine(OnStartTypingText());
			}
			else if (scenarioQueue.Count > 0)
			{
				logState = LogState.LOADING;
				MonoBehaviourSingleton<AudioManager>.Instance.Stop(nowPlaySE[0]);
				LoadDialogInfo();
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR03);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL02);
				OnClickCloseBtn();
			}
			break;
		case LogState.LOADING:
		case LogState.IGNORE:
			break;
		}
	}

	public void OnClickSkipBtn()
	{
		if (logState != LogState.SKIP)
		{
			bSkiped = true;
			Debug.Log("[DialogUI] Set Skip flag = true");
			StopAllCoroutines();
			logState = LogState.SKIP;
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(nowPlaySE[0]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
			OnClickCloseBtn();
		}
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Play(nowPlaySE[0], nowPlaySE[2]);
		m_cb.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public override void DoJoystickEvent()
	{
		if (MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.isPause || MonoBehaviourSingleton<CursorController>.Instance.IsEnable)
		{
			return;
		}
		if (dialogTimer <= 0f)
		{
			for (int i = 0; i < allowTriggerDialogKeys.Length; i++)
			{
				if (cInput.GetKeyUp(allowTriggerDialogKeys[i]))
				{
					dialogTimer = 0.3f;
					OnPlayerClickScreen();
					break;
				}
			}
		}
		else
		{
			dialogTimer -= Time.deltaTime;
		}
	}
}
