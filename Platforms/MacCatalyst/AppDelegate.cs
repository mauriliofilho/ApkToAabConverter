﻿using Foundation;
using UIKit;

namespace ApkToAabConverter;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Configurações ao iniciar o app
        var result = base.FinishedLaunching(application, launchOptions);
        return result;
    }
}
