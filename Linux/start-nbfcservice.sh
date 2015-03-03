#!/bin/bash

mono-service -m:NbfcService -l:/root/nbfcservice-lock "$(dirname -- "$0")/NbfcService.exe"
