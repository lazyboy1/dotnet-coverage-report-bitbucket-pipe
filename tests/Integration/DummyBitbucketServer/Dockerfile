FROM mcr.microsoft.com/dotnet/sdk:3.1 as build

#ENV PRROJECT_NAME "DummyBitbucketServer"

WORKDIR /source

COPY *.csproj .

RUN dotnet restore

COPY . ./

RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:3.1

WORKDIR /app

COPY --from=build /app .

RUN ls

ENTRYPOINT ["dotnet", "/app/DummyBitbucketServer.dll"]
