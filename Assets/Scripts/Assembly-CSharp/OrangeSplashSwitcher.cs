#define RELEASE
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using CriWare;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using enums;

public class OrangeSplashSwitcher : MonoBehaviour
{
	[SerializeField]
	private CriWareInitializer criWareInitializer;

	private CriAtomExPlayer player;

	[SerializeField]
	private Button nextBtn;

	private Image nextBtnImg;

	private float logoVolume;

	private OrangeSplash[] OrangeTitle;

	[SerializeField]
	private GameObject aspectRatioController;

	private int i;

	public long atomPlayerTime
	{
		get
		{
			if (player == null)
			{
				return 0L;
			}
			return player.GetTime();
		}
	}

	[DllImport("user32.dll")]
	public static extern bool SetWindowText(IntPtr hwnd, string lpString);

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string className, string windowName);

	private void Awake()
	{
		OrangeConst.Reader = OrangeDataReader.Instance;
		OrangeDataManager.Reader = OrangeDataReader.Instance;
		OrangeTextDataManager.Reader = OrangeDataReader.Instance;
		IntPtr hwnd = FindWindow(null, Application.productName);
		SetWindowText(hwnd, "ROCKMAN X DiVE");
		if (aspectRatioController != null)
		{
			aspectRatioController.gameObject.SetActive(true);
			UnityEngine.Object.DontDestroyOnLoad(aspectRatioController);
		}
		MonoBehaviourSingleton<OrangeSDKManager>.Instance.Init(delegate
		{
			DeviceHelper.GetProxy();
			string text = Language.Unknown.ToString();
			string @string = PlayerPrefs.GetString("ORANGE_L10N_KEY_LANGUAGE", text);
			if (text == @string)
			{
				LocalizationScriptableObject.Instance.m_Language = MonoBehaviourSingleton<LocalizationManager>.Instance.GetGameLanguageBySystem();
			}
			else
			{
				LocalizationScriptableObject.Instance.m_Language = (Language)Enum.Parse(typeof(Language), @string);
			}
			OrangeDataReader.Instance.ReadTextDataLocal();
			MonoBehaviourSingleton<LocalizationManager>.Instance.LoadOrangeTextTable();
			nextBtnImg = nextBtn.GetComponent<Image>();
			nextBtn.interactable = false;
			OrangeTitle = GetComponentsInChildren<OrangeSplash>();
		});
		LeanTween.init();
	}

	private void Start()
	{
		CriWareInitializer[] array = UnityEngine.Object.FindObjectsOfType<CriWareInitializer>();
		criWareInitializer = array[0];
		StartCoroutine(OnStartSplash());
	}

	private void OnApplicationFocus(bool focus)
	{
		if (player != null)
		{
			if (Application.isFocused)
			{
				player.SetVolume(logoVolume);
			}
			else
			{
				player.SetVolume(0f);
			}
			player.UpdateAll();
		}
	}

	private IEnumerator OnStartSplash()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		while (OrangeTitle == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (i < OrangeTitle.Length)
		{
			OrangeTitle[i].Switcher = this;
			OrangeTitle[i].SetSplashParam();
			yield return CoroutineDefine._waitForEndOfFrame;
			nextBtn.interactable = true;
			yield return OrangeTitle[i].Active();
			i++;
		}
		nextBtn.interactable = false;
		nextBtnImg.raycastTarget = false;
		yield return CoroutineDefine._waitForEndOfFrame;
		SceneManager.LoadScene("bootup");
	}

	public void OnClickGoNextSplash()
	{
		OrangeTitle[i].JumpToNext();
	}

	public IEnumerator OnCriWareInitializer()
	{
		Debug.Log("[OrangeSplashSwitcher]:CriWareInitializer");
		criWareInitializer.Initialize();
		while (!CriAtomPlugin.IsLibraryInitialized())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		Debug.Log("[OrangeSplashSwitcher]:CriWareInitializer OK");
		player = new CriAtomExPlayer(256, 1);
	}

	public void PlayAudio(string fileName)
	{
		logoVolume = PlayerPrefs.GetFloat("ORANGE_SETTING_SE_VOLUME", 1f);
		player.SetVolume(logoVolume);
		player.SetFile(null, Path.Combine(Common.streamingAssetsPath, string.Format("hca/{0}.hca", fileName)));
		player.SetFormat(CriAtomEx.Format.HCA);
		player.Start();
	}

	private void OnDestroy()
	{
		if (player != null)
		{
			player.Dispose();
		}
	}
}
