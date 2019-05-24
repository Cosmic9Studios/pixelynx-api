FROM mcr.microsoft.com/dotnet/core/sdk:2.2
COPY . /tempApp
WORKDIR /tempApp/AssetStore.Api
RUN dotnet build
RUN ls
RUN mv bin/Debug/netcoreapp2.2 /app
WORKDIR /app
CMD [ "dotnet", "AssetStore.Api.dll" ]
