using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class DrawMagazine : MonoBehaviour
{
	private class SectorMeshCreator
	{
		private float _radius;

		private float _startAngleDegree;

		private float _angleDegree;

		private Mesh _cacheMesh;

		public Mesh CreateMesh(float radius, float startAngleDegree, float angleDegree, int angleDegreePrecision, int radiusPrecision)
		{
			if (!CheckDiff(radius, startAngleDegree, angleDegree, angleDegreePrecision, radiusPrecision))
			{
				return _cacheMesh;
			}
			Mesh mesh = Create(radius, startAngleDegree, angleDegree);
			if (mesh == null)
			{
				return _cacheMesh;
			}
			_cacheMesh = mesh;
			_radius = radius;
			_startAngleDegree = startAngleDegree;
			_angleDegree = angleDegree;
			return _cacheMesh;
		}

		private Vector3 CalcPoint(float angle)
		{
			angle %= 360f;
			if (angle == 0f)
			{
				return new Vector3(1f, 0f, 0f);
			}
			if (angle == 180f)
			{
				return new Vector3(-1f, 0f, 0f);
			}
			if (angle <= 45f || angle > 315f)
			{
				return new Vector3(1f, 0f - Mathf.Tan((float)Math.PI / 180f * angle), 0f);
			}
			if (angle <= 135f)
			{
				return new Vector3(1f / Mathf.Tan((float)Math.PI / 180f * angle), -1f, 0f);
			}
			if (angle <= 225f)
			{
				return new Vector3(-1f, Mathf.Tan((float)Math.PI / 180f * angle), 0f);
			}
			return new Vector3(-1f / Mathf.Tan((float)Math.PI / 180f * angle), 1f, 0f);
		}

		private Mesh Create(float radius, float startAngleDegree, float angleDegree)
		{
			if (startAngleDegree == 360f)
			{
				startAngleDegree = 0f;
			}
			Mesh mesh = new Mesh();
			List<Vector3> list = new List<Vector3>
			{
				Vector3.zero,
				CalcPoint(startAngleDegree)
			};
			float[] array = new float[4] { 45f, 135f, 225f, 315f };
			Vector3[] array2 = new Vector3[4]
			{
				new Vector3(1f, -1f, 0f),
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 1f, 0f),
				new Vector3(1f, 1f, 0f)
			};
			for (int i = 0; i < array.Length; i++)
			{
				if (startAngleDegree < array[i] && array[i] - startAngleDegree < angleDegree)
				{
					list.Add(array2[i]);
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (startAngleDegree < array[j] + 360f && array[j] + 360f - startAngleDegree < angleDegree)
				{
					list.Add(array2[j]);
				}
			}
			list.Add(CalcPoint(startAngleDegree + angleDegree));
			Vector3[] array3 = new Vector3[list.Count];
			Vector2[] array4 = new Vector2[array3.Length];
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k] = list[k] * radius;
				array4[k] = new Vector2(list[k].x * 0.5f + 0.5f, list[k].y * 0.5f + 0.5f);
			}
			int[] array5 = new int[(array3.Length - 2) * 3];
			int num = 0;
			int num2 = 1;
			while (num < array5.Length)
			{
				array5[num] = 0;
				array5[num + 2] = num2;
				array5[num + 1] = num2 + 1;
				num += 3;
				num2++;
			}
			mesh.vertices = array3;
			mesh.triangles = array5;
			mesh.uv = array4;
			return mesh;
		}

		private bool CheckDiff(float radius, float startAngleDegree, float angleDegree, int angleDegreePrecision, int radiusPrecision)
		{
			if ((int)(startAngleDegree - _startAngleDegree) == 0 && (int)((angleDegree - _angleDegree) * (float)angleDegreePrecision) == 0)
			{
				return (int)((radius - _radius) * (float)radiusPrecision) != 0;
			}
			return true;
		}
	}

	public float Radius = 2f;

	[Range(0f, 360f)]
	public float StartAngleDegree = 270f;

	[Range(0f, 360f)]
	public float AngleDegree = 100f;

	public int AngleDegreePrecision = 1000;

	public int RadiusPrecision = 1000;

	private MeshFilter _meshFilter;

	private readonly SectorMeshCreator _creator = new SectorMeshCreator();

	[ExecuteInEditMode]
	private void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
	}

	private void Update()
	{
		_meshFilter.mesh = _creator.CreateMesh(Radius, StartAngleDegree, AngleDegree, AngleDegreePrecision, RadiusPrecision);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.gray;
		DrawMesh();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		DrawMesh();
	}

	private void DrawMesh()
	{
		Mesh mesh = _creator.CreateMesh(Radius, StartAngleDegree, AngleDegree, AngleDegreePrecision, RadiusPrecision);
		int[] triangles = mesh.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Gizmos.DrawLine(Convert2World(mesh.vertices[triangles[i]]), Convert2World(mesh.vertices[triangles[i + 1]]));
			Gizmos.DrawLine(Convert2World(mesh.vertices[triangles[i]]), Convert2World(mesh.vertices[triangles[i + 2]]));
			Gizmos.DrawLine(Convert2World(mesh.vertices[triangles[i + 1]]), Convert2World(mesh.vertices[triangles[i + 2]]));
		}
	}

	private Vector3 Convert2World(Vector3 src)
	{
		return base.transform.TransformPoint(src);
	}
}
