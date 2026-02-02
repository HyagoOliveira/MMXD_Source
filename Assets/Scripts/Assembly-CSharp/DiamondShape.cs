using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
public class DiamondShape : MaskableGraphic
{
	private Vector3[] newVertices = new Vector3[6];

	private Vector2[] newUV = new Vector2[6];

	private int[] newTriangles = new int[15]
	{
		0, 2, 1, 0, 3, 2, 0, 4, 3, 0,
		5, 4, 0, 1, 5
	};

	private Color[] newColors = new Color[6]
	{
		Color.red,
		Color.blue,
		Color.blue,
		Color.blue,
		Color.blue,
		Color.blue
	};

	private bool bInspectorChanged;

	[SerializeField]
	[Range(0f, 100f)]
	private float m_BaseScale = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_Point0 = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_Point1 = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_Point2 = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_Point3 = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_Point4 = 1f;

	[SerializeField]
	private Color m_CenterColor = new Color(0.11f, 0.52f, 0.96f, 1f);

	[SerializeField]
	private Color m_OuterColor = new Color(0.23f, 1f, 1f, 1f);

	protected override void Awake()
	{
		base.Awake();
		ModifyMesh();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GetComponent<CanvasRenderer>().Clear();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		OnValidate();
	}

	private void OnValidate()
	{
		bInspectorChanged = true;
	}

	public void SetPointValue(int index, float val)
	{
		switch (index)
		{
		case 0:
			m_Point0 = val;
			break;
		case 1:
			m_Point1 = val;
			break;
		case 2:
			m_Point2 = val;
			break;
		case 3:
			m_Point3 = val;
			break;
		case 4:
			m_Point4 = val;
			break;
		}
		OnValidate();
	}

	private void ModifyMesh()
	{
		Mesh mesh = new Mesh();
		float num = 50f;
		newVertices[0] = Vector3.zero;
		newVertices[1] = Vector3.up * num * m_BaseScale * m_Point0;
		newVertices[2] = Quaternion.Euler(0f, 0f, 72f) * Vector3.up * num * m_BaseScale * m_Point1;
		newVertices[3] = Quaternion.Euler(0f, 0f, 144f) * Vector3.up * num * m_BaseScale * m_Point2;
		newVertices[4] = Quaternion.Euler(0f, 0f, 216f) * Vector3.up * num * m_BaseScale * m_Point3;
		newVertices[5] = Quaternion.Euler(0f, 0f, 288f) * Vector3.up * num * m_BaseScale * m_Point4;
		newColors[0] = m_CenterColor;
		newColors[1] = Color.Lerp(m_CenterColor, m_OuterColor, m_Point0);
		newColors[2] = Color.Lerp(m_CenterColor, m_OuterColor, m_Point1);
		newColors[3] = Color.Lerp(m_CenterColor, m_OuterColor, m_Point2);
		newColors[4] = Color.Lerp(m_CenterColor, m_OuterColor, m_Point3);
		newColors[5] = Color.Lerp(m_CenterColor, m_OuterColor, m_Point4);
		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.triangles = newTriangles;
		mesh.colors = newColors;
		CanvasRenderer component = GetComponent<CanvasRenderer>();
		if ((bool)component)
		{
			component.Clear();
			component.SetMesh(mesh);
			component.SetMaterial(new Material(Shader.Find("UI/Default")), null);
		}
	}

	private new void Start()
	{
	}

	private void Update()
	{
		if (bInspectorChanged)
		{
			ModifyMesh();
			bInspectorChanged = false;
		}
	}
}
