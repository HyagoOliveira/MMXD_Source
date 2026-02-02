using NaughtyAttributes;
using UnityEngine;

public class OrangeScrollSePlayer : MonoBehaviour
{
	[BoxGroup("Sound")]
	[SerializeField]
	protected SystemSE cueId = SystemSE.CRI_SYSTEMSE_SYS_SCROLL02;

	[SerializeField]
	protected OrangeUIBase parentUI;

	[SerializeField]
	protected float distance = 100f;

	protected float nowDistance;

	protected float lastPosDir;

	[SerializeField]
	[Tooltip("第一次出現不撥放")]
	protected bool isFirstInit = true;

	protected bool isParentExist;

	private void Awake()
	{
		isParentExist = parentUI != null;
	}

	public void Start()
	{
		nowDistance = GetDirection();
	}

	public void Update()
	{
		float direction = GetDirection();
		if (direction == 0f)
		{
			nowDistance = 0f;
			lastPosDir = GetDirection();
			return;
		}
		float num = Mathf.Abs(direction - lastPosDir);
		if (nowDistance + num > distance)
		{
			if (!isFirstInit)
			{
				if (isParentExist && !parentUI.IsVisible)
				{
					nowDistance = 0f;
					return;
				}
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
		lastPosDir = direction;
	}

	protected virtual float GetDirection()
	{
		return base.transform.localPosition.y;
	}

	public void ResetDis()
	{
		lastPosDir = GetDirection();
		nowDistance = 0f;
	}
}
