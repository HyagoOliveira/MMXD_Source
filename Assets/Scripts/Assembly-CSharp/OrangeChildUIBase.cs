using UnityEngine;

public abstract class OrangeChildUIBase : MonoBehaviour
{
	public virtual void Setup()
	{
	}

	public virtual void OpenUI()
	{
		base.gameObject.SetActive(true);
	}

	public virtual void CloseUI()
	{
		base.gameObject.SetActive(false);
	}
}
