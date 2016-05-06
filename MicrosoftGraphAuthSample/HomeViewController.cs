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

			SignOutButton.TouchUpInside += (object sender, EventArgs e) => DismissModalViewController(true);

			AvatarImageView.Layer.CornerRadius = AvatarImageView.Frame.Width / 2;
		}
    }
}