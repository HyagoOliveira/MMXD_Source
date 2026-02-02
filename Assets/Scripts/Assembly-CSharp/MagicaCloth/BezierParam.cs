using System;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class BezierParam : IDataHash
	{
		[SerializeField]
		private float startValue;

		[SerializeField]
		private float endValue;

		[SerializeField]
		private bool useEndValue;

		[SerializeField]
		private float curveValue;

		[SerializeField]
		private bool useCurveValue;

		public float StartValue
		{
			get
			{
				return startValue;
			}
		}

		public float EndValue
		{
			get
			{
				if (!useEndValue)
				{
					return startValue;
				}
				return endValue;
			}
		}

		public float CurveValue
		{
			get
			{
				if (!useCurveValue || !useEndValue)
				{
					return 0f;
				}
				return curveValue;
			}
		}

		public bool UseCurve
		{
			get
			{
				if (useEndValue)
				{
					return useCurveValue;
				}
				return false;
			}
		}

		public BezierParam(float val)
		{
			startValue = val;
			endValue = val;
			useEndValue = false;
			curveValue = 0f;
			useCurveValue = false;
		}

		public BezierParam(float sval, float eval)
		{
			startValue = sval;
			endValue = eval;
			useEndValue = true;
			curveValue = 0f;
			useCurveValue = false;
		}

		public BezierParam(float sval, float eval, bool useEval, float cval, bool useCval)
		{
			startValue = sval;
			endValue = eval;
			useEndValue = useEval;
			curveValue = cval;
			useCurveValue = useCval;
		}

		public void SetParam(float sval, float eval, bool useEval = true, float cval = 0f, bool useCval = false)
		{
			startValue = sval;
			endValue = eval;
			useEndValue = useEval;
			curveValue = cval;
			useCurveValue = useCval;
		}

		public float Evaluate(float x)
		{
			return MathUtility.GetBezierValue(StartValue, EndValue, CurveValue, Mathf.Clamp01(x));
		}

		public BezierParam AutoSetup(float startVal, float endVal, float curveVal = 0f)
		{
			if (startVal == endVal)
			{
				SetParam(startVal, endVal, false);
			}
			else if (curveVal == 0f)
			{
				SetParam(startVal, endVal);
			}
			else
			{
				SetParam(startVal, endVal, true, Mathf.Clamp(curveVal, -1f, 1f), true);
			}
			return this;
		}

		public int GetDataHash()
		{
			return 0 + startValue.GetDataHash() + endValue.GetDataHash() + useEndValue.GetDataHash() + curveValue.GetDataHash() + useCurveValue.GetDataHash();
		}
	}
}
