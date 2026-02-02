#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class PvpEndUI : OrangeUIBase
{
	public GameObject LeftPlayer;

	public GameObject RightPlayer;

	public GameObject oneroot;

	public GameObject threeroot;

	public GameObject fiveroot;

	public GameObject oneLeftPlayer;

	public GameObject oneRightPlayer;

	public GameObject[] threeLeftPlayer;

	public GameObject[] threeRightPlayer;

	public GameObject[] fiveLeftPlayer;

	public GameObject[] fiveRightPlayer;

	public GameObject[] TopLeft;

	public GameObject[] TopRight;

	public GameObject refCommonSmallIcon;

	public Image[] WinObjs;

	public Image[] LostObjs;

	public Image[] DrawObjs;

	public Image WhiteBG;

	public Image LeftSrollBG;

	public Image RightSrollBG;

	[SerializeField]
	private GameObject SeasonRoot;

	[SerializeField]
	private GameObject[] LeftWeaponObjs;

	[SerializeField]
	private GameObject[] RightWeaponObjs;

	[SerializeField]
	private Image[] LeftCharaterObjs;

	[SerializeField]
	private Image[] RightCharaterObjs;

	[SerializeField]
	private Transform[] LeftCharacterStarOnRoot;

	[SerializeField]
	private Transform[] RightCharacterStarOnRoot;

	[SerializeField]
	private GameObject LeftRankIconBase;

	[SerializeField]
	private GameObject RightRankIconBase;

	[SerializeField]
	private Text[] LeftHurtPercentText;

	[SerializeField]
	private Text[] RightHurtPercentText;

	[SerializeField]
	private Text SeasonScoreText;

	[SerializeField]
	private Text SeasonNextRankInfoText;

	[SerializeField]
	private Text LeftNameText;

	[SerializeField]
	private Text RightNameText;

	[SerializeField]
	private GameObject LeftPlayerIcon;

	[SerializeField]
	private GameObject RightPlayerIcon;

	[SerializeField]
	private Button LeftAddFriendBtn;

	[SerializeField]
	private Button RightAddFriendBtn;

	[SerializeField]
	private Image SeasonNextRankBar;

	[SerializeField]
	private GameObject[] RankAnimatorRoot;

	[SerializeField]
	private Animator[] RankAnimator;

	[SerializeField]
	private Image[] LeftHPbarImage;

	[SerializeField]
	private Image[] RightHPbarImage;

	[SerializeField]
	private Image[] RankIconImage;

	[SerializeField]
	private Image[] RankStarIconImage;

	[SerializeField]
	private Image[] AnimatorRankImage;

	[SerializeField]
	private Image[] AnimatorRankStarImage;

	[SerializeField]
	private Button ContinueBtn;

	[SerializeField]
	private Button BackToPVPRoomBtn;

	[SerializeField]
	private OrangeText BackToPVPRoomCountdown;

	[SerializeField]
	private Transform PlayerSignRootL;

	[SerializeField]
	private Transform PlayerSignRootR;

	[SerializeField]
	private Transform PlayerSignRootSeasonL;

	[SerializeField]
	private Transform PlayerSignRootSeasonR;

	[SerializeField]
	private GameObject SignObject;

	private bool bIsSeason;

	private string[] TeamPlayerID = new string[2] { "", "" };

	private int[] nTotalDmg;

	private int[] nMVPID;

	private List<GameObject> listLeftMove = new List<GameObject>();

	private List<GameObject> listRightMove = new List<GameObject>();

	private List<Image> RightSrollBGs = new List<Image>();

	private List<Image> LeftSrollBGs = new List<Image>();

	private int nWinType = 1;

	private bool bDelayedCall;

	private MemberInfo hostMemberInfo;

	private MemberInfo guestMemberInfo;

	private int countdownTweenId;

	private int NewScore;

	private int OldScore;

	public void SetPlayerSignIcon(Transform SignRoot, int n_ID, string PlayerID, bool bHUD = false)
	{
	}

	private void Init()
	{
		oneroot.SetActive(false);
		threeroot.SetActive(false);
		fiveroot.SetActive(false);
		SeasonRoot.SetActive(false);
		LeftPlayer.SetActive(false);
		RightPlayer.SetActive(false);
		LeftSrollBG.gameObject.SetActive(false);
		RightSrollBG.gameObject.SetActive(false);
		float height = ((RectTransform)RightSrollBG.transform).rect.height;
		for (int i = 0; i < 2; i++)
		{
			RightSrollBGs.Add(UnityEngine.Object.Instantiate(RightSrollBG, RightSrollBG.transform.parent));
			LeftSrollBGs.Add(UnityEngine.Object.Instantiate(LeftSrollBG, LeftSrollBG.transform.parent));
			Vector3 localPosition = RightSrollBG.transform.localPosition;
			RightSrollBGs[i].transform.localPosition = new Vector3(localPosition.x, ((RectTransform)RightSrollBG.transform).rect.height * (float)(-i), 0f);
			localPosition = LeftSrollBG.transform.localPosition;
			LeftSrollBGs[i].transform.localPosition = new Vector3(localPosition.x, ((RectTransform)LeftSrollBG.transform).rect.height * (float)(-i), 0f);
			RightSrollBGs[i].gameObject.SetActive(true);
			LeftSrollBGs[i].gameObject.SetActive(true);
		}
		for (int j = 0; j < 2; j++)
		{
			RightSrollBGs[j].transform.parent = RightSrollBG.transform;
			LeftSrollBGs[j].transform.parent = LeftSrollBG.transform;
		}
		LeftSrollBG.enabled = false;
		RightSrollBG.enabled = false;
		StartCoroutine(ScroolBGSCoroutine(height, 4f, 3f));
	}

	private IEnumerator ScroolBGSCoroutine(float fHeight, float fMove = 2f, float fSS = 1f)
	{
		Color tmpColor = new Color(1f, 1f, 1f, 1f);
		while (true)
		{
			float num = fMove * Time.deltaTime * 60f;
			for (int i = 0; i < 2; i++)
			{
				Vector3 localPosition = RightSrollBGs[i].transform.localPosition;
				localPosition.y += num;
				if (localPosition.y >= fHeight)
				{
					localPosition.y -= 2f * fHeight;
				}
				RightSrollBGs[i].transform.localPosition = localPosition;
				RightSrollBGs[i].color = tmpColor;
				localPosition = LeftSrollBGs[i].transform.localPosition;
				localPosition.y += num;
				if (localPosition.y >= fHeight)
				{
					localPosition.y -= 2f * fHeight;
				}
				LeftSrollBGs[i].transform.localPosition = localPosition;
				LeftSrollBGs[i].color = tmpColor;
				float a = 0.7f + 0.3f * Mathf.Sin(Time.realtimeSinceStartup * fSS);
				tmpColor.a = a;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	public void StartMove()
	{
		float num = -1500f;
		for (int i = 0; i < listLeftMove.Count; i++)
		{
			listLeftMove[i].transform.localPosition = new Vector3(num - (float)(i * 350), 0f, 0f);
		}
		num = 1500f;
		for (int j = 0; j < listRightMove.Count; j++)
		{
			listRightMove[j].transform.localPosition = new Vector3(num + (float)(j * 350), 0f, 0f);
		}
		for (int k = 0; k < TopLeft.Length; k++)
		{
			TopLeft[k].transform.localPosition = new Vector3(-1500f, TopLeft[k].transform.localPosition.y, 0f);
			TopRight[k].transform.localPosition = new Vector3(1500f, TopRight[k].transform.localPosition.y, 0f);
		}
		float[] array = new float[2] { 800f, -800f };
		for (int l = 0; l < 2; l++)
		{
			WinObjs[l].gameObject.SetActive(nWinType == 1);
			LostObjs[l].gameObject.SetActive(nWinType == 0);
			DrawObjs[l].gameObject.SetActive(nWinType == 2);
			WinObjs[l].gameObject.transform.localPosition = new Vector3(0f, array[l], 0f);
			LostObjs[l].gameObject.transform.localPosition = new Vector3(0f, array[l], 0f);
			DrawObjs[l].gameObject.transform.localPosition = new Vector3(0f, array[l], 0f);
		}
		StartCoroutine(MoveCoroutine());
	}

	public IEnumerator MoveCoroutine()
	{
		float[] fYMove = new float[2] { -40f, 40f };
		float nCount = 800f;
		float timeDeltaX60 = Time.deltaTime * 60f;
		while (nCount > 0f)
		{
			for (int i = 0; i < 2; i++)
			{
				Vector3 localPosition = WinObjs[i].gameObject.transform.localPosition;
				localPosition.y += fYMove[i] * timeDeltaX60;
				float y = localPosition.y;
				float num = 0f;
				WinObjs[i].gameObject.transform.localPosition = localPosition;
				localPosition = LostObjs[i].gameObject.transform.localPosition;
				localPosition.y += fYMove[i] * timeDeltaX60;
				float y2 = localPosition.y;
				float num2 = 0f;
				LostObjs[i].gameObject.transform.localPosition = localPosition;
				localPosition = DrawObjs[i].gameObject.transform.localPosition;
				localPosition.y += fYMove[i] * timeDeltaX60;
				float y3 = localPosition.y;
				float num3 = 0f;
				DrawObjs[i].gameObject.transform.localPosition = localPosition;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			nCount -= 40f * timeDeltaX60;
		}
		WhiteBG.gameObject.SetActive(true);
		WhiteBG.color = new Color(1f, 1f, 1f, 0f);
		float fAlpha = 0f;
		float ObjAlpha = 1f;
		while (fAlpha < 1f)
		{
			Vector3 localScale = WinObjs[1].transform.localScale;
			localScale.x += 0.1f * timeDeltaX60;
			localScale.y += 0.1f * timeDeltaX60;
			WinObjs[1].transform.localScale = localScale;
			WinObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = LostObjs[1].transform.localScale;
			localScale.x += 0.1f * timeDeltaX60;
			localScale.y += 0.1f * timeDeltaX60;
			LostObjs[1].transform.localScale = localScale;
			LostObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = DrawObjs[1].transform.localScale;
			localScale.x += 0.1f * timeDeltaX60;
			localScale.y += 0.1f * timeDeltaX60;
			DrawObjs[1].transform.localScale = localScale;
			DrawObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			WhiteBG.color = new Color(1f, 1f, 1f, fAlpha);
			fAlpha += 0.1f * timeDeltaX60;
			ObjAlpha -= 0.05f * timeDeltaX60;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (fAlpha > 0f)
		{
			Vector3 localScale = WinObjs[1].transform.localScale;
			localScale.x += 0.02f * timeDeltaX60;
			localScale.y += 0.02f * timeDeltaX60;
			WinObjs[1].transform.localScale = localScale;
			WinObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = LostObjs[1].transform.localScale;
			localScale.x += 0.02f * timeDeltaX60;
			localScale.y += 0.02f * timeDeltaX60;
			LostObjs[1].transform.localScale = localScale;
			LostObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = DrawObjs[1].transform.localScale;
			localScale.x += 0.02f * timeDeltaX60;
			localScale.y += 0.02f * timeDeltaX60;
			DrawObjs[1].transform.localScale = localScale;
			DrawObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			WhiteBG.color = new Color(1f, 1f, 1f, fAlpha);
			fAlpha -= 0.03f * timeDeltaX60;
			ObjAlpha -= 0.01f * timeDeltaX60;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (ObjAlpha > 0f)
		{
			WinObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			LostObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			DrawObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			ObjAlpha -= 0.01f * timeDeltaX60;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT02);
		int inverSE = 0;
		float fMoveDis = 35f;
		while (listLeftMove[listLeftMove.Count - 1].transform.localPosition.x != 0f || listRightMove[listRightMove.Count - 1].transform.localPosition.x != 0f)
		{
			Vector3 localPosition = WinObjs[0].transform.localPosition;
			localPosition.y += 15f * timeDeltaX60;
			if (localPosition.y > 321.4f)
			{
				localPosition.y = 321.4f;
			}
			WinObjs[0].transform.localPosition = localPosition;
			LostObjs[0].transform.localPosition = localPosition;
			DrawObjs[0].transform.localPosition = localPosition;
			for (int j = 0; j < TopLeft.Length; j++)
			{
				localPosition = TopLeft[j].transform.localPosition;
				localPosition.x += fMoveDis * timeDeltaX60;
				if (localPosition.x > 0f)
				{
					localPosition.x = 0f;
				}
				TopLeft[j].transform.localPosition = localPosition;
				localPosition = TopRight[j].transform.localPosition;
				localPosition.x -= fMoveDis * timeDeltaX60;
				if (localPosition.x < 0f)
				{
					localPosition.x = 0f;
				}
				TopRight[j].transform.localPosition = localPosition;
			}
			for (int k = 0; k < listLeftMove.Count; k++)
			{
				localPosition = listLeftMove[k].transform.localPosition;
				localPosition.x += fMoveDis * timeDeltaX60;
				if (localPosition.x > 0f)
				{
					localPosition.x = 0f;
				}
				listLeftMove[k].transform.localPosition = localPosition;
			}
			for (int l = 0; l < listRightMove.Count; l++)
			{
				localPosition = listRightMove[l].transform.localPosition;
				localPosition.x -= fMoveDis * timeDeltaX60;
				if (localPosition.x < 0f)
				{
					localPosition.x = 0f;
				}
				listRightMove[l].transform.localPosition = localPosition;
			}
			if (listLeftMove.Count > 1 && inverSE < 2)
			{
				switch (inverSE)
				{
				case 0:
					if (listLeftMove[1].transform.localPosition.x > -450f && inverSE == 0)
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT02);
						inverSE++;
					}
					break;
				case 1:
					if (listLeftMove[2].transform.localPosition.x > -350f && inverSE == 1)
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT02);
						inverSE++;
					}
					break;
				}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	public void SeasonStartMove()
	{
		float[] array = new float[2] { 800f, -800f };
		for (int i = 0; i < 2; i++)
		{
			WinObjs[i].gameObject.SetActive(nWinType == 1);
			LostObjs[i].gameObject.SetActive(nWinType == 0);
			DrawObjs[i].gameObject.SetActive(nWinType == 2);
			WinObjs[i].gameObject.transform.localPosition = new Vector3(0f, array[i], 0f);
			LostObjs[i].gameObject.transform.localPosition = new Vector3(0f, array[i], 0f);
			DrawObjs[i].gameObject.transform.localPosition = new Vector3(0f, array[i], 0f);
		}
		StartCoroutine(SeasonMoveCoroutine());
	}

	public IEnumerator SeasonMoveCoroutine()
	{
		float[] fYMove = new float[2] { -40f, 40f };
		Vector3 tPos2;
		for (int nCount = 20; nCount > 0; nCount--)
		{
			for (int i = 0; i < 2; i++)
			{
				tPos2 = WinObjs[i].gameObject.transform.localPosition;
				tPos2.y += fYMove[i];
				float y = tPos2.y;
				float num = 0f;
				WinObjs[i].gameObject.transform.localPosition = tPos2;
				tPos2 = LostObjs[i].gameObject.transform.localPosition;
				tPos2.y += fYMove[i];
				float y2 = tPos2.y;
				float num2 = 0f;
				LostObjs[i].gameObject.transform.localPosition = tPos2;
				tPos2 = DrawObjs[i].gameObject.transform.localPosition;
				tPos2.y += fYMove[i];
				float y3 = tPos2.y;
				float num3 = 0f;
				DrawObjs[i].gameObject.transform.localPosition = tPos2;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		WhiteBG.gameObject.SetActive(true);
		WhiteBG.color = new Color(1f, 1f, 1f, 0f);
		float fAlpha = 0f;
		float ObjAlpha = 1f;
		while (fAlpha < 1f)
		{
			Vector3 localScale = WinObjs[1].transform.localScale;
			localScale.x += 0.1f;
			localScale.y += 0.1f;
			WinObjs[1].transform.localScale = localScale;
			WinObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = LostObjs[1].transform.localScale;
			localScale.x += 0.1f;
			localScale.y += 0.1f;
			LostObjs[1].transform.localScale = localScale;
			LostObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = DrawObjs[1].transform.localScale;
			localScale.x += 0.1f;
			localScale.y += 0.1f;
			DrawObjs[1].transform.localScale = localScale;
			DrawObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			WhiteBG.color = new Color(1f, 1f, 1f, fAlpha);
			fAlpha += 0.1f;
			ObjAlpha -= 0.05f;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (fAlpha > 0f)
		{
			Vector3 localScale = WinObjs[1].transform.localScale;
			localScale.x += 0.02f;
			localScale.y += 0.02f;
			WinObjs[1].transform.localScale = localScale;
			WinObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = LostObjs[1].transform.localScale;
			localScale.x += 0.02f;
			localScale.y += 0.02f;
			LostObjs[1].transform.localScale = localScale;
			LostObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			localScale = DrawObjs[1].transform.localScale;
			localScale.x += 0.02f;
			localScale.y += 0.02f;
			DrawObjs[1].transform.localScale = localScale;
			DrawObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			WhiteBG.color = new Color(1f, 1f, 1f, fAlpha);
			fAlpha -= 0.03f;
			ObjAlpha -= 0.01f;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (ObjAlpha > 0f)
		{
			WinObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			LostObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			DrawObjs[1].color = new Color(1f, 1f, 1f, ObjAlpha);
			ObjAlpha -= 0.01f;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (listLeftMove.Count == 1)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT02);
		}
		tPos2 = WinObjs[0].transform.localPosition;
		while (tPos2.y < 321.4f)
		{
			tPos2 = WinObjs[0].transform.localPosition;
			tPos2.y += 15f;
			if (tPos2.y > 321.4f)
			{
				tPos2.y = 321.4f;
			}
			WinObjs[0].transform.localPosition = tPos2;
			LostObjs[0].transform.localPosition = tPos2;
			DrawObjs[0].transform.localPosition = tPos2;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private void OpenWinBackround(int i)
	{
		if (!(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
		{
			return;
		}
		if (nWinType == 1)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team == 0)
			{
				LeftSrollBG.gameObject.SetActive(true);
			}
			else
			{
				RightSrollBG.gameObject.SetActive(true);
			}
		}
		else if (nWinType == 0)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team == 0)
			{
				RightSrollBG.gameObject.SetActive(true);
			}
			else
			{
				LeftSrollBG.gameObject.SetActive(true);
			}
		}
	}

	public void ShowOnePvpEnd()
	{
		oneroot.SetActive(true);
		threeroot.SetActive(false);
		fiveroot.SetActive(false);
		SeasonRoot.SetActive(false);
		int count = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
		int num = 0;
		int num2 = 0;
		GameObject gameObject = UnityEngine.Object.Instantiate(LeftPlayer, oneLeftPlayer.transform);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(RightPlayer, oneRightPlayer.transform);
		nTotalDmg = new int[2];
		nMVPID = new int[2];
		InitTeamDmg();
		for (int i = 0; i < count; i++)
		{
			int team = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team == 0)
			{
				OpenWinBackround(i);
				SetPlayerData(gameObject, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i], true, nMVPID[team] == i);
				listLeftMove.Add(gameObject);
				num++;
			}
			else if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team == 1)
			{
				OpenWinBackround(i);
				SetPlayerData(gameObject2, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i], true, nMVPID[team] == i);
				listRightMove.Add(gameObject2);
				num2++;
			}
		}
		StartMove();
	}

	public void ShowThreePvpEnd()
	{
		oneroot.SetActive(false);
		threeroot.SetActive(true);
		fiveroot.SetActive(false);
		SeasonRoot.SetActive(false);
		int count = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
		int num = 0;
		int num2 = 0;
		GameObject[] array = new GameObject[threeLeftPlayer.Length];
		for (int i = 0; i < threeLeftPlayer.Length; i++)
		{
			array[i] = UnityEngine.Object.Instantiate(LeftPlayer, threeLeftPlayer[i].transform);
		}
		GameObject[] array2 = new GameObject[threeRightPlayer.Length];
		for (int j = 0; j < threeRightPlayer.Length; j++)
		{
			array2[j] = UnityEngine.Object.Instantiate(RightPlayer, threeRightPlayer[j].transform);
		}
		InitTeamDmg();
		for (int k = 0; k < count; k++)
		{
			int team = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k].Team;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k].Team == 0)
			{
				OpenWinBackround(k);
				SetPlayerData(array[num], MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k], false, nMVPID[team] == k);
				listLeftMove.Add(array[num]);
				num++;
			}
			else if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k].Team == 1)
			{
				OpenWinBackround(k);
				SetPlayerData(array2[num2], MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k], false, nMVPID[team] == k);
				listRightMove.Add(array2[num2]);
				num2++;
			}
		}
		StartMove();
	}

	public void ShowFivePvpEnd()
	{
		oneroot.SetActive(false);
		threeroot.SetActive(false);
		fiveroot.SetActive(true);
		SeasonRoot.SetActive(false);
		int count = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
		int num = 0;
		int num2 = 0;
		GameObject[] array = new GameObject[fiveLeftPlayer.Length];
		for (int i = 0; i < fiveLeftPlayer.Length; i++)
		{
			array[i] = UnityEngine.Object.Instantiate(LeftPlayer, fiveLeftPlayer[i].transform);
		}
		GameObject[] array2 = new GameObject[fiveRightPlayer.Length];
		for (int j = 0; j < fiveRightPlayer.Length; j++)
		{
			array2[j] = UnityEngine.Object.Instantiate(RightPlayer, fiveRightPlayer[j].transform);
		}
		InitTeamDmg();
		for (int k = 0; k < count; k++)
		{
			int team = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k].Team;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k].Team == 0)
			{
				OpenWinBackround(k);
				SetPlayerData(array[num], MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k], false, nMVPID[team] == k);
				listLeftMove.Add(array[num]);
				num++;
			}
			else if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k].Team == 1)
			{
				OpenWinBackround(k);
				SetPlayerData(array2[num2], MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[k], false, nMVPID[team] == k);
				listRightMove.Add(array2[num2]);
				num2++;
			}
		}
		StartMove();
	}

	private void SetCharacterIconInfo(Image tObj, MemberInfo tMI, int idx)
	{
		string s_ICON = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(tMI.netSealBattleSettingInfo.CharacterList[idx].CharacterID).s_ICON;
		SKIN_TABLE value = null;
		if (tMI.netSealBattleSettingInfo.CharacterList[idx].Skin != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(tMI.netSealBattleSettingInfo.CharacterList[idx].Skin, out value))
		{
			s_ICON = value.s_ICON;
		}
		LoadCutInIcon(s_ICON, tObj);
	}

	public void SetWeaponIconInfo(NetWeaponInfo inputNetWeapon, NetSealBattleSettingInfo netSealBattleSettingInfo, Transform transf)
	{
		if (inputNetWeapon.WeaponID <= 0)
		{
			return;
		}
		WEAPON_TABLE tWeapon_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[inputNetWeapon.WeaponID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			GameObject obj = UnityEngine.Object.Instantiate(asset, transf);
			CommonIconBase component = obj.GetComponent<CommonIconBase>();
			obj.transform.localScale = new Vector3(0.8f, 0.8f, 0f);
			int num = 0;
			foreach (NetWeaponExpertInfo weaponExpert in netSealBattleSettingInfo.WeaponExpertList)
			{
				if (weaponExpert.WeaponID == inputNetWeapon.WeaponID)
				{
					num += weaponExpert.ExpertLevel;
				}
			}
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWeapon_TABLE.s_ICON);
			component.SetOtherInfo(inputNetWeapon, CommonIconBase.WeaponEquipType.UnEquip, true, num, false);
			component.EnableLevel(false);
			component.EnableWeaponRank(false);
		});
	}

	public void ShowSeasonEnd()
	{
		LeftPlayer.SetActive(false);
		RightPlayer.SetActive(false);
		oneroot.SetActive(false);
		threeroot.SetActive(false);
		fiveroot.SetActive(false);
		SeasonRoot.SetActive(true);
		int count = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
		int num = 0;
		int num2 = 0;
		nTotalDmg = new int[2];
		nMVPID = new int[2];
		InitTeamDmg();
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			bool flag2 = false;
			int team = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team;
			TeamPlayerID[team] = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId;
			switch (team)
			{
			case 0:
			{
				LeftNameText.text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId;
				int num6 = 0;
				string playerId2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId;
				if (playerId2 == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					SocketPlayerHUD value3;
					if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(LeftPlayerIcon.transform, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID, new Vector3(0.7f, 0.7f, 0.7f), false);
						SetPlayerSignIcon(PlayerSignRootSeasonL, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID, playerId2);
					}
					else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId2, out value3))
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(LeftPlayerIcon.transform, value3.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
						SetPlayerSignIcon(PlayerSignRootSeasonL, value3.m_TitleNumber, playerId2);
					}
					LeftAddFriendBtn.gameObject.SetActive(false);
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonExpired)
					{
						num6 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore;
					}
					else if (nWinType == 1)
					{
						num6 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_WIN;
					}
					else if (nWinType == 2)
					{
						num6 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_LOSE;
					}
					else
					{
						num6 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_LOSE;
						flag2 = true;
					}
				}
				else
				{
					LeftAddFriendBtn.gameObject.SetActive(!ManagedSingleton<FriendHelper>.Instance.IsFriend(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId));
					num6 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore;
					SocketPlayerHUD value4;
					if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId2, out value4))
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(LeftPlayerIcon.transform, value4.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
						SetPlayerSignIcon(PlayerSignRootSeasonL, value4.m_TitleNumber, playerId2);
					}
					if (nWinType == 1)
					{
						flag2 = true;
					}
				}
				LeftNameText.text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Nickname;
				List<NetCharacterInfo> characterList2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.CharacterList;
				double num7 = 300.0;
				for (int k = 0; k < characterList2.Count; k++)
				{
					if (LeftCharacterStarOnRoot != null && LeftCharacterStarOnRoot.Length == 3)
					{
						UpdateSeasonCharacterStars(LeftCharacterStarOnRoot[k], characterList2[k].Star);
					}
					SetCharacterIconInfo(LeftCharaterObjs[k], MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i], k);
					int characterID2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.CharacterList[k].CharacterID;
					if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonHPInit)
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonAddPlayerDMG(i, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId, 0);
					}
					OrangeBattleServerManager.SeasonCharaterInfo seasonCharaterInfo2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.idcSeasonCharaterInfoList[i][characterID2];
					if (seasonCharaterInfo2.m_Percent > num7)
					{
						seasonCharaterInfo2.m_Percent = num7;
					}
					num7 -= seasonCharaterInfo2.m_Percent;
					LeftHurtPercentText[k].text = seasonCharaterInfo2.m_Percent.ToString("0.0") + "%";
					int num8 = ((k >= MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nNowCharacterID) ? seasonCharaterInfo2.m_HP : 0);
					num8 = ((seasonCharaterInfo2.m_HP >= 0) ? num8 : 0);
					num8 = ((!flag2) ? num8 : 0);
					float y2 = Mathf.Clamp((float)num8 / (float)seasonCharaterInfo2.m_MHP, 0f, 1f);
					LeftHPbarImage[k].transform.localScale = new Vector3(1f, y2, 1f);
					flag = true;
				}
				NetWeaponInfo mainWeaponInfo2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.MainWeaponInfo;
				SetWeaponIconInfo(mainWeaponInfo2, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo, LeftWeaponObjs[0].transform);
				NetWeaponInfo subWeaponInfo2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.SubWeaponInfo;
				SetWeaponIconInfo(subWeaponInfo2, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo, LeftWeaponObjs[1].transform);
				OpenWinBackround(i);
				num++;
				HUNTERRANK_TABLE hunterRankTable2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(num6);
				LeftRankIconBase.GetComponent<RankIconBase>().Setup(hunterRankTable2.n_MAIN_RANK, hunterRankTable2.n_SUB_RANK);
				break;
			}
			case 1:
			{
				bool flag3 = false;
				RightNameText.text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId;
				int num3 = 0;
				string playerId = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId;
				if (playerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					SocketPlayerHUD value;
					if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(RightPlayerIcon.transform, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID, new Vector3(0.7f, 0.7f, 0.7f), false);
						SetPlayerSignIcon(PlayerSignRootSeasonR, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID, playerId);
					}
					else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out value))
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(RightPlayerIcon.transform, value.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
						SetPlayerSignIcon(PlayerSignRootSeasonR, value.m_TitleNumber, playerId);
					}
					RightAddFriendBtn.gameObject.SetActive(false);
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonExpired)
					{
						num3 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore;
					}
					else if (nWinType == 1)
					{
						num3 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_WIN;
					}
					else if (nWinType == 2)
					{
						num3 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_LOSE;
					}
					else
					{
						num3 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_LOSE;
						flag3 = true;
					}
				}
				else
				{
					RightAddFriendBtn.gameObject.SetActive(!ManagedSingleton<FriendHelper>.Instance.IsFriend(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId));
					num3 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.PVPScore;
					SocketPlayerHUD value2;
					if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out value2))
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(RightPlayerIcon.transform, value2.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
						SetPlayerSignIcon(PlayerSignRootSeasonR, value2.m_TitleNumber, playerId);
					}
					if (nWinType == 1)
					{
						flag3 = true;
					}
				}
				RightNameText.text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Nickname;
				List<NetCharacterInfo> characterList = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.CharacterList;
				double num4 = 300.0;
				for (int j = 0; j < characterList.Count; j++)
				{
					if (RightCharacterStarOnRoot != null && RightCharacterStarOnRoot.Length == 3)
					{
						UpdateSeasonCharacterStars(RightCharacterStarOnRoot[j], characterList[j].Star);
					}
					SetCharacterIconInfo(RightCharaterObjs[j], MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i], j);
					int characterID = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.CharacterList[j].CharacterID;
					if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonHPInit)
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonAddPlayerDMG(i, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId, 0);
					}
					OrangeBattleServerManager.SeasonCharaterInfo seasonCharaterInfo = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.idcSeasonCharaterInfoList[i][characterID];
					if (seasonCharaterInfo.m_Percent > num4)
					{
						seasonCharaterInfo.m_Percent = num4;
					}
					num4 -= seasonCharaterInfo.m_Percent;
					RightHurtPercentText[j].text = seasonCharaterInfo.m_Percent.ToString("0.0") + "%";
					int num5 = ((j >= MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nNowCharacterID) ? seasonCharaterInfo.m_HP : 0);
					num5 = ((seasonCharaterInfo.m_HP >= 0) ? num5 : 0);
					num5 = ((!flag3) ? num5 : 0);
					float y = Mathf.Clamp((float)num5 / (float)seasonCharaterInfo.m_MHP, 0f, 1f);
					RightHPbarImage[j].transform.localScale = new Vector3(1f, y, 1f);
					flag = true;
				}
				NetWeaponInfo mainWeaponInfo = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.MainWeaponInfo;
				SetWeaponIconInfo(mainWeaponInfo, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo, RightWeaponObjs[0].transform);
				NetWeaponInfo subWeaponInfo = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo.SubWeaponInfo;
				SetWeaponIconInfo(subWeaponInfo, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo, RightWeaponObjs[1].transform);
				OpenWinBackround(i);
				num2++;
				HUNTERRANK_TABLE hunterRankTable = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(num3);
				RightRankIconBase.GetComponent<RankIconBase>().Setup(hunterRankTable.n_MAIN_RANK, hunterRankTable.n_SUB_RANK);
				break;
			}
			}
		}
		if (count <= 0 || !flag)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_NOT_REACHABLE_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		NewScore = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore;
		OldScore = 0;
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonExpired)
		{
			SeasonScoreText.text = "0";
		}
		else if (nWinType == 1)
		{
			SeasonScoreText.text = "+" + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_WIN;
			OldScore = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore - MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_WIN;
		}
		else
		{
			SeasonScoreText.text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_LOSE.ToString();
			OldScore = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore - MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_PT_LOSE;
		}
		Invoke("OnPlayerRankAnimator", 1f);
		HUNTERRANK_TABLE hunterRankTable3 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(NewScore);
		float num9 = 0f;
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsHunterRankMax(NewScore))
		{
			SeasonNextRankInfoText.text = string.Format("{0}/---", NewScore);
			num9 = 1f;
		}
		else
		{
			SeasonNextRankInfoText.text = string.Format("{0}/{1}", NewScore, hunterRankTable3.n_PT_MAX + 1);
			num9 = Mathf.Clamp((float)NewScore / (float)hunterRankTable3.n_PT_MAX, 0f, 1f);
		}
		SeasonNextRankBar.transform.localScale = new Vector3(num9, 1f, 1f);
		SeasonStartMove();
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonExpired)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_SEASON_END"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
	}

	private void UpdateSeasonCharacterStars(Transform starOnRoot, int starCount)
	{
		if (starOnRoot != null)
		{
			starOnRoot.parent.gameObject.SetActive(true);
			for (int i = 0; i < 5; i++)
			{
				starOnRoot.GetChild(i).gameObject.SetActive(i < starCount);
			}
		}
	}

	public void OnFriendInvite(int team)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(TeamPlayerID[team]);
		});
	}

	public void OnPlayerRankAnimator()
	{
		HUNTERRANK_TABLE hunterRankTable = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(NewScore);
		HUNTERRANK_TABLE currentHunterRankTable = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable;
		for (int i = 0; i < RankAnimatorRoot.Length; i++)
		{
			RankAnimatorRoot[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < RankAnimator.Length; j++)
		{
			RankAnimator[j].gameObject.SetActive(false);
		}
		if (hunterRankTable.n_ID > currentHunterRankTable.n_ID)
		{
			if (hunterRankTable.n_MAIN_RANK > currentHunterRankTable.n_MAIN_RANK)
			{
				RankAnimatorRoot[0].gameObject.SetActive(true);
				RankAnimator[0].gameObject.SetActive(true);
				AnimatorRankImage[0].sprite = RankIconImage[currentHunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankImage[3].sprite = RankIconImage[currentHunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[0].sprite = RankStarIconImage[currentHunterRankTable.n_SUB_RANK].sprite;
				AnimatorRankImage[1].sprite = RankIconImage[hunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankImage[2].sprite = RankIconImage[hunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[1].sprite = RankStarIconImage[hunterRankTable.n_SUB_RANK].sprite;
				RankAnimator[0].Play("UP_rank", 0);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RANKUP);
				bDelayedCall = true;
				LeanTween.delayedCall(4.3f, (Action)delegate
				{
					RankAnimatorRoot[0].gameObject.SetActive(false);
					RankAnimator[0].gameObject.SetActive(false);
					bDelayedCall = false;
				});
			}
			else if (hunterRankTable.n_SUB_RANK > MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_SUB_RANK)
			{
				RankAnimatorRoot[1].gameObject.SetActive(true);
				RankAnimator[2].gameObject.SetActive(true);
				AnimatorRankImage[6].sprite = RankIconImage[currentHunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[4].sprite = RankStarIconImage[currentHunterRankTable.n_SUB_RANK].sprite;
				AnimatorRankImage[7].sprite = RankIconImage[hunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[5].sprite = RankStarIconImage[hunterRankTable.n_SUB_RANK].sprite;
				AnimatorRankStarImage[6].sprite = RankStarIconImage[hunterRankTable.n_SUB_RANK].sprite;
				float length = RankAnimator[2].GetCurrentAnimatorClipInfo(0)[0].clip.length;
				RankAnimator[2].Play("UP_star", 0);
				LeanTween.delayedCall(0.3f, (Action)delegate
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP01);
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
				});
				bDelayedCall = true;
				LeanTween.delayedCall(length, (Action)delegate
				{
					RankAnimatorRoot[1].gameObject.SetActive(false);
					RankAnimator[2].gameObject.SetActive(false);
					bDelayedCall = false;
				});
			}
		}
		else if (hunterRankTable.n_ID < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_ID)
		{
			if (hunterRankTable.n_MAIN_RANK < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_MAIN_RANK)
			{
				RankAnimatorRoot[0].gameObject.SetActive(true);
				RankAnimator[1].gameObject.SetActive(true);
				AnimatorRankImage[4].sprite = RankIconImage[currentHunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[2].sprite = RankStarIconImage[currentHunterRankTable.n_SUB_RANK].sprite;
				AnimatorRankImage[5].sprite = RankIconImage[hunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[3].sprite = RankStarIconImage[hunterRankTable.n_SUB_RANK].sprite;
				float length2 = RankAnimator[1].GetCurrentAnimatorClipInfo(0)[0].clip.length;
				RankAnimator[1].Play("Down", 0);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RANKDOWN);
				bDelayedCall = true;
				LeanTween.delayedCall(length2, (Action)delegate
				{
					RankAnimatorRoot[0].gameObject.SetActive(false);
					RankAnimator[1].gameObject.SetActive(false);
					bDelayedCall = false;
				});
			}
			else if (hunterRankTable.n_SUB_RANK < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable.n_SUB_RANK)
			{
				RankAnimatorRoot[1].gameObject.SetActive(true);
				RankAnimator[3].gameObject.SetActive(true);
				AnimatorRankImage[8].sprite = RankIconImage[currentHunterRankTable.n_MAIN_RANK].sprite;
				AnimatorRankStarImage[7].sprite = RankStarIconImage[currentHunterRankTable.n_SUB_RANK].sprite;
				AnimatorRankStarImage[8].sprite = RankStarIconImage[hunterRankTable.n_SUB_RANK].sprite;
				float length3 = RankAnimator[3].GetCurrentAnimatorClipInfo(0)[0].clip.length;
				RankAnimator[3].Play("Down_star", 0);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RANKDOWN);
				bDelayedCall = true;
				LeanTween.delayedCall(length3, (Action)delegate
				{
					RankAnimatorRoot[1].gameObject.SetActive(false);
					RankAnimator[3].gameObject.SetActive(false);
					bDelayedCall = false;
				});
			}
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore = NewScore;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable = hunterRankTable;
	}

	private void InitTeamDmg()
	{
		nTotalDmg = new int[2];
		nMVPID = new int[2];
		int count = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			int team = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].Team;
			nTotalDmg[team] += MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nALLDMG;
			nMVPID[team] = i;
		}
		for (int j = 0; j < count; j++)
		{
			int team = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[j].Team;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[j].nALLDMG > MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[nMVPID[team]].nALLDMG)
			{
				nMVPID[team] = j;
			}
		}
	}

	private void SetPlayerData(GameObject tObj, MemberInfo tMI, bool IsOne, bool IsMvp)
	{
		tObj.transform.localPosition = Vector3.zero;
		tObj.transform.Find("killimg/MVPLight").gameObject.SetActive(IsMvp);
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(tMI.netSealBattleSettingInfo.CharacterList[0].CharacterID);
		Transform transform = tObj.transform.Find("Image/Image/PlayerImgRoot");
		string s_ICON = characterTable.s_ICON;
		SKIN_TABLE value = null;
		if (tMI.netSealBattleSettingInfo.CharacterList[0].Skin != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(tMI.netSealBattleSettingInfo.CharacterList[0].Skin, out value))
		{
			s_ICON = value.s_ICON;
		}
		Image componentInChildren = transform.GetComponentInChildren<Image>(true);
		if ((bool)componentInChildren)
		{
			LoadCutInIcon(s_ICON, componentInChildren);
		}
		Transform transform2 = tObj.transform.Find("Image/Image/PlayerImgRoot/imgStarRoot");
		Transform transform3 = tObj.transform.Find("Image/Image/PlayerImgRoot/imgStarRoot/imgStarOnGroup");
		if ((bool)transform2 && (bool)transform3)
		{
			transform2.gameObject.SetActive(true);
			int star = tMI.netSealBattleSettingInfo.CharacterList[0].Star;
			for (int i = 0; i < 5; i++)
			{
				transform3.GetChild(i).gameObject.SetActive(i < star);
			}
		}
		tObj.transform.Find("lifebg").gameObject.SetActive(IsOne);
		if (IsOne)
		{
			Transform transform4 = tObj.transform.Find("lifebg/lifepercent");
			OrangeCharacter playerByID = StageUpdate.GetPlayerByID(tMI.PlayerId);
			if (playerByID != null)
			{
				transform4.GetComponent<Text>().text = ((float)(int)playerByID.Hp * 100f / (float)(int)playerByID.MaxHp).ToString("0") + "%";
			}
			else
			{
				transform4.GetComponent<Text>().text = "0%";
			}
		}
		bool flag = false;
		flag = ((tMI.Team != MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam() - 1) ? (nWinType != 1) : (nWinType == 1));
		tObj.transform.Find("mvpbgwin").gameObject.SetActive(!IsOne && IsMvp && flag);
		tObj.transform.Find("mvpbglost").gameObject.SetActive(!IsOne && IsMvp && !flag);
		if (!IsOne && IsMvp)
		{
			Transform transform5 = tObj.transform.Find("mvpbgwin/ScoreMVP");
			if (nTotalDmg[tMI.Team] > 0)
			{
				transform5.GetComponent<Text>().text = tMI.nALLDMG * 100 / nTotalDmg[tMI.Team] + "%";
			}
			else
			{
				transform5.GetComponent<Text>().text = "0%";
			}
			transform5 = tObj.transform.Find("mvpbglost/ScoreMVP");
			if (nTotalDmg[tMI.Team] > 0)
			{
				transform5.GetComponent<Text>().text = tMI.nALLDMG * 100 / nTotalDmg[tMI.Team] + "%";
			}
			else
			{
				transform5.GetComponent<Text>().text = "0%";
			}
		}
		tObj.transform.Find("unmvpbg").gameObject.SetActive(!IsOne && !IsMvp);
		if (!IsOne && !IsMvp)
		{
			Transform transform6 = tObj.transform.Find("unmvpbg/Score");
			if (nTotalDmg[tMI.Team] > 0)
			{
				transform6.GetComponent<Text>().text = tMI.nALLDMG * 100 / nTotalDmg[tMI.Team] + "%";
			}
			else
			{
				transform6.GetComponent<Text>().text = "0%";
			}
		}
		Transform transform7 = tObj.transform.Find("WeaponIcon1");
		CommonIconBase component = UnityEngine.Object.Instantiate(refCommonSmallIcon, transform7.transform).GetComponent<CommonIconBase>();
		if (tMI.netSealBattleSettingInfo.MainWeaponInfo.WeaponID != 0)
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tMI.netSealBattleSettingInfo.MainWeaponInfo.WeaponID];
			int num = 0;
			foreach (NetWeaponExpertInfo weaponExpert in tMI.netSealBattleSettingInfo.WeaponExpertList)
			{
				if (weaponExpert.WeaponID == tMI.netSealBattleSettingInfo.MainWeaponInfo.WeaponID)
				{
					num += weaponExpert.ExpertLevel;
				}
			}
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
			component.SetOtherInfo(tMI.netSealBattleSettingInfo.MainWeaponInfo, CommonIconBase.WeaponEquipType.UnEquip, true, num, false);
		}
		else
		{
			component.Setup(0, "", "");
			component.SetOtherInfo(null, CommonIconBase.WeaponEquipType.UnEquip);
		}
		component.EnableLevel(false);
		component.EnableWeaponRank(false);
		Transform transform8 = tObj.transform.Find("WeaponIcon2");
		component = UnityEngine.Object.Instantiate(refCommonSmallIcon, transform8.transform).GetComponent<CommonIconBase>();
		if (tMI.netSealBattleSettingInfo.SubWeaponInfo.WeaponID != 0)
		{
			WEAPON_TABLE wEAPON_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tMI.netSealBattleSettingInfo.SubWeaponInfo.WeaponID];
			int num2 = 0;
			foreach (NetWeaponExpertInfo weaponExpert2 in tMI.netSealBattleSettingInfo.WeaponExpertList)
			{
				if (weaponExpert2.WeaponID == tMI.netSealBattleSettingInfo.SubWeaponInfo.WeaponID)
				{
					num2 += weaponExpert2.ExpertLevel;
				}
			}
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE2.s_ICON);
			component.SetOtherInfo(tMI.netSealBattleSettingInfo.SubWeaponInfo, CommonIconBase.WeaponEquipType.UnEquip, true, num2, false);
		}
		else
		{
			component.Setup(0, "", "");
			component.SetOtherInfo(null, CommonIconBase.WeaponEquipType.UnEquip);
		}
		component.EnableLevel(false);
		component.EnableWeaponRank(false);
		tObj.transform.Find("playername").GetComponent<Text>().text = tMI.Nickname;
		tObj.transform.Find("killnum").GetComponent<Text>().text = tMI.nKillNum.ToString();
		Button component2 = tObj.transform.Find("AddFriendBtn").GetComponent<Button>();
		if (tMI.PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			component2.gameObject.SetActive(false);
			Transform signRoot = tObj.transform.Find("PlayerSignRoot");
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo != null && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				SetPlayerSignIcon(signRoot, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID, tMI.PlayerId);
			}
			else
			{
				SetPlayerSignIcon(signRoot, 0, tMI.PlayerId);
			}
		}
		else
		{
			if (ManagedSingleton<FriendHelper>.Instance.IsFriend(tMI.PlayerId))
			{
				component2.gameObject.SetActive(false);
			}
			else
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = tMI.PlayerId;
				loadCallBackObj.lcb = AddFriendCall;
				component2.onClick.AddListener(loadCallBackObj.LoadCBNoParam);
				component2.gameObject.SetActive(true);
			}
			Transform signRoot2 = tObj.transform.Find("PlayerSignRoot");
			SetPlayerSignIcon(signRoot2, 0, tMI.PlayerId, true);
		}
		tObj.SetActive(true);
	}

	private void AddFriendCall(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
	{
		string sPlayerID = tObj.loadStageObjData as string;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(sPlayerID);
		});
	}

	public void Setup(int nSetWinType)
	{
		Init();
		if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType == PVPMatchType.FriendOneVSOne)
		{
			foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
			{
				Debug.Log("FriendPVPHostID: " + MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID);
				if (item.PlayerId == MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID)
				{
					hostMemberInfo = item;
				}
				else
				{
					guestMemberInfo = item;
				}
			}
			if ((bool)ContinueBtn)
			{
				ContinueBtn.gameObject.SetActive(false);
			}
			if ((bool)BackToPVPRoomBtn)
			{
				BackToPVPRoomBtn.gameObject.SetActive(true);
			}
			if ((bool)BackToPVPRoomCountdown)
			{
				BackToPVPRoomCountdown.transform.parent.gameObject.SetActive(true);
				int pVP_RETURN_TIME = OrangeConst.PVP_RETURN_TIME;
				countdownTweenId = LeanTween.value(pVP_RETURN_TIME, 0f, pVP_RETURN_TIME).setOnUpdate(delegate(float f)
				{
					BackToPVPRoomCountdown.text = ((int)f).ToString();
				}).setOnComplete((Action)delegate
				{
					OnClickCloseBtn();
				})
					.uniqueId;
			}
		}
		nWinType = nSetWinType;
		MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
		if (nWinType == 1)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 44);
		}
		else if (nWinType == 0 || nWinType == 2)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 37);
		}
		int num = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
		bIsSeason = false;
		if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
		{
			STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID];
			if (sTAGE_TABLE.n_MAIN == 90000 && sTAGE_TABLE.n_SUB == 1)
			{
				num = 0;
				bIsSeason = true;
			}
		}
		switch (num)
		{
		case 2:
			ShowOnePvpEnd();
			break;
		case 6:
			ShowThreePvpEnd();
			break;
		case 10:
			ShowFivePvpEnd();
			break;
		case 0:
			ShowSeasonEnd();
			break;
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT01);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public override void OnClickCloseBtn()
	{
		if (bDelayedCall)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			if (bIsSeason)
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.SEASON;
			}
			else
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.PVPROOMSELECT;
			}
			base.OnClickCloseBtn();
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.BLACK, delegate
			{
				StageUpdate.go2home();
			});
		});
	}

	public void OnClickReMathBtn()
	{
		if (bDelayedCall)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			if (bIsSeason)
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.SEASONRANDOMMATCHING;
			}
			else
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.PVPRANDOMMATCHING;
			}
			base.OnClickCloseBtn();
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.BLACK, delegate
			{
				StageUpdate.go2home();
			}, false);
		}, OrangeSceneManager.LoadingType.TIP);
	}

	public void OnClickBackToPVPRoom()
	{
		if (bDelayedCall)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			if (hostMemberInfo.PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.FRIENDPVPCREATEPRIVATEROOM;
			}
			else
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.FRIENDPVPJOINPRIVATEROOM;
			}
			base.OnClickCloseBtn();
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.BLACK, delegate
			{
				StageUpdate.go2home();
			});
		});
	}

	private void LoadCutInIcon(string iconName, Image img)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter2("icon_" + iconName), "icon_" + iconName, delegate(Sprite spr)
		{
			if (spr != null)
			{
				img.sprite = spr;
				img.color = Color.white;
			}
			else
			{
				img.color = Color.clear;
			}
		});
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref countdownTweenId);
	}
}
