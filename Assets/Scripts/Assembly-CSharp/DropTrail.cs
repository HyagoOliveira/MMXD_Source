using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DropTrail : MonoBehaviour
{
	[Serializable]
	private class Path
	{
		public float timeCreated;

		public float fadeAlpha;

		public Vector3 localPosition = Vector3.zero;

		public Quaternion localRotation = Quaternion.identity;

		public float timeElapsed
		{
			get
			{
				return Time.time - timeCreated;
			}
		}

		public Path(Vector3 localPosition, Quaternion localRotation)
		{
			this.localPosition = localPosition;
			this.localRotation = localRotation;
			timeCreated = Time.time;
		}
	}

	public new bool enabled = true;

	public Material material;

	public float lifeTime = 3f;

	public AnimationCurve widthCurve;

	public float widthMultiplier = 0.5f;

	public int angleDivisions = 10;

	public float vertexDistance = 0.5f;

	public LineTextureMode textureMode;

	private const string _name = "[Hidden]DropTrailMesh";

	[HideInInspector]
	private GameObject _trail;

	[HideInInspector]
	private Vector3 _relativePos;

	[HideInInspector]
	private MeshFilter _meshFilter;

	[HideInInspector]
	private MeshRenderer _meshRenderer;

	[SerializeField]
	private List<Path> paths = new List<Path>();

	private Mesh _mesh
	{
		get
		{
			return _meshFilter.mesh;
		}
		set
		{
			_meshFilter.mesh = value;
		}
	}

	private int pathCnt
	{
		get
		{
			return paths.Count();
		}
	}

	private void Awake()
	{
		CheckExistence();
	}

	private void Update()
	{
		if (CheckExistence() && CheckActive())
		{
			UpdateTrail();
			UpdateMesh();
		}
	}

	public void Clear()
	{
		paths.Clear();
	}

	private bool CheckExistence()
	{
		if (!_trail)
		{
			Transform transform = base.transform.Find("[Hidden]DropTrailMesh");
			if ((bool)transform)
			{
				_trail = transform.gameObject;
				_meshFilter = _trail.GetComponent<MeshFilter>();
				_meshRenderer = _trail.GetComponent<MeshRenderer>();
			}
			else
			{
				_trail = RainDropTools.CreateHiddenObject("[Hidden]DropTrailMesh", base.transform).gameObject;
			}
		}
		if (!_meshFilter)
		{
			_meshFilter = _trail.AddComponent<MeshFilter>();
		}
		if (!_meshRenderer)
		{
			_meshRenderer = _trail.AddComponent<MeshRenderer>();
		}
		if (material == null)
		{
			return false;
		}
		_meshRenderer.material = material;
		return true;
	}

	private bool CheckActive()
	{
		_meshRenderer.enabled = enabled;
		return enabled;
	}

	private void UpdateTrail()
	{
		paths.RemoveAll((Path t) => t.timeElapsed >= lifeTime);
		if (pathCnt == 0)
		{
			paths.Add(new Path(base.transform.localPosition, base.transform.localRotation));
			paths.Add(new Path(base.transform.localPosition, base.transform.localRotation));
			_relativePos = base.transform.localPosition;
		}
		if (pathCnt == 1)
		{
			paths.Add(new Path(base.transform.localPosition, base.transform.localRotation));
			_relativePos = base.transform.localPosition;
		}
		if ((paths[0].localPosition - base.transform.localPosition).sqrMagnitude < vertexDistance)
		{
			return;
		}
		Vector3 vector = paths[0].localPosition - paths[1].localPosition;
		Vector3 vector2 = base.transform.localPosition - paths[0].localPosition;
		Quaternion a = Quaternion.identity;
		Quaternion quaternion = Quaternion.identity;
		if (vector.magnitude != 0f)
		{
			a = Quaternion.LookRotation(vector, Vector3.forward);
		}
		if (vector2.magnitude != 0f)
		{
			quaternion = Quaternion.LookRotation(vector2, Vector3.forward);
		}
		a.eulerAngles += Vector3.forward * -90f;
		quaternion.eulerAngles += Vector3.forward * -90f;
		if (paths.Count() >= 2)
		{
			float num = Mathf.Acos(Vector3.Dot(vector, vector2) / (vector.magnitude * vector2.magnitude)) * 180f / (float)Math.PI;
			if (!float.IsNaN(num))
			{
				int num2 = (int)num / angleDivisions;
				for (int i = 0; i < num2; i++)
				{
					Quaternion localRotation = Quaternion.Slerp(a, quaternion, (float)i / (float)num2);
					paths.Insert(0, new Path(paths[0].localPosition, localRotation));
				}
			}
		}
		_relativePos = vector2;
		paths.Insert(0, new Path(base.transform.localPosition, quaternion));
	}

	private void UpdateMesh()
	{
		if (pathCnt <= 1)
		{
			_meshRenderer.enabled = false;
			return;
		}
		_meshRenderer.enabled = true;
		Vector3[] array = new Vector3[pathCnt * 2];
		Vector2[] array2 = new Vector2[pathCnt * 2];
		int[] array3 = new int[(pathCnt - 1) * 6];
		for (int i = 0; i < pathCnt; i++)
		{
			float num = (float)i / (float)pathCnt;
			Path path = paths[i];
			_trail.transform.parent = base.transform.parent;
			_trail.transform.localPosition = path.localPosition;
			_trail.transform.localRotation = path.localRotation;
			float num2 = Mathf.Max(widthMultiplier * widthCurve.Evaluate(num) * 0.5f, 0.001f);
			array[i * 2] = _trail.transform.TransformPoint(0f, num2, 0f);
			array[i * 2 + 1] = _trail.transform.TransformPoint(0f, 0f - num2, 0f);
			float x = num;
			if (textureMode == LineTextureMode.Tile)
			{
				x = i;
			}
			array2[i * 2] = new Vector2(x, 0f);
			array2[i * 2 + 1] = new Vector2(x, 1f);
			if (i != 0)
			{
				array3[(i - 1) * 6] = i * 2 - 2;
				array3[(i - 1) * 6 + 1] = i * 2 - 1;
				array3[(i - 1) * 6 + 2] = i * 2;
				array3[(i - 1) * 6 + 3] = i * 2 + 1;
				array3[(i - 1) * 6 + 4] = i * 2;
				array3[(i - 1) * 6 + 5] = i * 2 - 1;
			}
			_trail.transform.parent = null;
		}
		_mesh.Clear();
		_mesh.vertices = array;
		_mesh.uv = array2;
		_mesh.triangles = array3;
		_trail.transform.localPosition = Vector3.zero;
		_trail.transform.localRotation = Quaternion.identity;
		_trail.transform.localScale = Vector3.one;
		_trail.transform.parent = base.transform;
	}

	private void OnDrawGizmos()
	{
		if (_relativePos != Vector3.zero)
		{
			Vector3 vector = base.transform.TransformPoint(0f, 0f, 0f);
			Vector3 vector2 = base.transform.TransformPoint(_relativePos) - vector;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, base.transform.position + vector2.normalized * 2f);
		}
	}
}
