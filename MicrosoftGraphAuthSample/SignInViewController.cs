using Foundation;
using System;
using UIKit;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;

namespace MicrosoftGraphAuthSample
{
    public partial class SignInViewController : UIViewController
    {
		static readonly string Authority = "https://login.microsoftonline.com/common";
		// Currently there's no other scope available out of openid: http://stackoverflow.com/a/32614100
		// Just authentication actually
		static readonly string Scope = string.Empty;
		static readonly string ClientId = "b440f986-4950-46b7-b69f-c1fb09da7ecc";
		static readonly Uri RedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

		string _accessToken;
		AuthenticationContext _authenticationContext;

        public SignInViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			MicrosoftAccountButton.TouchUpInside += LaunchMicrosoftAuth;
		}

		async void LaunchMicrosoftAuth (object sender, EventArgs e)
		{
			var signInSuccessful = true;

			try {
				await SignInAsync ();
			} catch (Exception ex) {
				signInSuccessful = false;

				var alertView = new UIAlertView {
					Title = "Ops!",
					Message = ex.Message
				};
				alertView.AddButton ("OK...");
				alertView.Show ();
			}

			if (signInSuccessful) {
				var storyboard = UIStoryboard.FromName ("Main", null);
				var homeViewController = storyboard.InstantiateViewController ("Home");
				PresentModalViewController (homeViewController, true);
			}
		}

		async Task SignInAsync ()
		{
			try {
				if (_authenticationContext == null)
					_authenticationContext = new AuthenticationContext (Authority, false);

				await RetrieveAccessTokenAndUserInfoAsync ();
			} catch (AdalException ex) when (ex.ErrorCode == AdalError.AuthenticationCanceled) {
				// User taps on Cancel. We do nothing
			} catch (AdalServiceException ex) when (ex.ErrorCode == "access_denied") {
				// TODO
			} catch (Exception ex) {
				throw new Exception (string.Format("There is an issue with the sign-in setup, more specifically: \n\n{0}\n\n" +
				                                   "Please, try again later. Thank you for your patience",
				                                   ex.Message));
			}
		}

		async Task RetrieveAccessTokenAndUserInfoAsync ()
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
			} catch (Exception) {
				errorAcquiringToken = true;
			}

			if (errorAcquiringToken) {
				try {
					//var userCredential = new UserCredential (ClientId, ClientSecret);
					var authenticationParentUiContext = new PlatformParameters (this);
					result = await _authenticationContext.AcquireTokenAsync (
						new string [] { Scope },
						null,
						clientId: ClientId,
						redirectUri: RedirectUri,
						parameters: authenticationParentUiContext);

					if (result != null && !string.IsNullOrWhiteSpace (result.Token))
						_accessToken = result.Token;
				} catch (Exception e) {
					throw;
				}
			}
		}

		//async Task<byte []> GetUserPhotoAsync ()
		//{
		//	byte [] result = null;

		//	try {
		//		string url = "https://graph.microsoft.com/v1.0/me/photo/$value";

		//		using (var client = new HttpClient ()) {
		//			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Bearer", accessToken);
		//			var stream = await client.GetStreamAsync (url);
		//			result = ReadStream (stream);
		//		}
		//	} catch (Exception) {
		//		result = null;
		//	}

		//	return result;
		//}
    }
}