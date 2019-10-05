FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS dotnet-sdk

COPY . /tempApp
WORKDIR /tempApp/AssetStore.Api
RUN dotnet publish -o ./publish

FROM dotnet-sdk

COPY --from=0 /tempApp/AssetStore.Api/publish /app
RUN apt-get update && apt-get install -y curl && curl -sSL https://sdk.cloud.google.com | bash
ENV PATH="$PATH:/root/google-cloud-sdk/bin"
WORKDIR /app
CMD [ "dotnet", "AssetStore.Api.dll" ]

