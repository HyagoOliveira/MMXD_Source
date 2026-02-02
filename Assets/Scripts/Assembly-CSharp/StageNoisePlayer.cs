using UnityEngine;

public class StageNoisePlayer : MonoBehaviour
{
	[SerializeField]
	private float time = 0.35f;

	private bool isFirst = true;

	private void Awake()
	{
		isFirst = true;
	}

	private void OnEnable()
	{
		if (isFirst)
		{
			isFirst = false;
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.GetOrLoadUI("UI_StageNoise", delegate(StageNoiseUI ui)
		{
			ui.Setup(time);
		});
	}
}
