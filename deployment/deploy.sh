#!/bin/bash
cat > ./terraform.tfvars <<- EOM
app_version = "${GITHUB_REF:10}"
app_environment = "$1"
docker_pass = $DOCKER_TOKEN
EOM

./install_kubectl.sh

terraform init -input=false -backend-config=${1,,}/backend.tfvars # To Lowercase
terraform apply -input=false -var-file ./terraform.tfvars -auto-approve