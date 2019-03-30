#!/bin/bash

# build solution
xbuild /t:Clean /p:Configuration=ReleaseLinux NoteBookFanControl.sln
xbuild /t:Build /p:Configuration=ReleaseLinux NoteBookFanControl.sln

popd 
