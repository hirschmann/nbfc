$workingDir = split-path $MyInvocation.MyCommand.Path
push-location $workingDir

# download nuget if necessary
if(!(test-path .\nuget.exe)) {
	(new-object System.Net.WebClient).DownloadFile('http://nuget.org/nuget.exe', "$workingDir\nuget.exe")
}

# update nuget
.\nuget.exe update -self

# restore nuget packages for solution
.\nuget.exe restore

# build solution
& ${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe /t:Build /p:Configuration=ReleaseWindows NoteBookFanControl.sln

pop-location
