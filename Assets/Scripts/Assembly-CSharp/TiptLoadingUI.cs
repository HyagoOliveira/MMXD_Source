using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TiptLoadingUI : MonoBehaviour, ILoadingState
{
	private bool isSystemInit;

	private readonly string format = "_loading";

	[SerializeField]
	private Image imgCharacter;

	[SerializeField]
	private Text textCharacterName;

	[SerializeField]
	private Text textProgess;

	[SerializeField]
	private Text textLoadingTip;

	[SerializeField]
	private Slider sliderFill;

	private float progressResource;

	private float progressScene;

	private float progressStageRes;

	private string[] arrTipKey;

	private int tipLength;

	public bool IsComplete { get; set; }

	public object[] Params { get; set; }

	private void Awake()
	{
		IsComplete = false;
		arrTipKey = ManagedSingleton<OrangeTextDataManager>.Instance.TIP_TABLE_DICT.Keys.ToArray();
		tipLength = arrTipKey.Length;
		progressResource = 0f;
		progressScene = 0f;
		progressStageRes = 0f;
	}

	private void Start()
	{
		isSystemInit = MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE;
		Setup();
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, UpdateResourceProgress);
		Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_SCENE_PROGRESS, UpdateSceneProgress);
		Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_STAGE_RES_PROGRESS, UpdateStageResourceProgress);
		StartCoroutine(OnStartUpdateProgress());
	}

	private void OnDisable()
	{
		IsComplete = true;
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, UpdateResourceProgress);
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_SCENE_PROGRESS, UpdateSceneProgress);
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_STAGE_RES_PROGRESS, UpdateStageResourceProgress);
	}

	private void UpdateResourceProgress(float p_progress)
	{
		progressResource = Mathf.Max(progressResource, p_progress);
	}

	private void UpdateSceneProgress(float progress)
	{
		progressScene = Mathf.Max(progressScene, progress);
	}

	private void UpdateStageResourceProgress(float p_progress)
	{
		progressStageRes = Mathf.Max(progressResource, p_progress);
	}

	private void Setup()
	{
		textProgess.text = "0%";
		UpdateTip();
		CHARACTER_TABLE[] characterByLoading = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterByLoading();
		if (isSystemInit)
		{
			CHARACTER_TABLE cHARACTER_TABLE = characterByLoading[Random.Range(0, characterByLoading.Length)];
			textCharacterName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			string assetName = cHARACTER_TABLE.s_ICON + format;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_loading, assetName, delegate(Sprite spr)
			{
				if (spr != null)
				{
					imgCharacter.sprite = spr;
					imgCharacter.color = Color.white;
				}
				IsComplete = true;
			});
		}
		else
		{
			textCharacterName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterByLoading[0].w_NAME);
			imgCharacter.color = Color.white;
			IsComplete = true;
		}
	}

	public void OnClickUpdateTipBtn()
	{
		UpdateTip();
		if (isSystemInit)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		}
	}

	private void UpdateTip()
	{
		if (tipLength > 0)
		{
			string p_key = arrTipKey[Random.Range(0, tipLength)];
			textLoadingTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.TIP_TABLE_DICT.GetL10nValue(p_key);
		}
	}

	private IEnumerator OnStartUpdateProgress()
	{
		while (Params == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		float[] rate = new float[3]
		{
			(float)Params[0],
			(float)Params[1],
			(float)Params[2]
		};
		float spd = 2f * Time.deltaTime;
		while (true)
		{
			float target = progressScene * rate[0] + progressResource * rate[1] + progressStageRes * rate[2];
			float value = Mathf.MoveTowards(sliderFill.value, target, spd);
			sliderFill.value = value;
			textProgess.text = value.ToString("p");
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}
}
