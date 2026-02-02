#define RELEASE
using System.Collections.Generic;
using UnityEngine;

namespace Curve
{
	public class CurveTester : MonoBehaviour
	{
		[SerializeField]
		protected CurveType type;

		[SerializeField]
		protected List<Vector3> points;

		[SerializeField]
		protected float unit = 0.1f;

		[SerializeField]
		protected bool point = true;

		[SerializeField]
		protected bool tangent = true;

		[SerializeField]
		protected bool frame;

		protected Curve curve;

		protected List<FrenetFrame> frames;

		[SerializeField]
		private GameObject target;

		private int nowIdx;

		public List<Vector3> Points
		{
			get
			{
				return points;
			}
		}

		public bool IsStop { get; set; }

		private void OnEnable()
		{
			if (points.Count <= 0)
			{
				points = new List<Vector3>
				{
					Vector3.zero,
					Vector3.up,
					Vector3.right
				};
			}
		}

		private void Start()
		{
			Setup();
		}

		public void MoveNetxt()
		{
			if (!IsStop)
			{
				nowIdx++;
				if (nowIdx >= Points.Count)
				{
					nowIdx = 0;
				}
				LeanTween.value(target, 0f, 1f, 30f).setOnUpdate(delegate(float val)
				{
					target.transform.position = curve.GetPointAt(val);
					target.transform.rotation = Quaternion.LookRotation(curve.GetTangentAt(val));
				}).setOnComplete(MoveNetxt);
			}
		}

		public void AddPoint(Vector3 p)
		{
			points.Add(p);
		}

		public void Setup()
		{
			curve = Build();
			int segments = Mathf.FloorToInt(100f);
			frames = curve.ComputeFrenetFrames(segments, false);
		}

		public Curve Build()
		{
			Curve result = null;
			if (type == CurveType.CatmullRom)
			{
				result = new CatmullRomCurve(points);
			}
			else
			{
				Debug.LogWarning("CurveType is not defined.");
			}
			return result;
		}

		private void OnDrawGizmos()
		{
			if (curve == null)
			{
				Setup();
			}
			DrawGizmos();
		}

		private void DrawGizmos()
		{
			float num = unit * 2f;
			int num2 = Mathf.FloorToInt(100f);
			if (frames == null)
			{
				frames = curve.ComputeFrenetFrames(num2, false);
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			for (int i = 0; i < num2; i++)
			{
				float u = (float)i * 0.01f;
				Vector3 pointAt = curve.GetPointAt(u);
				if (point)
				{
					Gizmos.color = Color.white;
					Gizmos.DrawSphere(pointAt, unit);
				}
				if (tangent)
				{
					Vector3 tangentAt = curve.GetTangentAt(u);
					Vector3 vector = (tangentAt + Vector3.one) * 0.5f;
					Gizmos.color = new Color(vector.x, vector.y, vector.z);
					Gizmos.DrawLine(pointAt, pointAt + tangentAt * num);
				}
				if (frame)
				{
					FrenetFrame frenetFrame = frames[i];
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(pointAt, pointAt + frenetFrame.Tangent * num);
					Gizmos.color = Color.green;
					Gizmos.DrawLine(pointAt, pointAt + frenetFrame.Normal * num);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(pointAt, pointAt + frenetFrame.Binormal * num);
				}
			}
		}
	}
}
