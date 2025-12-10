$workingDir = split-path $MyInvocation.MyCommand.Path
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$nugeturl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

function download-nuget {
	$client = New-Object System.Net.WebClient
	
	# Set proxy to null to prevent the web client from trying to retrieve
	# proxy settings from IE (which is incredibly slow)
	$client.Proxy = $null
	$client.DownloadFile($nugeturl, "$workingDir\nuget.exe")
}

push-location $workingDir

# download nuget if necessary
if(!(test-path .\nuget.exe)) {
	write-output "NuGet could not be found. Downloading latest recommended version."
	download-nuget
}

# restore nuget packages for solution
& .\nuget.exe restore

# re-download nuget and retry on error
if($LASTEXITCODE -ne 0) {	
	write-output "Packages could not be restored. Updating NuGet."
	remove-item .\nuget.exe
	download-nuget
	& .\nuget.exe restore
}

# get msbuild path
$path = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
$msbuild = join-path $path 'MSBuild\Current\Bin\MSBuild.exe'

# build solution
& $msbuild /t:Clean,Build /p:Configuration=ReleaseWindows NoteBookFanControl.sln

pop-location
