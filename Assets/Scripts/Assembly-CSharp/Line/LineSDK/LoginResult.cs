using UnityEngine;

namespace Line.LineSDK
{
	public class LoginResult
	{
		[SerializeField]
		private AccessToken accessToken;

		[SerializeField]
		private string scope;

		[SerializeField]
		private UserProfile userProfile;

		[SerializeField]
		private bool friendshipStatusChanged;

		[SerializeField]
		private string IDTokenNonce;

		public AccessToken AccessToken
		{
			get
			{
				return accessToken;
			}
		}

		public string[] Scopes
		{
			get
			{
				return scope.Split(' ');
			}
		}

		public UserProfile UserProfile
		{
			get
			{
				return userProfile;
			}
		}

		public bool IsFriendshipStatusChanged
		{
			get
			{
				return friendshipStatusChanged;
			}
		}

		public string IdTokenNonce
		{
			get
			{
				return IDTokenNonce;
			}
		}
	}
}
