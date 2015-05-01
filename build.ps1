$workingDir = split-path $MyInvocation.MyCommand.Path
push-location $workingDir

if(!(test-path nuget.exe)) {
	(new-object System.Net.WebClient).DownloadFile('http://nuget.org/nuget.exe', "$workingDir\nuget.exe")
}

nuget.exe restore
& $env:windir\Microsoft.NET\Framework\v4.0.*\msbuild.exe /t:Build /p:Configuration=ReleaseWindows NoteBookFanControl.sln

pop-location
