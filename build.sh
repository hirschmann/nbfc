#!/bin/bash

xbuild /t:Build /p:Configuration=ReleaseLinux "$(dirname -- "$0")/NoteBookFanControl.sln"