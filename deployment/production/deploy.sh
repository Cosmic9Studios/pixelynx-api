DIRECTORY=$(cd `dirname $0` && pwd)
cd $DIRECTORY

gpg --quiet --batch --yes --decrypt --passphrase="$1" --output $GOOGLE_APPLICATION_CREDENTIALS ./pixelynx.json.gpg

cat > ./terraform.tfvars <<- EOM
vars = {
    version = "${GITHUB_REF:10}"
    docker_user = "phenry20"
    docker_pass = "$DOCKER_TOKEN"
    environment = "Production"
}
EOM

terragrunt apply -var-file ./terraform.tfvars -auto-approve