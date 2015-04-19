#!/bin/bash

mono-service -m:NbfcService "$(dirname -- "$0")/NbfcService.exe"
