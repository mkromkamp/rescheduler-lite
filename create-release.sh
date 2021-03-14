# !/bin/bash

dotnet publish ./src/Rescheduler.Api -c Release -o ./bin/linux-x64
dotnet publish ./src/Rescheduler.Api -c Release -o ./bin/linux-arm64 -r linux-arm64
dotnet publish ./src/Rescheduler.Api -c Release -o ./bin/linux-musl-x64 -r linux-musl-x64
dotnet publish ./src/Rescheduler.Api -c Release -o ./bin/win-x64 -r win-x64
