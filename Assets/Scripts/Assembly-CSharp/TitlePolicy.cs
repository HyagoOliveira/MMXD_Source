using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class TitlePolicy : MonoBehaviour
{
	private class Policy
	{
		public string content { get; set; }
	}

	[SerializeField]
	private Text policyText;

	private Policy policy;

	private void Start()
	{
		StartCoroutine(GetPolicyFromNet());
	}

    [System.Obsolete]
    private IEnumerator GetPolicyFromNet()
	{
		string url = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Policy;
		using (WWW www = new WWW(url))
		{
			yield return www;
			policy = JsonConvert.DeserializeObject<Policy>(www.text);
			policyText.text = policy.content;
		}
	}
}
