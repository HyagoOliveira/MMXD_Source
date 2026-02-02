using OrangeAudio;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDemoUI : OrangeUIBase
{
	public static Color ChromaKeyColor = Color.clear;

	[SerializeField]
	private RawImage tModelImg;

	[SerializeField]
	private Image ChromaKey;

	private RenderTextureObj textureObj;

	public int characterId { get; set; } = 1;


	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CUSTOM;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(Object obj)
		{
			textureObj = Object.Instantiate((GameObject)obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			textureObj.OnlyDebut = true;
			textureObj.AssignNewRender(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterId], null, null, new Vector3(0.05f, -0.95f, 5f), tModelImg);
			ModelRotateDrag component = tModelImg.GetComponent<ModelRotateDrag>();
			if ((bool)component)
			{
				component.SetModelTransform(textureObj.RenderPosition);
			}
		});
		ChromaKey.color = ChromaKeyColor;
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARACTER_RT_VISIBLE, enable);
		}
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterDemo", delegate(CharacterDemoUI ui)
			{
				OnClickCloseBtn();
				ui.characterId = characterId;
				ui.Setup();
			});
		}
		else
		{
			if (!Input.GetKeyDown(KeyCode.E))
			{
				return;
			}
			Demo_EasterEggs componentInChildren = textureObj.GetComponentInChildren<Demo_EasterEggs>();
			if (!(componentInChildren != null) || !componentInChildren.isStart)
			{
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
			if ((bool)componentInChildren.GetComponent<CharacterAnimatonEvent>())
			{
				componentInChildren.GetComponent<CharacterAnimatonEvent>().IgnoreAnimEvents = false;
				componentInChildren.enabled = false;
				componentInChildren.isStart = false;
				if ((bool)componentInChildren.GetComponent<Animator>())
				{
					componentInChildren.GetComponent<Animator>().Play("3");
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (null != textureObj)
		{
			Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
		base.OnClickCloseBtn();
	}

	protected override void DoCustomEscapeEvent()
	{
		OnClickCloseBtn();
	}
}
