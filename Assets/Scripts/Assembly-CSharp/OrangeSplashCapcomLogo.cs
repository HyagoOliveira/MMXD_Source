using Coffee.UIExtensions;
using UnityEngine;

public class OrangeSplashCapcomLogo : OrangeSplash
{
	[SerializeField]
	private float triggerTime = 3000f;

	[SerializeField]
	private UIShiny shiny;

	private bool active;

	private void Update()
	{
		if (active || !splashActive || !shiny || !((float)base.Switcher.atomPlayerTime >= triggerTime))
		{
			return;
		}
		active = true;
		LeanTween.value(shiny.gameObject, 0.2f, 0.6f, 0.25f).setOnUpdate(delegate(float f)
		{
			if ((bool)shiny)
			{
				shiny.effectFactor = f;
			}
		}).setLoopPingPong(1);
	}
}
