using System;
using System.Windows.Forms;

namespace ColorClockSaver {
	static class Program {
		[STAThread]
		static void Main(string[] args) {
			bool show = true;

			if (args.Length > 0) {
				var flag = args[0].ToLower().Trim().Substring(0, 2);
				switch (flag) {
				case "/p":
					// Preview
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					var previousWindowHandle = args[1];
					Application.Run(new MainForm(new IntPtr(long.Parse(previousWindowHandle))));
					show = false;
					break;
				case "/c":
					// Configure
					// inform the user no options can be set in this screen saver
					MessageBox.Show("This screensaver has no options that you can set",
						"Blue Screen Saver",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					show = false;
					break;
				}
			}

			if (show) {
				// Show the screensaver.
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				ShowScreensaver();
				Application.Run();
			}
		}

		// will show the screen saver
		static void ShowScreensaver() {
			// loops through all the computer's screens (monitors)
			foreach (var screen in Screen.AllScreens) {
				// creates a form just for that screen and passes it the bounds of that screen
				var screensaver = new MainForm(screen.Bounds);
				screensaver.Show();
			}
		}
	}
}
