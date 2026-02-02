using UnityEngine.UI;

public class OrangeText : Text
{
	public bool IsLocalizationFont;

	public bool IsLocalizationText;

	public bool IsNonBreakingSpaceText;

	public static readonly string no_breaking_space = "\u00a0";

	public string LocalizationKey;

	protected override void Awake()
	{
		UpdateTextImmediate();
		base.alignByGeometry = IsLocalizationFont || IsLocalizationText;
		base.Awake();
		RegisterDirtyVerticesCallback(VertDirtyAction);
	}

	private void VertDirtyAction()
	{
		if (IsNonBreakingSpaceText && text.Contains(" "))
		{
			text = text.Replace(" ", no_breaking_space);
		}
	}

	public void UpdateTextImmediate()
	{
		if (IsLocalizationText && LocalizationKey != string.Empty)
		{
			text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(LocalizationKey);
			base.font = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		}
		else if (IsLocalizationFont)
		{
			base.font = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		}
	}

	protected override void OnDisable()
	{
		UnregisterDirtyVerticesCallback(VertDirtyAction);
		base.OnDisable();
	}
}
