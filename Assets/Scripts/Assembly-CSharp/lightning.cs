using System;
using System.Collections.Generic;
using UnityEngine;

public class lightning : MonoBehaviour
{
	public Texture[] BeamFrames;

	public Texture[] BeamFramesEx;

	[HideInInspector]
	public LineRenderer lineRendererA;

	[HideInInspector]
	public LineRenderer lineRendererB;

	public List<Transform> _lineList;

	public List<Vector3> _hitList;

	public Transform nullhit;

	public float beamLength = 2f;

	private int frameNo;

	private int frameNoB;

	private float m_LastUpdateShowTime;

	public float m_UpdateShowDeltaTime = 0.04f;

	private int m_FrameUpdate;

	private bool isActive;

	public float MaxLine = 2f;

	public bool isRandPos = true;

	private void Awake()
	{
		lineRendererA = _lineList[0].GetChild(0).transform.GetComponent<LineRenderer>();
		lineRendererB = _lineList[0].GetChild(1).transform.GetComponent<LineRenderer>();
		frameNo = 0;
		frameNoB = 0;
	}

	private void Start()
	{
	}

	private void Animate()
	{
		lineRendererA.material.mainTexture = BeamFrames[frameNo];
		frameNo++;
		if (frameNo >= BeamFrames.Length)
		{
			frameNo = 0;
		}
		lineRendererB.material.mainTexture = BeamFramesEx[frameNoB];
		frameNoB++;
		if (frameNoB >= BeamFramesEx.Length)
		{
			frameNoB = 0;
		}
	}

	private void Updata_hitList()
	{
		int num = 0;
		for (int i = 0; i < _hitList.Count; i++)
		{
			Vector3 vector = base.transform.position;
			Vector3 vector2 = _hitList[0];
			lineRendererA = _lineList[i].GetChild(0).transform.GetComponent<LineRenderer>();
			lineRendererB = _lineList[i].GetChild(1).transform.GetComponent<LineRenderer>();
			if (i > 0)
			{
				_lineList[i].transform.position = _hitList[i - 1 - num];
				vector = _hitList[i - 1 - num];
				vector2 = _hitList[i];
			}
			if (isRandPos && i != _hitList.Count - 1)
			{
				vector2.y += UnityEngine.Random.Range(-0.3f, 0.3f);
			}
			Vector3 vector3 = -(vector - vector2).normalized;
			float num2 = Vector2.Distance(vector, vector2);
			if (num2 > 500f)
			{
				num++;
				continue;
			}
			num = 0;
			int num3 = Convert.ToInt16(Math.Ceiling(num2 / 2f));
			beamLength = num2;
			if ((float)num3 < 2f)
			{
				lineRendererA.positionCount = 2;
				lineRendererA.SetPosition(0, Vector3.zero);
				lineRendererA.SetPosition(1, new Vector3(vector3.z * num2, vector3.y * num2, vector3.x * num2));
				lineRendererB.positionCount = 2;
				lineRendererB.SetPosition(0, Vector3.zero);
				lineRendererB.SetPosition(1, new Vector3(vector3.z * num2, vector3.y * num2, vector3.x * num2));
				continue;
			}
			lineRendererA.positionCount = num3 + 1;
			lineRendererA.SetPosition(0, Vector3.zero);
			lineRendererB.positionCount = num3 + 1;
			lineRendererB.SetPosition(0, Vector3.zero);
			for (int j = 1; j < num3; j++)
			{
				float num4 = UnityEngine.Random.Range(0f, 0.4f) * num2 / MaxLine;
				float num5 = UnityEngine.Random.Range(-0.1f, 0.1f);
				if (!isRandPos)
				{
					num5 = 0f;
					num4 = 0f;
				}
				num5 = ((!((double)num5 < 0.0)) ? (num5 + num4) : (num5 - num4));
				float num6 = 0f;
				float num7 = 0f;
				if (vector3.y == 0f)
				{
					num7 = 0f;
					num6 = 1f;
				}
				else
				{
					num6 = 1f - Math.Abs(vector3.y);
					num7 = Math.Abs(vector3.y);
				}
				lineRendererA.SetPosition(j, new Vector3((0f - vector3.z) * (float)j * 2f, vector3.y * (float)j * 2f + num5 * num6, vector3.x * (float)j * 2f + num5 * num7));
				lineRendererB.SetPosition(j, new Vector3((0f - vector3.z) * (float)j * 2f, vector3.y * (float)j * 2f + num5 * num6, vector3.x * (float)j * 2f + num5 * num7));
			}
			lineRendererA.SetPosition(num3, new Vector3((0f - vector3.z) * num2, vector3.y * num2, vector3.x * num2));
			lineRendererB.SetPosition(num3, new Vector3((0f - vector3.z) * num2, vector3.y * num2, vector3.x * num2));
		}
	}

	public void ClearBuffLine()
	{
		int count = _hitList.Count;
		if (count > 1)
		{
			for (int num = count - 1; num > 0; num--)
			{
				_hitList.Remove(_hitList[num]);
			}
		}
	}

	public void Update_Lightning()
	{
		if (!isActive || _hitList.Count <= 0)
		{
			return;
		}
		if (_hitList.Count > _lineList.Count)
		{
			int num = _hitList.Count - _lineList.Count;
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_lineList[0].gameObject);
				gameObject.transform.parent = base.transform;
				gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
				_lineList.Add(gameObject.transform);
			}
		}
		else if (_hitList.Count < _lineList.Count)
		{
			int num2 = _lineList.Count - _hitList.Count;
			for (int j = 0; j < num2; j++)
			{
				GameObject gameObject2 = _lineList[_lineList.Count - 1].gameObject;
				_lineList.Remove(gameObject2.transform);
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		Updata_hitList();
		nullhit.position = new Vector3(_hitList[0].x, _hitList[0].y, _hitList[0].z);
		if (BeamFrames.Length <= 1)
		{
			return;
		}
		m_FrameUpdate++;
		if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
		{
			for (int k = 0; k < _hitList.Count; k++)
			{
				Vector3 position = base.transform.position;
				Vector3 vector = _hitList[0];
				lineRendererA = _lineList[k].GetChild(0).transform.GetComponent<LineRenderer>();
				lineRendererB = _lineList[k].GetChild(1).transform.GetComponent<LineRenderer>();
				Animate();
			}
			m_FrameUpdate = 0;
			m_LastUpdateShowTime = Time.realtimeSinceStartup;
		}
	}

	public void SetActive(bool active)
	{
		isActive = active;
		lineRendererA.enabled = active;
		lineRendererB.enabled = active;
	}
}
