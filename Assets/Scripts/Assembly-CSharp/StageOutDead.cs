using System.Collections;
using StageLib;
using UnityEngine;

public class StageOutDead : MonoBehaviour
{
	public float fDeadTime;

	public float fDeadTimeNow;

	public bool bInGameScreen;

	public float cameraHHalf;

	public float cameraWHalf;

	public Vector3 dd;

	private Camera tCamera;

	private CameraControl tCameraControl;

	private StageObjBase tSOB;

	private float fNetCheckTime = 3f;

	private void Start()
	{
		tCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
		if (tCamera == null)
		{
			StartCoroutine(InitCoroutine());
		}
		else
		{
			tCameraControl = tCamera.GetComponent<CameraControl>();
		}
		fDeadTimeNow = fDeadTime * 2f;
		fNetCheckTime = fDeadTime * 0.5f;
		tSOB = GetComponent<StageObjBase>();
	}

	private IEnumerator InitCoroutine()
	{
		while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		tCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
		tCameraControl = tCamera.GetComponent<CameraControl>();
	}

	private void LateUpdate()
	{
		if (!(tCameraControl != null))
		{
			return;
		}
		if (bInGameScreen)
		{
			if (!tCameraControl.CurrentLockRange.PositionInRange(base.transform.position))
			{
				fDeadTimeNow -= Time.deltaTime;
				if (fDeadTimeNow + Time.deltaTime > fNetCheckTime && fDeadTimeNow < fNetCheckTime)
				{
					StageUpdate.SyncStageObj(4, 8, tSOB.sNetSerialID, true);
				}
				if (fDeadTimeNow <= 0f)
				{
					GetComponent<EnemyControllerBase>().bNeedDead = true;
					Object.Destroy(this);
				}
			}
			else
			{
				fDeadTimeNow = fDeadTime;
			}
		}
		else if (tCameraControl.CurrentLockRange.PositionInRange(base.transform.position))
		{
			bInGameScreen = true;
			fDeadTimeNow = fDeadTime;
		}
		else
		{
			fDeadTimeNow -= Time.deltaTime;
			if (fDeadTimeNow + Time.deltaTime > fNetCheckTime && fDeadTimeNow < fNetCheckTime)
			{
				StageUpdate.SyncStageObj(4, 8, tSOB.sNetSerialID, true);
			}
			if (fDeadTimeNow <= 0f)
			{
				GetComponent<EnemyControllerBase>().bNeedDead = true;
				Object.Destroy(this);
			}
		}
	}

	public void CheckCountDown()
	{
		if (tCameraControl != null && bInGameScreen && tCameraControl.CurrentLockRange.PositionInRange(base.transform.position) && tSOB != null)
		{
			StageUpdate.SyncStageObj(4, 9, tSOB.sNetSerialID, true);
		}
	}

	public void ReSetCountDown()
	{
		if (bInGameScreen)
		{
			fDeadTimeNow = fDeadTime;
		}
		else
		{
			fDeadTimeNow = fDeadTime * 2f;
		}
	}
}
