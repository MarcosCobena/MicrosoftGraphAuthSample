using System;
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
				var storyboard = UIStoryboard.FromName ("Main", null);
				var homeViewController = storyboard.InstantiateViewController ("Home");
				await PresentViewControllerAsync (homeViewController, true);
			}

			SignInActivityIndicator.StopAnimating ();
			MicrosoftAccountButton.Enabled = true;
		}
    }
}