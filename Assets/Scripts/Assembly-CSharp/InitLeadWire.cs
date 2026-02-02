using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitLeadWire : MonoBehaviour
{
	public ParticleSystem tFxParticleSystem;

	[NonSerialized]
	public MeshFilter tMeshFilter;

	[NonSerialized]
	public MeshRenderer tMeshRender;

	public float fBurnTime = 1f;

	private float fMaxY = -9999999f;

	private float fMinY = 9999999f;

	public string[] BurnSE;

	private MaterialPropertyBlock tMaterialPropertyBlock;

	private OrangeCriSource SoundSource;

	private bool bPlayed;

	public void Start()
	{
		tMeshFilter = GetComponent<MeshFilter>();
		tMeshRender = GetComponent<MeshRenderer>();
		Mesh sharedMesh = tMeshFilter.sharedMesh;
		List<Vector3> list = new List<Vector3>();
		sharedMesh.GetVertices(list);
		Vector3 position = base.transform.position;
		foreach (Vector3 item in list)
		{
			Vector4 vector = base.transform.localToWorldMatrix * item;
			if (vector.y + position.y > fMaxY)
			{
				fMaxY = vector.y + position.y;
			}
			if (vector.y + position.y < fMinY)
			{
				fMinY = vector.y + position.y;
			}
		}
		tMaterialPropertyBlock = new MaterialPropertyBlock();
		tMeshRender.GetPropertyBlock(tMaterialPropertyBlock);
		tMaterialPropertyBlock.SetFloat("_CutoffMax", fMaxY);
		tMaterialPropertyBlock.SetFloat("_CutoffMin", fMinY);
		tMaterialPropertyBlock.SetFloat("_ScaleFactor", 1f);
		tMeshRender.SetPropertyBlock(tMaterialPropertyBlock);
		SoundSource = GetComponentInParent<OrangeCriSource>();
	}

	public void StartBurn()
	{
		if (SoundSource != null && BurnSE.Length >= 2)
		{
			SoundSource.PlaySE(BurnSE[0], BurnSE[1]);
			bPlayed = true;
		}
		StartCoroutine(BurnCoroutine(fBurnTime));
	}

	private void OnDisable()
	{
		if (SoundSource != null && bPlayed && BurnSE.Length >= 4)
		{
			SoundSource.PlaySE(BurnSE[2], BurnSE[3]);
			bPlayed = false;
		}
	}

	private IEnumerator BurnCoroutine(float fTime)
	{
		if (tFxParticleSystem != null)
		{
			tFxParticleSystem.Play(true);
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		MoveAccordingY[] tMoveAccordingYs = base.transform.GetComponentsInChildren<MoveAccordingY>();
		float fNowTime = fTime;
		while (fNowTime >= 0f)
		{
			float num = fNowTime / fTime;
			tMaterialPropertyBlock.SetFloat("_ScaleFactor", num);
			tMeshRender.SetPropertyBlock(tMaterialPropertyBlock);
			MoveAccordingY[] array = tMoveAccordingYs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetYAndMove(fMinY + (fMaxY - fMinY) * num);
			}
			fNowTime -= Time.deltaTime;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private void BurnStart()
	{
		StartBurn();
	}
}
