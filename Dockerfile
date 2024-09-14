### CONTEXT AT PROJECTS FOLDER ###
# Build the dotnet api
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine-amd64 AS base
WORKDIR /app
ENV IS_DOCKER=TRUE
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk update  \
	&& apk add icu-dev



# DOTNET API
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
ARG TARGETARCH
WORKDIR /app/dotnet
COPY . .
RUN dotnet restore
RUN dotnet build ./KillBot.csproj -c release -o /app/dotnet/build --property WarningLevel=0 -a $TARGETARCH

FROM builder AS publish
RUN dotnet publish . -c release -o /app/dotnet/publish --property WarningLevel=0 -a $TARGETARCH

FROM base AS final
WORKDIR /app/dotnet
COPY --from=publish /app/dotnet/publish .
ENTRYPOINT ["dotnet", "KillBot.dll"]

