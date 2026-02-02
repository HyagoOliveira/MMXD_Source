#define RELEASE
using System.Collections.Generic;
using UnityEngine;

public class EnhanceScrollView : MonoBehaviour
{
	public enum ScrollMode
	{
		horizontal = 0,
		vertical = 1
	}

	[SerializeField]
	private UDragEnhanceView udragEV;

	public ScrollMode scrollMode;

	public AnimationCurve scaleCurve;

	public AnimationCurve positionCurve;

	public AnimationCurve depthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	[Tooltip("The Start center index")]
	public int startCenterIndex;

	public float cellSpace = 10f;

	private float totalSpaceWidth = 500f;

	public float xyFixedPositionValue = 46f;

	public float lerpDuration = 0.2f;

	private float mCurrentDuration;

	private int mCenterIndex;

	public bool enableLerpTween = true;

	private EnhanceItem curCenterItem;

	private EnhanceItem preCenterItem;

	private bool canChangeItem = true;

	private float dFactor = 0.2f;

	private float originHorizontalValue = 0.1f;

	public float curHorizontalValue = 0.5f;

	private int depthFactor = 5;

	public List<EnhanceItem> listEnhanceItems;

	private List<EnhanceItem> listSortedItems = new List<EnhanceItem>();

	public bool InitAtStart = true;

	public float factor = 0.001f;

	public bool IsDraging { get; private set; }

	private void Start()
	{
		if (InitAtStart)
		{
			Setup();
		}
	}

