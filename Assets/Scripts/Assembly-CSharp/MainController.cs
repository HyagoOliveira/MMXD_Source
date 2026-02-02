#define RELEASE
using System.Collections;
using Line.LineSDK;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
	public Image userIconImage;

	public Text displayNameText;

	public Text statusMessageText;

	public Text rawJsonText;

	public void Login()
	{
		string[] scopes = new string[1] { "profile" };
		LineSDK.Instance.Login(scopes, delegate(Result<LoginResult> result)
		{
			result.Match(delegate(LoginResult value)
			{
				StartCoroutine(UpdateProfile(value.UserProfile));
				UpdateRawSection(value);
			}, delegate(Error error)
			{
				UpdateRawSection(error);
			});
		});
	}

	public void GetProfile()
	{
		LineAPI.GetProfile(delegate(Result<UserProfile> result)
		{
			result.Match(delegate(UserProfile value)
			{
				StartCoroutine(UpdateProfile(value));
				UpdateRawSection(value);
			}, delegate(Error error)
			{
				UpdateRawSection(error);
			});
		});
	}

	public void GetCurrentToken()
	{
		StoredAccessToken currentAccessToken = LineSDK.Instance.CurrentAccessToken;
		UpdateRawSection(currentAccessToken);
	}

	public void VerifyToken()
	{
		LineAPI.VerifyAccessToken(delegate(Result<AccessTokenVerifyResult> result)
		{
			result.Match(delegate(AccessTokenVerifyResult value)
			{
				UpdateRawSection(value);
			}, delegate(Error error)
			{
				UpdateRawSection(error);
			});
		});
	}

	public void RefreshToken()
	{
		LineAPI.RefreshAccessToken(delegate(Result<AccessToken> result)
		{
			result.Match(delegate(AccessToken value)
			{
				UpdateRawSection(value);
			}, delegate(Error error)
			{
				UpdateRawSection(error);
			});
		});
	}

	public void GetFriendshipStatus()
	{
		LineAPI.GetBotFriendshipStatus(delegate(Result<BotFriendshipStatus> result)
		{
			result.Match(delegate(BotFriendshipStatus value)
			{
				UpdateRawSection(value);
			}, delegate(Error error)
			{
				UpdateRawSection(error);
			});
		});
	}

	public void Logout()
	{
		LineSDK.Instance.Logout(delegate(Result<Unit> result)
		{
			result.Match(delegate
			{
				ResetProfile();
			}, delegate(Error error)
			{
				UpdateRawSection(error);
			});
		});
	}

    [System.Obsolete]
    private IEnumerator UpdateProfile(UserProfile profile)
	{
		if (profile.PictureUrl != null)
		{
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(profile.PictureUrl);
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError)
			{
				Debug.LogError(www.error);
			}
			else
			{
				Texture2D content = DownloadHandlerTexture.GetContent(www);
				userIconImage.color = Color.white;
				userIconImage.sprite = Sprite.Create(content, new Rect(0f, 0f, content.width, content.height), new Vector2(0f, 0f));
			}
		}
		else
		{
			yield return null;
		}
		displayNameText.text = profile.DisplayName;
		statusMessageText.text = profile.StatusMessage;
	}

	private void ResetProfile()
	{
		userIconImage.color = Color.gray;
		userIconImage.sprite = null;
		displayNameText.text = "Display Name";
		statusMessageText.text = "Status Message";
	}

	private void UpdateRawSection(object obj)
	{
		if (obj == null)
		{
			rawJsonText.text = "null";
			return;
		}
		string text = JsonUtility.ToJson(obj);
		if (text == null)
		{
			rawJsonText.text = "Invalid Object";
			return;
		}
		rawJsonText.text = text;
		((RectTransform)rawJsonText.gameObject.transform.parent).localPosition = Vector3.zero;
	}
}
