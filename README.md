![NoteBook FanControl](https://github.com/hirschmann/nbfc/wiki/images/banner.png)

NBFC is a cross-platform fan control service for notebooks.
It comes with a powerful configuration system, which allows to adjust it to many different notebook models.

## How to build

First you have to clone the latest revision of this repo:  
`git clone --depth 1 https://github.com/hirschmann/nbfc.git`


#### Build on Windows
Make sure these packages are installed on your machine:
- at least: [.NET Framework 4.5](https://www.microsoft.com/en-US/download/details.aspx?id=42643)
- at least: [WiX Toolset v3.9](http://wixtoolset.org/releases/)

To build the solution, run the `build.ps1` script (which is included in the cloned repo) via Windows Powershell.

If the build was successful there should be a setup file (NbfcBootstrapper.exe) at `nbfc\Windows\Setup\NbfcBootstrapper\bin\Release\`.  
Just start it and follow the instructions :)

#### Build on Linux
_Linux support is still experimental!_

If mono is not available on your machine, install the complete package:  
`sudo apt-get install mono-complete`

To build the solution, run the `build.sh` script (which is included in the cloned repo).
The result can be found at `nbfc/Linux/bin/ReleaseLinux`

If everything worked well, you may want to start the service: `sudo start-nbfcservice.sh`
You can control it via nbfc.exe, e.g. `mono nbfc.exe config --apply 'Name of the config'` to load a config, `mono nbfc.exe start` to start the service and `mono nbfc.exe set --auto` to start automatic fan control.  
To learn more about nbfc.exe use `mono nbfc.exe --help`.  
Finally, to stop the serivce use `sudo stop-nbfcservice.sh`.

Have fun :)

