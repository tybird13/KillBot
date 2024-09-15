### CONTEXT AT PROJECTS FOLDER ###
# Build the dotnet api
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app
ENV IS_DOCKER=TRUE
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ARG DISCORD_KILL_BOT_TOKEN
ENV DISCORD_KILL_BOT_TOKEN=$DISCORD_KILL_BOT_TOKEN
RUN apk update  \
	&& apk add icu-dev \
	&& apk add sqlite
# DOTNET API
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /app/dotnet
COPY . .
RUN dotnet restore
RUN dotnet build ./KillBot.csproj -c release -o /app/dotnet/build --property WarningLevel=0

FROM builder AS publish
RUN dotnet publish . -c release -o /app/dotnet/publish --property WarningLevel=0

FROM base AS final
WORKDIR /app/dotnet
COPY --from=publish /app/dotnet/publish .
ENTRYPOINT ["dotnet", "KillBot.dll"]

