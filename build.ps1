$workingDir = split-path $MyInvocation.MyCommand.Path
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
push-location $workingDir

# download nuget if necessary
if(!(test-path .\nuget.exe)) {
	(new-object System.Net.WebClient).DownloadFile('http://nuget.org/nuget.exe', "$workingDir\nuget.exe")
}

# update nuget
.\nuget.exe update -self

# restore nuget packages for solution
.\nuget.exe restore

# get msbuild path
$path = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
$msbuild = join-path $path 'MSBuild\15.0\Bin\MSBuild.exe'

# build solution
& $msbuild /t:Build /p:Configuration=ReleaseWindows NoteBookFanControl.sln

pop-location
