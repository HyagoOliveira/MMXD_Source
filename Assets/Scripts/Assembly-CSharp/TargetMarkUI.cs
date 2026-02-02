using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class TargetMarkUI : OrangeUIBase, IManagedLateUpdateBehavior
{
	[SerializeField]
	private RectTransform panel;

	private Camera targetCamera;

	private float panelWidth = -1f;

	private float panelHeight = -1f;

	private string vtmName = "VisualTargetMark";

	private float angleZ;

	private Vector3 checkPoint = new Vector3(0f, 0f, 0f);

	private Vector2 CachePosion = Vector2.zero;

	private Dictionary<OrangeCharacter, VisualTargetMark> dictCacheMask = new Dictionary<OrangeCharacter, VisualTargetMark>();

	public void ApplyLateUpdate()
	{
		targetCamera = OrangeSceneManager.FindObjectOfTypeCustom<CameraControl>().GetComponent<Camera>();
		panelWidth = 800f;
		panelHeight = 500f;
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<VisualTargetMark>("prefab/visualtargetmark", vtmName, 1, delegate
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		});
	}

	public void RemoveLateUpdate()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(vtmName);
		dictCacheMask = new Dictionary<OrangeCharacter, VisualTargetMark>();
		OnClickCloseBtn();
	}

	public void LateUpdateFunc()
	{
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			if (runPlayer.IsLocalPlayer)
			{
				continue;
			}
			VisualTargetMark value = null;
			if (!MonoBehaviourSingleton<PoolManager>.Instance.ExistsInPool<VisualTargetMark>(vtmName))
			{
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<VisualTargetMark>("prefab/visualtargetmark", vtmName, 1, null);
			}
			if (!dictCacheMask.TryGetValue(runPlayer, out value))
			{
				value = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<VisualTargetMark>(vtmName);
				value.rt.SetParent(panel, false);
				dictCacheMask.Add(runPlayer, value);
			}
			if ((int)runPlayer.Hp <= 0)
			{
				if (value.gameObject.activeSelf)
				{
					value.gameObject.SetActive(false);
				}
			}
			else
			{
				if (targetCamera == null)
				{
					continue;
				}
				checkPoint = targetCamera.WorldToViewportPoint(runPlayer.transform.localPosition);
				if (checkPoint.x > 1f || checkPoint.y > 1f || checkPoint.x < 0f || checkPoint.y < 0f)
				{
					if (!value.gameObject.activeSelf)
					{
						value.gameObject.SetActive(true);
					}
					if ((int)runPlayer.TargetMask == (int)ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask)
					{
						value.SetType(1);
					}
					else
					{
						value.SetType(0);
					}
					CachePosion = new Vector2(runPlayer.transform.localPosition.x, runPlayer.transform.localPosition.y) - new Vector2(targetCamera.transform.localPosition.x, targetCamera.transform.localPosition.y);
					angleZ = Vector3.Angle(Vector3.up, CachePosion);
					Vector3 lhs = Vector3.Cross(new Vector3(0f, 1f, 0f), CachePosion);
					angleZ *= Mathf.Sign(Vector3.Dot(lhs, Vector3.forward));
					value.transform.eulerAngles = new Vector3(0f, 0f, angleZ);
					CachePosion = CachePosion.normalized * ((panelWidth > panelHeight) ? panelWidth : panelHeight);
					CachePosion.x = Mathf.Clamp(CachePosion.x, (0f - panelWidth) / 2f, panelWidth / 2f);
					CachePosion.y = Mathf.Clamp(CachePosion.y, (0f - panelHeight) / 2f, panelHeight / 2f);
					value.rt.anchoredPosition = CachePosion;
				}
				else if (value.gameObject.activeSelf)
				{
					value.gameObject.SetActive(false);
				}
			}
		}
	}
}
