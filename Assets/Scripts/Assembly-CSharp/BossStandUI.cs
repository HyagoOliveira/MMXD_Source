using UnityEngine;

public class BossStandUI : OrangeUIBase
{
	[SerializeField]
	private Transform rootBossImage;

	public void Setup(string p_bossIntro)
	{
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(p_bossIntro))
		{
			string text = "St_Enemy_" + p_bossIntro;
			string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, text);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetGameObjectAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, text), text, delegate(GameObject obj)
			{
				rootBossImage.transform.localScale = new Vector3(0.77f, 0.77f, 1f);
				obj.GetComponent<StandBase>().Setup(rootBossImage);
			});
		}
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}
}
