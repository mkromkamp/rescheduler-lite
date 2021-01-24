# !/bin/bash

dotnet publish -c Release --project ./src/Rescheduler.Api -o ../../bin/ -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true --self-contained true