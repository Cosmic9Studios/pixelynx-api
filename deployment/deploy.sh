cat > ./terraform.tfvars <<- EOM
app_version = "${GITHUB_REF:10}"
app_environment = "$1"
EOM

echo $DOCKER_TOKEN | docker login --username "phenry20" --password-stdin
terraform init -input=false
terraform apply -input=false -var-file ./terraform.tfvars -auto-approve
kubectl create secret generic regcred --from-file=.dockerconfigjson=~/.docker/config.json --type=kubernetes.io/dockerconfigjson
kubectl apply -f k8s/deployment.yaml