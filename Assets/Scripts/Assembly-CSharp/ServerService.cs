#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ServerService<T> : MonoBehaviourSingleton<T> where T : MonoBehaviour
{
	private string serviceToken = "";

	public int SeqID;

	private DateTime lastResponseTime = DateTime.Now;

	protected RequestCommand lastRequestCommand;

	protected Queue<RequestCommand> queRequest = new Queue<RequestCommand>();

	private WaitForSecondsRealtime waitforSecRealtime;

	public string ServiceToken
	{
		get
		{
			return serviceToken;
		}
		set
		{
			Debug.Log(string.Format("My ServiceToken is {0}.", value));
			serviceToken = value;
		}
	}

	public string ServerUrl { get; set; }

	public void Awake()
	{
		waitforSecRealtime = new WaitForSecondsRealtime((float)HttpSetting.MinRequestDuration / 1000f);
	}

	public bool ContainsRequest(Type type)
	{
		if (lastRequestCommand != null && lastRequestCommand.serverRequest.GetType() == type)
		{
			return true;
		}
		if (queRequest != null && queRequest.Count > 0)
		{
			foreach (RequestCommand item in queRequest)
			{
				if (item.serverRequest.GetType() == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void SendRequest<U>(IRequest req, Action<U> cb = null, bool errorRtnTitle = false) where U : IResponse
	{
		SendCommand(req, typeof(U), delegate(IResponse res)
		{
			if (cb != null)
			{
				cb((U)res);
			}
		}, errorRtnTitle);
	}

	protected void SendCommand(IRequest req, Type resType, Action<IResponse> responseCallback = null, bool errorRtnTitle = false)
	{
		RequestCommand item = new RequestCommand
		{
			callbackEvent = responseCallback,
			serverRequest = req,
			responseType = resType,
			errorReturnTitle = errorRtnTitle
		};
		queRequest.Enqueue(item);
		SendNextCommand();
	}

	protected void SendNextCommand()
	{
		if (lastRequestCommand == null && queRequest.Count > 0)
		{
			StartCoroutine(BeginCommand(queRequest.Dequeue()));
		}
	}

    [Obsolete]
    protected IEnumerator BeginCommand(RequestCommand cmd)
	{
		if (string.IsNullOrEmpty(ServerUrl))
		{
			Debug.LogError("ServerUrl is not Initialized");
			yield break;
		}
		cmd.serverRequest.SeqID = SeqID;
		lastRequestCommand = cmd;
		SendingRequest(true);
		if (HttpSetting.MinRequestDuration - (DateTime.Now - lastResponseTime).TotalMilliseconds > 0.0)
		{
			yield return waitforSecRealtime;
		}
		IRequest serverRequest = cmd.serverRequest;
		string encryptString = JsonHelper.Serialize(serverRequest);
		string s = AesCrypto.Encode(encryptString);
		string uri = string.Format("{0}/{1}", ServerUrl.TrimEnd('/'), serverRequest.GetType().Name);
		UnityWebRequest www = UnityWebRequest.Put(uri, Encoding.ASCII.GetBytes(s));
		www.method = "POST";
		www.timeout = HttpSetting.Timeout;
		www.SetRequestHeader("authorization", serviceToken);
		www.SetRequestHeader("Content-Type", "text/plain");
		www.SetRequestHeader("user-agent", "");
		Debug.Log(string.Format("Client送出HTTP封包, Request={0}, URL={1}", serverRequest.GetType(), www.url));
		yield return www.SendWebRequest();
		while (!www.isDone)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		lastResponseTime = DateTime.Now;
		lastRequestCommand = null;
		SendingRequest(false);
		if (www.isNetworkError)
		{
			WWWNetworkError(cmd);
			yield break;
		}
		if (www.isHttpError)
		{
			WWWRequestError(www, cmd);
			yield break;
		}
		byte[] data = www.downloadHandler.data;
		Debug.Log("Client收到Server的回傳封包, length=" + data.Length);
		try
		{
			byte[] bytes = LZ4Helper.DecodeWithoutHeader(data);
			encryptString = AesCrypto.Decode(Encoding.ASCII.GetString(bytes));
			SeqID++;
			if (cmd.responseType != null)
			{
				object obj = JsonHelper.Deserialize(encryptString, cmd.responseType);
				ParseServerResponse(cmd, obj as IResponse);
			}
			else
			{
				Debug.Log("未設定DeserializeObject的類型, Request=" + cmd.serverRequest.GetType());
			}
			SendNextCommand();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			WWWDecryptError(cmd);
		}
	}

	protected abstract void SendingRequest(bool isSending);

	protected abstract void ParseServerResponse(RequestCommand cmd, IResponse res);

	protected abstract bool WWWRequestError(UnityWebRequest www, RequestCommand cmd);

	protected abstract void WWWNetworkError(RequestCommand cmd);

	protected abstract void WWWDecryptError(RequestCommand cmd);
}
