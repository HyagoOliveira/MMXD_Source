using CallbackDefs;
using DragonBones;
using UnityEngine;

public class StandNaviDb : MonoBehaviour
{
	public enum NAVI_DB_TYPE
	{
		NORMAL = 0,
		GLASS = 1
	}

	public const int MIN_ANIMATION_ID = 1;

	public const int MAX_ANIMATION_ID = 5;

	[SerializeField]
	private UnityArmatureComponent armatureComp;

	[SerializeField]
	private GameObject[] objNormalNeedOff;

	[SerializeField]
	private NonDrawingGraphic nonDrawingGraphic;

	private int nowPlayingId = -1;

	private Callback touchCb;

	public void Setup(NAVI_DB_TYPE type, int defaultId = 0, Callback touchCB = null)
	{
		touchCb = touchCB;
		nonDrawingGraphic.raycastTarget = touchCB != null;
		UpdateType(type);
		Play(defaultId);
	}

	public void UpdateType(NAVI_DB_TYPE type)
	{
		if (type == NAVI_DB_TYPE.NORMAL || type != NAVI_DB_TYPE.GLASS)
		{
			GameObject[] array = objNormalNeedOff;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
		}
		else
		{
			GameObject[] array = objNormalNeedOff;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
		}
	}

	public void Play(int animationID, int loop = 0)
	{
		if (nowPlayingId != animationID && armatureComp.animation != null)
		{
			nowPlayingId = animationID;
			armatureComp.animation.Play(GetAnimationName(animationID), loop);
		}
	}

	private string GetAnimationName(int idx)
	{
		if (armatureComp.animation == null)
		{
			return null;
		}
		if (armatureComp.animation.animationNames.Count <= idx)
		{
			return null;
		}
		return armatureComp.animation.animationNames[idx];
	}

	public void OnClickThis()
	{
		touchCb.CheckTargetToInvoke();
	}
}
