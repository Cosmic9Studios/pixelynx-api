DIRECTORY=$(cd `dirname $0` && pwd)
cd $DIRECTORY/prod

gpg --quiet --batch --yes --decrypt --passphrase="$1" --output $GOOGLE_APPLICATION_CREDENTIALS ../account.json.gpg

cat > ./terraform.tfvars <<- EOM
vars = {
    version = "${GITHUB_REF:10}"
    docker_user = "phenry20"
    docker_pass = "$CI_TOKEN"
}
EOM

terragrunt apply -var-file ./terraform.tfvars -auto-approve