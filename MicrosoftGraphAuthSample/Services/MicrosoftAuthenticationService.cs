using System;
using System.Diagnostics.Contracts;
using System.Linq;
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
			AppDelegate.Storage.Delete (AppDelegate.TokenCacheKey);
		}

		internal static async Task<bool> SignInSilentlyAsync ()
		{
			var successfulSignIn = false;

			try {
				var serializedTokenCache = AppDelegate.Storage.Get<byte[]> (AppDelegate.TokenCacheKey);

				if (serializedTokenCache == null)
					throw new Exception ("There was previously a sign out. We need to sign in again");

				if (_authenticationContext == null) {
					var tokenCache = new TokenCache (serializedTokenCache);
					_authenticationContext = new AuthenticationContext (Authority, false, tokenCache);
				}

				var result = await _authenticationContext.AcquireTokenSilentAsync (
					new string [] { Scope },
					clientId: ClientId);

				if (result != null) {
					SaveTokenCache ();
					successfulSignIn = true;
				}
			} catch {
			}

			return successfulSignIn;
		}

		static void SaveTokenCache ()
		{
			var previousTokenCache = AppDelegate.Storage.Get<byte[]> (AppDelegate.TokenCacheKey);
			var serializedTokenCache = _authenticationContext.TokenCache.Serialize ();

			if (previousTokenCache != null &&
			    serializedTokenCache != null &&
			    previousTokenCache.Length == serializedTokenCache.Length &&
			    previousTokenCache.SequenceEqual (serializedTokenCache))
				return;

			AppDelegate.Storage.Put (AppDelegate.TokenCacheKey, serializedTokenCache);
		}

		static async Task RetrieveAccessTokenAndUserInfoAsync (UIViewController callerController)
		{
			var successfulSignIn = await SignInSilentlyAsync ();

			if (successfulSignIn)
				return;

			try {
				var authenticationParentUiContext = new PlatformParameters (callerController);
				var result = await _authenticationContext.AcquireTokenAsync (
					new string [] { Scope },
					null,
					clientId: ClientId,
					redirectUri: RedirectUri,
					parameters: authenticationParentUiContext);

				if (result != null)
					SaveTokenCache ();
			} catch {
				throw;
			}
		}
	}
}

