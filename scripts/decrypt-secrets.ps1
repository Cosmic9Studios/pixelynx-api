Set-Location $PSScriptRoot/../AssetStore.Api
gcloud kms decrypt --plaintext-file appsecrets.json --ciphertext-file appsecrets.json.encrypted --key (Get-Content appsecrets.json.keyname)