	public void Setup()
	{
		preCenterItem = null;
		canChangeItem = true;
		int count = listEnhanceItems.Count;
		dFactor = (float)Mathf.RoundToInt(1f / (float)count * 10000f) * 0.0001f;
		mCenterIndex = count / 2;
		if (count % 2 == 0)
		{
			mCenterIndex = count / 2 - 1;
		}
		int num = 0;
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			listEnhanceItems[num2].CurveOffSetIndex = num2;
			listEnhanceItems[num2].CenterOffSet = dFactor * (float)(mCenterIndex - num);
			listEnhanceItems[num2].SetSelectState(false);
			listEnhanceItems[num2].EnhanceScrollView = this;
			UDragEnhanceView component = listEnhanceItems[num2].gameObject.GetComponent<UDragEnhanceView>();
			if (component != null)
			{
				component.SetScrollView(this);
			}
			num++;
		}
		if (udragEV != null)
		{
			udragEV.SetScrollView(this);
		}
		if (startCenterIndex < 0 || startCenterIndex >= count)
		{
			Debug.LogError("## startCenterIndex < 0 || startCenterIndex >= listEnhanceItems.Count  out of index ##");
			startCenterIndex = mCenterIndex;
		}
		listSortedItems = new List<EnhanceItem>(listEnhanceItems.ToArray());
		totalSpaceWidth = cellSpace * (float)count;
		curCenterItem = listEnhanceItems[startCenterIndex];
		curHorizontalValue = 0.5f - curCenterItem.CenterOffSet;
		LerpTweenToTarget(0f, curHorizontalValue);
	}

	private void LerpTweenToTarget(float originValue, float targetValue, bool needTween = false)
	{
		if (!needTween)
		{
			SortEnhanceItem();
			originHorizontalValue = targetValue;
			UpdateEnhanceScrollView(targetValue);
			OnTweenOver();
		}
		else
		{
			originHorizontalValue = originValue;
			curHorizontalValue = targetValue;
			mCurrentDuration = 0f;
		}
		enableLerpTween = needTween;
	}

	public void DisableLerpTween()
	{
		enableLerpTween = false;
	}

	public void UpdateEnhanceScrollView(float fValue)
	{
		for (int i = 0; i < listEnhanceItems.Count; i++)
		{
			EnhanceItem enhanceItem = listEnhanceItems[i];
			float xPosValue = GetXPosValue(fValue, enhanceItem.CenterOffSet);
			float scaleValue = GetScaleValue(fValue, enhanceItem.CenterOffSet);
			float depthCurveValue = depthCurve.Evaluate(fValue + enhanceItem.CenterOffSet);
			switch (scrollMode)
			{
			case ScrollMode.horizontal:
				enhanceItem.UpdateScrollViewItems(xPosValue, depthCurveValue, depthFactor, listEnhanceItems.Count, xyFixedPositionValue, scaleValue);
				break;
			case ScrollMode.vertical:
				enhanceItem.UpdateScrollViewItems(xyFixedPositionValue, depthCurveValue, depthFactor, listEnhanceItems.Count, xPosValue, scaleValue);
				break;
			}
		}
	}

	private void Update()
	{
		if (enableLerpTween)
		{
			TweenViewToTarget();
		}
	}

	private void TweenViewToTarget()
	{
		mCurrentDuration += Time.deltaTime;
		if (mCurrentDuration > lerpDuration)
		{
			mCurrentDuration = lerpDuration;
		}
		float t = mCurrentDuration / lerpDuration;
		float fValue = Mathf.Lerp(originHorizontalValue, curHorizontalValue, t);
		UpdateEnhanceScrollView(fValue);
		if (mCurrentDuration >= lerpDuration)
		{
			canChangeItem = true;
			enableLerpTween = false;
			OnTweenOver();
		}
	}

	private void OnTweenOver()
	{
		if (preCenterItem != null)
		{
			preCenterItem.SetSelectState(false);
		}
		if (curCenterItem != null)
		{
			curCenterItem.SetSelectState(true);
		}
	}

	private float GetScaleValue(float sliderValue, float added)
	{
		return scaleCurve.Evaluate(sliderValue + added);
	}

	private float GetXPosValue(float sliderValue, float added)
	{
		return positionCurve.Evaluate(sliderValue + added) * totalSpaceWidth;
	}

	private int GetMoveCurveFactorCount(EnhanceItem preCenterItem, EnhanceItem newCenterItem)
	{
		SortEnhanceItem();
		return Mathf.Abs(Mathf.Abs(newCenterItem.RealIndex) - Mathf.Abs(preCenterItem.RealIndex));
	}

	public static int SortPositionX(EnhanceItem a, EnhanceItem b)
	{
		return a.transform.localPosition.x.CompareTo(b.transform.localPosition.x);
	}

	public static int SortPositionY(EnhanceItem a, EnhanceItem b)
	{
		return a.transform.localPosition.y.CompareTo(b.transform.localPosition.y);
	}

	private void SortEnhanceItem()
	{
		if (scrollMode == ScrollMode.horizontal)
		{
			listSortedItems.Sort(SortPositionX);
		}
		else
		{
			listSortedItems.Sort(SortPositionY);
		}
		for (int num = listSortedItems.Count - 1; num >= 0; num--)
		{
			listSortedItems[num].RealIndex = num;
		}
	}

	public void SetTargetItemIndex(EnhanceItem selectItem)
	{
		if (!canChangeItem || curCenterItem == selectItem)
		{
			return;
		}
		canChangeItem = false;
		preCenterItem = curCenterItem;
		curCenterItem = selectItem;
		float num = positionCurve.Evaluate(0.5f) * totalSpaceWidth;
		bool flag = false;
		switch (scrollMode)
		{
		case ScrollMode.horizontal:
			if (selectItem.transform.localPosition.x > num)
			{
				flag = true;
			}
			break;
		case ScrollMode.vertical:
			if (selectItem.transform.localPosition.y > num)
			{
				flag = true;
			}
			break;
		}
		int moveCurveFactorCount = GetMoveCurveFactorCount(preCenterItem, selectItem);
		float num2 = 0f;
		num2 = ((!flag) ? (dFactor * (float)moveCurveFactorCount) : ((0f - dFactor) * (float)moveCurveFactorCount));
		float originValue = curHorizontalValue;
		LerpTweenToTarget(originValue, curHorizontalValue + num2, true);
	}

	public void OnBtnRightClick()
	{
		if (canChangeItem)
		{
			int num = curCenterItem.CurveOffSetIndex + 1;
			if (num > listEnhanceItems.Count - 1)
			{
				num = 0;
			}
			SetTargetItemIndex(listEnhanceItems[num]);
		}
	}

	public void OnBtnLeftClick()
	{
		if (canChangeItem)
		{
			int num = curCenterItem.CurveOffSetIndex - 1;
			if (num < 0)
			{
				num = listEnhanceItems.Count - 1;
			}
			SetTargetItemIndex(listEnhanceItems[num]);
		}
	}

	public void OnDragBegin()
	{
		IsDraging = true;
		curCenterItem.SetSelectState(false);
	}

	public void OnDragEnhanceViewMove(Vector2 delta)
	{
		switch (scrollMode)
		{
		case ScrollMode.horizontal:
			if (Mathf.Abs(delta.x) > 0f)
			{
				curHorizontalValue += delta.x * factor;
				LerpTweenToTarget(0f, curHorizontalValue);
			}
			break;
		case ScrollMode.vertical:
			if (Mathf.Abs(delta.y) > 0f)
			{
				curHorizontalValue += delta.y * factor;
				LerpTweenToTarget(0f, curHorizontalValue);
			}
			break;
		}
	}

	public void OnDragEnhanceViewEnd()
	{
		int index = 0;
		float f = curHorizontalValue - (float)(int)curHorizontalValue;
		float num = float.MaxValue;
		float num2 = 0.5f * (float)((!(curHorizontalValue < 0f)) ? 1 : (-1));
		for (int i = 0; i < listEnhanceItems.Count; i++)
		{
			float num3 = Mathf.Abs(Mathf.Abs(f) - Mathf.Abs(num2 - listEnhanceItems[i].CenterOffSet));
			if (num3 < num)
			{
				index = i;
				num = num3;
			}
		}
		originHorizontalValue = curHorizontalValue;
		preCenterItem = curCenterItem;
		if (!listEnhanceItems[index].IsVaild)
		{
			index = 0;
		}
		float targetValue = (float)(int)curHorizontalValue + (num2 - listEnhanceItems[index].CenterOffSet);
		curCenterItem = listEnhanceItems[index];
		LerpTweenToTarget(originHorizontalValue, targetValue, true);
		canChangeItem = false;
		IsDraging = false;
	}
}
