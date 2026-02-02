using Unity.Mathematics;

namespace MagicaCloth
{
	public struct CurveParam
	{
		public float sval;

		public float eval;

		public float cval;

		public int useCurve;

		public CurveParam(float value)
		{
			useCurve = 0;
			sval = value;
			eval = value;
			cval = 0f;
		}

		public CurveParam(float svalue, float evalue)
		{
			useCurve = 0;
			sval = svalue;
			eval = evalue;
			cval = 0f;
		}

		public CurveParam(BezierParam bezier)
		{
			useCurve = 0;
			sval = 0f;
			eval = 0f;
			cval = 0f;
			Setup(bezier);
		}

		public void Setup(BezierParam bezier)
		{
			useCurve = (bezier.UseCurve ? 1 : 0);
			sval = bezier.StartValue;
			eval = bezier.EndValue;
			cval = math.lerp(eval, sval, math.saturate(bezier.CurveValue * 0.5f + 0.5f));
		}

		public float Evaluate(float t)
		{
			t = math.saturate(t);
			if (useCurve == 1)
			{
				float num = 1f - t;
				return num * num * sval + 2f * num * t * cval + t * t * eval;
			}
			return math.lerp(sval, eval, t);
		}
	}
}
