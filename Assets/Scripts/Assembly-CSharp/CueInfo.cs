using System;
using System.Text.RegularExpressions;

[Serializable]
public class CueInfo
{
	private static readonly Regex s_StopRegex = new Regex("_stop.*$", RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly Regex s_LoopRegex = new Regex("_lp$|_lg$", RegexOptions.Compiled | RegexOptions.Singleline);

	public string sAcb = "";

	public string sCueName = "";

	public string[] sParseCUE;

	public string sFullKey = "";

	public string sCueKey = "";

	public string sHalfKey = "";

	public CueType eType;

	private int eIdx;

	public string EndStr
	{
		get
		{
			return sParseCUE[eIdx];
		}
	}

	public CueInfo()
	{
	}

	public CueInfo(string acb, string cuename)
	{
		Parse(acb, cuename);
	}

	public void Parse(string acb, string cuename)
	{
		sAcb = acb;
		sCueKey = (sCueName = cuename);
		sParseCUE = sCueName.Split('_');
		eIdx = sParseCUE.Length - 1;
		eType = GetCueType(sCueName);
		if (eType != 0)
		{
			sCueKey = sCueName.Substring(0, sCueName.Length - sParseCUE[eIdx].Length - 1);
		}
		sFullKey = sAcb + "," + sCueName;
		sHalfKey = sAcb + "," + sCueKey;
	}

	public static CueType GetCueType(string cuename)
	{
		if (s_LoopRegex.IsMatch(cuename))
		{
			return CueType.CT_LOOP;
		}
		if (s_StopRegex.IsMatch(cuename))
		{
			return CueType.CT_STOP;
		}
		return CueType.CT_NORMAL;
	}
}
