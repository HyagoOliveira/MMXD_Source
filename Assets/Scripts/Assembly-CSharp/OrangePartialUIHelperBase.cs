using UnityEngine;

[RequireComponent(typeof(OrangeUIBase))]
public abstract class OrangePartialUIHelperBase : MonoBehaviour
{
	protected OrangeUIBase _mainUI;

	protected virtual void Awake()
	{
		_mainUI = GetComponent<OrangeUIBase>();
	}
}
