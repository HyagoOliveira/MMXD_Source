using UnityEngine;
using UnityEngine.UI;

public class DebugDevQaMenu : MonoBehaviour
{
	[SerializeField]
	private Text textLockHpFlg;

	[SerializeField]
	private Text textLockEnemyHpFlg;

	[SerializeField]
	private Text textLockEnemyOneHitDieFlg;

	[SerializeField]
	private Text textEnemyDebugMode;

	[SerializeField]
	private Text textTutoFlg;

	[SerializeField]
	private Text textUilockFlg;

	[SerializeField]
	private Text textCDFlg;

	[SerializeField]
	private Text textCDFlg2;

	[SerializeField]
	private Text textDpsFlg;

	[SerializeField]
	private Text textAimInfoFlg;

	[SerializeField]
	private Text textEnemyFreezeBehaviorFlg;

	[SerializeField]
	private Text textAudioFlg;

	[SerializeField]
	private DpsController dpsController;

	[SerializeField]
	private AimInfoController aimInfoController;

	[SerializeField]
	private Text textAllStageCtrlEventFlg;

	[SerializeField]
	private Text textAlwaysShowFpsFlg;

	[SerializeField]
	private Text textMaintain;

	[SerializeField]
	private Text textStageClearOn;

	[SerializeField]
	private Text textGachaIgnorePerform;

	[SerializeField]
	private Transform contentPassive;

	[SerializeField]
	private PassiveSubItem passiveSubItem;
}
