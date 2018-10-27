#!/bin/bash

LOCK_FILE="/run/nbfc.pid"

case $1 in
    "start")
            DIR=$(dirname -- $0)
            mono-service -l:$LOCK_FILE -m:NbfcService "$DIR/NbfcService.exe"
            ;;
    "stop")
            kill -SIGTERM $(cat $LOCK_FILE)
            ;;
    "pause")
            kill -SIGUSR1 $(cat $LOCK_FILE)
            ;;
    "continue")
            kill -SIGUSR2 $(cat $LOCK_FILE)
            ;;
    *)
            echo "$0 (start|stop|pause|continue)"
            exit 1
esac
