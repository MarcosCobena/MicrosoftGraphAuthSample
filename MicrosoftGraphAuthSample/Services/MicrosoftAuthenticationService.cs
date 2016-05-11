using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;
using UIKit;

namespace MicrosoftGraphAuthSample
{
	public static class MicrosoftAuthenticationService
	{
		static readonly string Authority = "https://login.microsoftonline.com/common";
		// Currently there's no other scope available out of openid: http://stackoverflow.com/a/32614100
		// Just authentication actually
		static readonly string Scope = string.Empty;
		static readonly string ClientId = "b440f986-4950-46b7-b69f-c1fb09da7ecc";
		static readonly Uri RedirectUri = new Uri ("urn:ietf:wg:oauth:2.0:oob");

		static string _accessToken;
		static AuthenticationContext _authenticationContext;

		internal static async Task<bool> SignInAsync (UIViewController callerController)
		{
			Contract.Requires (callerController != null);

			var successfulSignIn = false;

			try {
				if (_authenticationContext == null)
					_authenticationContext = new AuthenticationContext (Authority, false);

				await RetrieveAccessTokenAndUserInfoAsync (callerController);
				successfulSignIn = true;
			} catch (AdalException ex) when (ex.ErrorCode == AdalError.AuthenticationCanceled) {
				// User taps on Cancel. We do nothing
			} catch (AdalServiceException ex) when (ex.ErrorCode == "access_denied") {
				throw new Exception ("It seems you rejected the access to this app. " +
				                     "You may want to try again, " +
				                     "we will take care of your info, promised");
			} catch (Exception ex) {
				throw new Exception ("There is an issue with the sign-in setup, " +
				                     $"more specifically: \n\n{ex.Message}\n\n" +
							   		 "Please, try again later. Thank you for your patience");
			}

			return successfulSignIn;
		}

		internal static void SignOut ()
		{
			_authenticationContext.TokenCache.Clear ();
			_accessToken = null;
		}

		static async Task RetrieveAccessTokenAndUserInfoAsync (UIViewController callerController)
		{
			var errorAcquiringToken = false;
			AuthenticationResult result = null;

			try {
				if (_accessToken == null)
					throw new Exception ("There was previously a sign out. We need to sign in again");

				result = await _authenticationContext.AcquireTokenSilentAsync (
					new string [] { Scope },
					clientId: ClientId);

				if (result != null && !string.IsNullOrWhiteSpace (result.Token))
					_accessToken = result.Token;
			} catch {
				errorAcquiringToken = true;
			}

			if (errorAcquiringToken) {
				try {
					var authenticationParentUiContext = new PlatformParameters (callerController);
					result = await _authenticationContext.AcquireTokenAsync (
						new string [] { Scope },
						null,
						clientId: ClientId,
						redirectUri: RedirectUri,
						parameters: authenticationParentUiContext);

					if (result != null && !string.IsNullOrWhiteSpace (result.Token))
						_accessToken = result.Token;
				} catch {
					throw;
				}
			}
		}
	}
}

