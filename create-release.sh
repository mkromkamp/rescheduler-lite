# !/bin/bash

dotnet publish -c Release -o ../../bin/ -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true --self-contained true