using NaughtyAttributes;
using UnityEngine;

public class OrangeEHScrollSePlayer : MonoBehaviour
{
	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE cueId = SystemSE.CRI_SYSTEMSE_SYS_SCROLL01;

	[SerializeField]
	protected float distance = 0.08f;

	private EnhanceScrollView enhanceScrollView;

	private float nowDistance;

	private float lastPosY;

	private bool isFirstInit = true;

	public void Start()
	{
		enhanceScrollView = base.gameObject.GetComponent<EnhanceScrollView>();
		nowDistance = enhanceScrollView.curHorizontalValue;
	}

	public void Update()
	{
		float curHorizontalValue = enhanceScrollView.curHorizontalValue;
		if (curHorizontalValue == 0f)
		{
			nowDistance = 0f;
			return;
		}
		float num = Mathf.Abs(curHorizontalValue - lastPosY);
		if (nowDistance + num > distance)
		{
			if (!isFirstInit)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cueId);
			}
			else
			{
				isFirstInit = false;
			}
			nowDistance = 0f;
		}
		else
		{
			nowDistance += num;
		}
		lastPosY = curHorizontalValue;
	}

	public void ResetDis()
	{
		lastPosY = enhanceScrollView.curHorizontalValue;
		nowDistance = 0f;
	}
}
