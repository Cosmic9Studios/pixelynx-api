Set-Location $PSScriptRoot/../AssetStore.Api
$keyName = (gcloud kms keys list --location global --keyring assetstore --format json | ConvertFrom-Json).name | Where-Object {$_ -like "*/appsecrets" }
$keyName | Out-File appsecrets.json.keyname