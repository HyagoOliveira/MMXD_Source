using TwitterKit.Unity;

namespace TwitterKit.Internal
{
	internal interface ITwitter
	{
		void Init(string consumerKey, string consumerSecret);

		void LogIn();

		void LogOut();

		TwitterSession Session();

		void RequestEmail(TwitterSession session);

		void Compose(TwitterSession session, string imageUri, string text, string[] hashtags);
	}
}
