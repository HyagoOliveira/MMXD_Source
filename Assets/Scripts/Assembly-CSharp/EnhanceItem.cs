using UnityEngine;

public class EnhanceItem : MonoBehaviour
{
	private int curveOffSetIndex;

	private int curRealIndex;

	private float dCurveCenterOffset;

	private Transform mTrs;

	public bool IsVaild { get; set; }

	public EnhanceScrollView EnhanceScrollView { get; set; }

	public int CurveOffSetIndex
	{
		get
		{
			return curveOffSetIndex;
		}
		set
		{
			curveOffSetIndex = value;
		}
	}

	public int RealIndex
	{
		get
		{
			return curRealIndex;
		}
		set
		{
			curRealIndex = value;
		}
	}

	public float CenterOffSet
	{
		get
		{
			return dCurveCenterOffset;
		}
		set
		{
			dCurveCenterOffset = value;
		}
	}

	private void Awake()
	{
		IsVaild = true;
		mTrs = base.transform;
		OnAwake();
	}

	private void Start()
	{
		OnStart();
	}

	public void UpdateScrollViewItems(float xValue, float depthCurveValue, int depthFactor, float itemCount, float yValue, float scaleValue)
	{
		Vector3 one = Vector3.one;
		Vector3 one2 = Vector3.one;
		one.x = xValue;
		one.y = yValue;
		mTrs.localPosition = one;
		SetItemDepth(depthCurveValue, depthFactor, itemCount);
		one2.x = (one2.y = scaleValue);
		mTrs.localScale = one2;
	}

	protected virtual void OnClickEnhanceItem()
	{
		if (!EnhanceScrollView.IsDraging)
		{
			EnhanceScrollView.SetTargetItemIndex(this);
		}
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void SetItemDepth(float depthCurveValue, int depthFactor, float itemCount)
	{
	}

	public virtual void SetSelectState(bool isCenter)
	{
	}
}
