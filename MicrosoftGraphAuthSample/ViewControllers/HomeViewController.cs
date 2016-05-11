using Foundation;
using System;
using UIKit;

namespace MicrosoftGraphAuthSample
{
    public partial class HomeViewController : UIViewController
    {
        public HomeViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// TODO: Clear token
			SignOutButton.TouchUpInside += SignOutAndNavigateBacktoSignIn;
		}

		void SignOutAndNavigateBacktoSignIn (object sender, EventArgs e)
		{
			MicrosoftAuthenticationService.SignOut ();
			DismissModalViewController (true);
		}
	}
}