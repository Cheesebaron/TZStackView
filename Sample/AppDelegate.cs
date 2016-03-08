using Foundation;
using UIKit;

namespace Sample
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // create a new window instance based on the screen size
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

			UISegmentedControl.Appearance.SetTitleTextAttributes (new UITextAttributes () { 
				Font = UIFont.FromName ("HelveticaNeue-Light", 10f)
			}, UIControlState.Normal);

			Window.RootViewController = new ViewController ();
            Window.MakeKeyAndVisible();
            return true;
        }
    }
}
