using System;
using System.Windows.Forms;
using Server.Helper;

namespace Server;

internal static class Program
{
	public static Form1 form;

	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		
		// Initialize console logger
		ConsoleLogger.Initialize();
		ConsoleLogger.LogInfo("Liberium RAT Server starting...");
		
		form = new Form1();
		
		// Log when main form is created
		ConsoleLogger.LogSuccess("Main form initialized successfully");
		
		// Handle application exit to cleanup console
		Application.ApplicationExit += (sender, e) => {
			ConsoleLogger.LogInfo("Application shutting down...");
			ConsoleLogger.Cleanup();
		};
		
		Application.Run(form);
	}
}

