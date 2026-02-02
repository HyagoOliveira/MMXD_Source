using UnityEngine;

public class FxInUILayer : FxBase
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Vector3 PosOffset = new Vector3(0f, 0f, 4f);

	[SerializeField]
	private RectTransform RectTrans;

	protected override void Awake()
	{
		base.Awake();
		if (canvas == null)
		{
			canvas = GetComponent<Canvas>();
		}
		if (RectTrans == null)
		{
			RectTrans = GetComponent<RectTransform>();
		}
	}

	public override void Active(params object[] p_params)
	{
		if (canvas.worldCamera == null)
		{
			canvas.worldCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.GetBattleGUICamera()._camera;
		}
		base.Active(p_params);
		RectTrans.position = canvas.worldCamera.transform.position + PosOffset;
	}

	private void Update()
	{
		RectTrans.position = canvas.worldCamera.transform.position + PosOffset;
	}
}
