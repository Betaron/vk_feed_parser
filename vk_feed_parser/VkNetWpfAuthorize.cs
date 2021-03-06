using System.Threading.Tasks;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Core;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Utils;
using Url = Flurl.Url;

namespace vk_feed_parser
{
	public class WpfAuthorize : IAuthorizationFlow
	{
		/// <summary>
		/// Менеджер версий VkApi
		/// </summary>
		private readonly IVkApiVersionManager _versionManager;

		private IApiAuthParams _authParams;

		public WpfAuthorize(IVkApiVersionManager versionManager)
		{
			_versionManager = versionManager;
		}

		[System.Obsolete]
		public Task<AuthorizationResult> AuthorizeAsync()
		{
			var dlg = new  Windows.BrowserLoginWindow();

			dlg.Browser.Address =  CreateAuthorizeUrl(_authParams.ApplicationId, _authParams.Settings.ToUInt64(), Display.Mobile, "123456");

			dlg.Browser.AddressChanged += (sender, args) =>
			{
				dlg.Browser.SetSilent();
				var result = VkAuthorization2.From(args.NewValue.ToString());

				if (!result.IsAuthorized)
				{
					return;
				}

				dlg.Auth = new AuthorizationResult
				{
					AccessToken = result.AccessToken,
					ExpiresIn = result.ExpiresIn,
					UserId = result.UserId,
					State = result.State
				};

				dlg.Close();
			};

			dlg.ShowDialog();

			return Task.FromResult(dlg.Auth);
		}

		public void SetAuthorizationParams(IApiAuthParams authorizationParams)
		{
			_authParams = authorizationParams;
		}

		public Url CreateAuthorizeUrl()
		{
			var url = new Url("https://oauth.vk.com/authorize")
				.SetQueryParam("client_id", _authParams.ApplicationId)
				.SetQueryParam("redirect_uri", "https://oauth.vk.com/blank.html")
				.SetQueryParam("display", Display.Mobile)
				.SetQueryParam("scope", _authParams.Settings.ToUInt64())
				.SetQueryParam("response_type", "token")
				.SetQueryParam("v", _versionManager.Version)
				.SetQueryParam("state", "1234567")
				.SetQueryParam("revoke", "1");

			return url;
		}

		public Url CreateAuthorizeUrl(ulong clientId, ulong scope, Display display, string state)
		{
			return CreateAuthorizeUrl().ToUri();
		}
	}
}