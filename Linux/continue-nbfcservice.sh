#!/bin/bash

kill -SIGUSR2 $(cat /run/nbfc.pid) 
