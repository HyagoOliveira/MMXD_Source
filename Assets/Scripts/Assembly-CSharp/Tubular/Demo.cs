using Curve;
using UnityEngine;

namespace Tubular
{
	[RequireComponent(typeof(CurveTester), typeof(MeshFilter), typeof(MeshRenderer))]
	public class Demo : MonoBehaviour
	{
		[SerializeField]
		protected int tubularSegments = 20;

		[SerializeField]
		protected float radius = 0.1f;

		[SerializeField]
		protected int radialSegments = 6;

		[SerializeField]
		protected bool closed;

		private void Start()
		{
			global::Curve.Curve curve = GetComponent<CurveTester>().Build();
			GetComponent<MeshFilter>().sharedMesh = Tubular.Build(curve, tubularSegments, radius, radialSegments, closed);
		}
	}
}
