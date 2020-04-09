#!/bin/sh

apk add --update npm
npm install -g pm2
pm2 start /bin/vault -- server -dev
sleep 5;
vault login token 
vault secrets disable secret
vault secrets enable -path=secret -version=1 kv
vault kv put secret/Auth JWTSecret="bfe222e7-b5eb-4d31-91df-c30797324fbc" StripeSecretKey="sk_test_7C2P4QKZAOKjdUpEkNSB7C3L"
vault secrets enable database
vault write database/config/my-postgresql-database \
    plugin_name=postgresql-database-plugin \
    allowed_roles="admin" \
    connection_url="postgresql://{{username}}:{{password}}@192.168.50.50:5432/postgres?sslmode=disable" \
    username="postgres" \
    password="devpass"

vault write database/roles/admin \
    db_name=my-postgresql-database \
    creation_statements="
        CREATE ROLE \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
        GRANT \"{{name}}\" to postgres; \
        GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres; \
        ALTER DEFAULT PRIVILEGES FOR ROLE \"{{name}}\" GRANT ALL PRIVILEGES ON TABLES TO PUBLIC;" \
    revocation_statements="
        REASSIGN OWNED BY \"{{name}}\" TO postgres; \
        DROP OWNED BY \"{{name}}\"; \
        DROP ROLE \"{{name}}\";" \
    default_ttl="1h" \
    max_ttl="24h"

tail -f /dev/null