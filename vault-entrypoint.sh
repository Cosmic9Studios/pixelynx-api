#!/bin/sh

apk add --update npm
npm install -g pm2
pm2 start /bin/vault -- server -dev
vault login token 
vault secrets disable secret
vault secrets enable -path=secret -version=1 kv
vault kv put secret/ConnectionStrings Pixelynx="Server=127.0.0.1;Port=5432;Database=postgres;User Id=postgres;Password=devpass;"
vault kv put secret/Auth JWTSecret="bfe222e7-b5eb-4d31-91df-c30797324fbc"
tail -f /dev/null