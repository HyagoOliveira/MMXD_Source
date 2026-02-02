using UnityEngine;

public class StageAnimationEvent : MonoBehaviour
{
	private EnemyControllerBase tSelfECB;

	private OrangeCharacter tSelfOC;

	public void EventSetStatus(int nSetNextStatus)
	{
		CheckECB();
	}

	public void CallFunction(string sFunctionName)
	{
		CheckECB();
		tSelfECB.transform.SendMessage(sFunctionName);
	}

	private void CheckECB()
	{
		if (tSelfECB == null && tSelfOC == null)
		{
			tSelfECB = base.transform.GetComponent<EnemyControllerBase>();
			Transform parent = base.transform;
			while (tSelfECB == null && parent != null)
			{
				tSelfECB = parent.GetComponent<EnemyControllerBase>();
				parent = parent.parent;
			}
			tSelfOC = base.transform.GetComponent<OrangeCharacter>();
			parent = base.transform;
			while (tSelfOC == null && parent != null)
			{
				tSelfOC = parent.GetComponent<OrangeCharacter>();
				parent = parent.parent;
			}
		}
	}

	public void SendMessageToCtrl(string sMsg)
	{
		CheckECB();
		if (tSelfECB != null)
		{
			tSelfECB.transform.SendMessage(sMsg);
		}
		else if (tSelfOC != null)
		{
			tSelfOC.transform.SendMessage(sMsg);
		}
		else
		{
			base.transform.SendMessage(sMsg);
		}
	}
}
