FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS dotnet-sdk

COPY . /tempApp
WORKDIR /tempApp/Pixelynx.Api
RUN dotnet publish -o ./publish

FROM dotnet-sdk

COPY --from=0 /tempApp/Pixelynx.Api/publish /app
WORKDIR /app
CMD [ "dotnet", "Pixelynx.Api.dll" ]

