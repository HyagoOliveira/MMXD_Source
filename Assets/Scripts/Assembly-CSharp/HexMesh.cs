using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
	private Mesh hexMesh;

	private MeshCollider meshCollider;

	private List<Vector3> vertices;

	private List<int> triangles;

	private void Awake()
	{
		GetComponent<MeshFilter>().mesh = (hexMesh = new Mesh());
		meshCollider = base.gameObject.AddComponent<MeshCollider>();
		hexMesh.name = "Hex Mesh";
		vertices = new List<Vector3>();
		triangles = new List<int>();
	}

	public void Triangulate(HexCell[] cells)
	{
		hexMesh.Clear();
		vertices.Clear();
		triangles.Clear();
		for (int i = 0; i < cells.Length; i++)
		{
			Triangulate(cells[i]);
		}
		hexMesh.vertices = vertices.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.RecalculateNormals();
		meshCollider.sharedMesh = hexMesh;
	}

	private void Triangulate(HexCell cell)
	{
		if (!(cell == null))
		{
			Vector3 localPosition = cell.transform.localPosition;
			for (int i = 0; i < 4; i++)
			{
				AddTriangle(localPosition + HexMetrics.Corners[0], localPosition + HexMetrics.Corners[i + 1], localPosition + HexMetrics.Corners[i + 2]);
			}
		}
	}

	private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int count = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(count);
		triangles.Add(count + 1);
		triangles.Add(count + 2);
	}
}
