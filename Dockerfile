FROM mcr.microsoft.com/dotnet/core/sdk:3.0
COPY . /tempApp
WORKDIR /tempApp/AssetStore.Api
RUN dotnet build
RUN ls
RUN mv bin/Debug/netcoreapp3.0 /app
WORKDIR /app
CMD [ "dotnet", "AssetStore.Api.dll" ]
