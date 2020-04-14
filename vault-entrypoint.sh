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
vault write database/config/postgres \
    plugin_name=postgresql-database-plugin \
    allowed_roles="*" \
    connection_url="postgresql://{{username}}:{{password}}@192.168.50.50:5432/postgres?sslmode=disable" \
    username="postgres" \
    password="devpass"

vault write database/roles/admin \
    db_name=postgres \
    creation_statements="
        CREATE ROLE \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
        GRANT \"{{name}}\" to postgres; \
        REASSIGN OWNED BY postgres TO \"{{name}}\";" \
    revocation_statements="
        REASSIGN OWNED BY \"{{name}}\" TO postgres; \
        DROP OWNED BY \"{{name}}\"; \
        DROP ROLE \"{{name}}\";" \
    default_ttl="10s" \
    max_ttl="20s"

vault write database/roles/read \
    db_name=postgres \
    creation_statements="
        CREATE ROLE \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
        GRANT \"{{name}}\" to postgres; \
        GRANT USAGE ON SCHEMA public TO \"{{name}}\"; \
        GRANT SELECT ON ALL TABLES IN SCHEMA public TO \"{{name}}\"; \
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO \"{{name}}\";" \
    revocation_statements="
        REASSIGN OWNED BY \"{{name}}\" TO postgres; \
        DROP OWNED BY \"{{name}}\"; \
        DROP ROLE \"{{name}}\";" \
    default_ttl="10m" \
    max_ttl="20m"

vault write database/roles/write \
    db_name=postgres \
    creation_statements="
        CREATE ROLE \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
        GRANT \"{{name}}\" to postgres; \
        GRANT USAGE ON SCHEMA public TO \"{{name}}\"; \
        GRANT INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO \"{{name}}\"; \
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT INSERT, UPDATE, DELETE ON TABLES TO \"{{name}}\";" \
    revocation_statements="
        REASSIGN OWNED BY \"{{name}}\" TO postgres; \
        DROP OWNED BY \"{{name}}\"; \
        DROP ROLE \"{{name}}\";" \
    default_ttl="10m" \
    max_ttl="20m"

vault write database/roles/read_write \
    db_name=postgres \
    creation_statements="
        CREATE ROLE \"{{name}}\" WITH LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; \
        GRANT \"{{name}}\" to postgres; \
        GRANT USAGE ON SCHEMA public TO \"{{name}}\"; \
        GRANT ALL ON ALL TABLES IN SCHEMA public TO \"{{name}}\"; \
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO \"{{name}}\";" \
    revocation_statements="
        REASSIGN OWNED BY \"{{name}}\" TO postgres; \
        DROP OWNED BY \"{{name}}\"; \
        DROP ROLE \"{{name}}\";" \
    default_ttl="10m" \
    max_ttl="20m"

tail -f /dev/null