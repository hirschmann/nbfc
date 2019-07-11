#!/bin/bash

NUGET_URL="https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

function download-nuget {
    curl -O $NUGET_URL
	
    # import Mozilla trusted root certificates into mono certificates store
    
    # Debian / Ubuntu / Arch
    cert-sync "/etc/ssl/certs/ca-certificates.crt"

    # Fedora / CentOS
    cert-sync "/etc/pki/tls/certs/ca-bundle.crt"
}


pushd $(dirname "${0}")

# download nuget if necessary
if [ ! -f ./nuget.exe ]
then
    echo "NuGet could not be found. Downloading latest recommended version."
    download-nuget
fi

mono nuget.exe restore

# restore nuget packages for solution
if [ "$?" != 0 ]
then
    echo "Packages could not be restored. Updating NuGet."
    rm ./nuget.exe
    download-nuget
    mono nuget.exe restore
fi    

# build solution
xbuild /t:Clean /p:Configuration=ReleaseLinux NoteBookFanControl.sln
xbuild /t:Build /p:Configuration=ReleaseLinux NoteBookFanControl.sln

popd 
