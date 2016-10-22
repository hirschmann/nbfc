#!/bin/bash

kill -SIGUSR1 $(cat /run/nbfc.pid)
