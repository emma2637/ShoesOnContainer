﻿From mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /code

#copy files to localhost to current docker location
COPY . .
#restore packages from nuget
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /out 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY --from=build /out /app/
WORKDIR /app
EXPOSE 80/tcp
ENTRYPOINT	["dotnet","WebMvc.dll"]
