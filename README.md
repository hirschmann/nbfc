NoteBook FanControl
===================

NBFC is a cross-platform fan control service for notebooks.
It comes with a powerful configuration system, which allows to adjust it to many different notebook models.

## How to build

First you have to clone the latest revision of this repo:<br/>
`git clone --depth 1 https://github.com/hirschmann/nbfc.git`


#### Build on Windows
Make sure these packages are installed on your machine:
* [.NET Framework 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
* [NuGet](https://docs.nuget.org/consume/command-line-reference)
* [WiX Toolset v3.9](https://wix.codeplex.com/releases/view/610859)

You should also add %windir%\Microsoft.NET\Framework and the folder which includes nuget.exe to your PATH.

To build the solution, navigate to the cloned repo:<br/>
`cd nbfc`<br/>
Restore the NuGet packages for the solution:<br/>
`nuget restore`<br/>
And finally build it:<br/>
`msbuild /t:Build /p:Configuration=ReleaseWindows NoteBookFanControl.sln`

If the build was successful there should be a setup file (NbfcBootstrapper.exe) at `nbfc\Windows\Setup\NbfcBootstrapper\bin\Release\`.
Just start it and follow the instructions :)

#### Build on Linux
_Linux support is still experimental!_

If mono is not available on you machine, install the complete package:<br/>
`sudo apt-get install mono-complete`

To build the solution, first navigate to the cloned repo:<br/>
`cd nbfc`<br/>
Then build the solution:<br/>
`xbuild /t:Build /p:Configuration=ReleaseLinux NoteBookFanControl.sln`

The result can be found at `nbfc/Linux/bin/ReleaseLinux`<br/>
If everything worked well, you may want to start the service:<br/>
`sudo start-nbfcservice.sh`<br/>
You can control it via nbfc.exe, e.g. `mono nbfc.exe load 'Name of the config'` to load a config and start the automatic fan control. To learn more about nbfc.exe call `mono nbfc.exe help`.
Finally, to stop the serivce use `sudo stop-nbfcservice.sh`.

Have fun :)

