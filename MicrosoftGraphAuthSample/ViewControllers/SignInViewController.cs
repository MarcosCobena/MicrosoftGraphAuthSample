using System;
using System.Threading.Tasks;
using PerpetualEngine.Storage;
using UIKit;

namespace MicrosoftGraphAuthSample
{
	public partial class SignInViewController : UIViewController
    {
		public SignInViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			SignInActivityIndicator.HidesWhenStopped = true;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			MicrosoftAccountButton.TouchUpInside += LaunchMicrosoftAuth;

			TrySignInSilentlyIfThereWasAPreviousSignInAsync ().ConfigureAwait(false);
		}

		async Task TrySignInSilentlyIfThereWasAPreviousSignInAsync ()
		{
			var previousSuccessfulSignIn = AppDelegate.Storage.Get<bool> (AppDelegate.SuccessfulSignInKey);

			if (!previousSuccessfulSignIn)
				return;

			SignInActivityIndicator.StartAnimating ();
			MicrosoftAccountButton.Enabled = false;

			var currentSuccessfulSignIn = await MicrosoftAuthenticationService.SignInSilentlyAsync ();

			if (currentSuccessfulSignIn)
				await NavigateToMainAsync ();

			SignInActivityIndicator.StopAnimating ();
			MicrosoftAccountButton.Enabled = true;
		}

		async void LaunchMicrosoftAuth (object sender, EventArgs e)
		{
			SignInActivityIndicator.StartAnimating ();
			MicrosoftAccountButton.Enabled = false;

			var signInSuccessful = false;

			try {
				signInSuccessful = await MicrosoftAuthenticationService.SignInAsync (this);
			} catch (Exception ex) {
				var alertView = new UIAlertView {
					Title = "Ops!",
					Message = ex.Message
				};
				alertView.AddButton ("OK...");
				alertView.Show ();
			}

			if (signInSuccessful) {
				AppDelegate.Storage.Put (AppDelegate.SuccessfulSignInKey, true);
				await NavigateToMainAsync ();
			}

			SignInActivityIndicator.StopAnimating ();
			MicrosoftAccountButton.Enabled = true;
		}

		async Task NavigateToMainAsync ()
		{
			var storyboard = UIStoryboard.FromName ("Main", null);
			var homeViewController = storyboard.InstantiateViewController ("Home");
			await PresentViewControllerAsync (homeViewController, true);
		}
	}
}