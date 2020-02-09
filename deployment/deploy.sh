cat > ./terraform.tfvars <<- EOM
vars = {
    app_version = "${GITHUB_REF:10}"
    app_environment = "$2"
}
EOM

echo $DOCKER_TOKEN | docker login --username "phenry20" --password-stdin
gpg --quiet --batch --yes --decrypt --passphrase="$1" --output $GOOGLE_APPLICATION_CREDENTIALS ./pixelynx.json.gpg
terraform apply -var-file ./terraform.tfvars -auto-approve
export KUBECONFIG=k8s/kubeconfig
kubectl apply -f k8s/deployment.yaml