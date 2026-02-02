#define RELEASE
using UnityEngine;

public class TestSomething : MonoBehaviour
{
	public void OnClickLoadBtn()
	{
		CreatePoolObjAndDosomeThing();
	}

	private void LoadAssets()
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { "table/achievements.json" }, delegate
		{
			Debug.Log("load ok.");
		});
	}

	private void LoadUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_Language", delegate(LanguageUI ui)
		{
			ui.Setup(null);
		});
	}

	private void PlaySound()
	{
	}

	public void CreatePoolObjAndDosomeThing()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<PoolSimple>("test/simpleobject", "simpleobject", 20, delegate
		{
			MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<PoolSimple>("simpleobject").DosomeThing();
		});
	}

	public void OnClickChangeScene(string sceneName)
	{
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene(sceneName, OrangeSceneManager.LoadingType.DEFAULT, delegate
		{
			Debug.Log(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene);
		});
	}

	public void OnClickChangeScene()
	{
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("bootup", OrangeSceneManager.LoadingType.DEFAULT, delegate
		{
			Debug.Log(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene);
		});
	}
}
