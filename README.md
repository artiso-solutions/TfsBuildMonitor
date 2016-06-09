TFS Build Monitor
=======================

This is a simple TFS build monitor which has been developed in C# and WPF.

Currently there are two UI types.

One for an individual use on your display and another one which can be used as a build monitor for the whole team on a big display.

## Small UI
![ScreenShot](https://raw.githubusercontent.com/artiso-solutions/TfsBuildMonitor/master/docs/BuildMonitorSmall.png)

## Big UI
![ScreenShot](https://raw.githubusercontent.com/artiso-solutions/TfsBuildMonitor/master/docs/BuildMonitorBig.png)

## Notifications
If a build succeeds, partially fails, completely fails or we detect that on the running build an error occurred, then a notification is shown.

To provide this feature to Windows 7 users we avoided using features for Windows 8 and 10.
![ScreenShot](https://raw.githubusercontent.com/artiso-solutions/TfsBuildMonitor/master/docs/BuildMonitorNotification.png)

## Settings
![ScreenShot](https://raw.githubusercontent.com/artiso-solutions/TfsBuildMonitor/master/docs/BuildMonitorSettings.png)

## Use Software
To use the build monitor you have to start the setup.exe in the publish directory.

Future updates will be downloaded automatically as soon as a new version is published on this github repository.
