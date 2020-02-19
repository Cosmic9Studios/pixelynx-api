#!/bin/sh

apk add --update npm
npm install -g pm2
pm2 start /bin/vault -- server -dev
sleep 5;
vault login token 
vault secrets disable secret
vault secrets enable -path=secret -version=1 kv
vault kv put secret/Auth JWTSecret="bfe222e7-b5eb-4d31-91df-c30797324fbc"
vault secrets enable database
vault write database/config/my-postgresql-database \
    plugin_name=postgresql-database-plugin \
    allowed_roles="admin" \
    connection_url="postgresql://{{username}}:{{password}}@192.168.50.50:5432/postgres?sslmode=disable" \
    username="postgres" \
    password="devpass"

vault write database/roles/admin \
    db_name=my-postgresql-database \
    creation_statements="CREATE USER \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
                        GRANT USAGE ON SCHEMA public TO \"{{name}}\"; \
                        GRANT ALL ON ALL TABLES IN SCHEMA public TO \"{{name}}\"; \
                        GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO \"{{name}}\"; \
                        GRANT ALL ON ALL FUNCTIONS IN SCHEMA public TO \"{{name}}\";" \
    default_ttl="1h" \
    max_ttl="24h"

tail -f /dev/null