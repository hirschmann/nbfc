#!/bin/bash

pushd $(dirname "${0}")

# download nuget if necessary
if [ -f ./nuget.exe ]
then
	wget http://nuget.org/nuget.exe
fi

# import Mozilla trusted root certificates into mono certificates store
mozroots --import --sync

# restore nuget packages for solution
mono nuget.exe restore

# build solution
xbuild /t:Build /p:Configuration=ReleaseLinux NoteBookFanControl.sln

popd 