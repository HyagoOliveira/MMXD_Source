using System.Collections;
using DragonBones;
using UnityEngine;
using UnityEngine.UI;

public class Performance : MonoBehaviour
{
	public UnityDragonBonesData dragonBoneData;

	public Text text;

	private void Start()
	{
		UnityFactory.factory.LoadData(dragonBoneData);
		StartCoroutine(BuildArmatureComponent());
	}

	private IEnumerator BuildArmatureComponent()
	{
		int lY = 20;
		int lX = 20;
		int index = 0;
		int y = 0;
		while (y < lY)
		{
			int num;
			for (int x = 0; x < lX; x = num)
			{
				Vector3 localPosition = new Vector3(((float)x * 10f / (float)lX - 5f) * 1f, ((float)y * 10f / (float)lY - 5f) * 1f, (float)x + (float)(lX * y) * 0.01f);
				GameObject gameObject = new GameObject("mecha_1406");
				UnityArmatureComponent unityArmatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1406", "", "", "", gameObject);
				unityArmatureComponent.armature.cacheFrameRate = 24u;
				unityArmatureComponent.animation.Play("walk");
				unityArmatureComponent.transform.localPosition = localPosition;
				unityArmatureComponent.transform.localScale = Vector3.one * 0.5f;
				yield return new WaitForSecondsRealtime(0.1f);
				Text obj = text;
				num = index + 1;
				index = num;
				obj.text = "Count:" + num;
				num = x + 1;
			}
			num = y + 1;
			y = num;
		}
	}
}
