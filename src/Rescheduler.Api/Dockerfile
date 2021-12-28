# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY ./*.sln .
COPY ./src/. ./src/
COPY ./test/. ./test/
RUN dotnet restore -r linux-musl-x64

WORKDIR /source/src/Rescheduler.Api
RUN dotnet publish -c release -o /app -r linux-musl-x64 --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["./Rescheduler.Api"]