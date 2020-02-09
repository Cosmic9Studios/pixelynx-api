data "terraform_remote_state" "k8s" {
  backend = "gcs"
  config = {
    bucket = "staging-pixelynx-state"
    prefix = "gcp/k8s-cluster"
  }
}

resource "local_file" "kubeconfig" {
    content = data.terraform_remote_state.k8s.outputs.kubeconfig
    filename = "${path.root}/k8s/kubeconfig"
}

resource "local_file" "ca_cert" {
    sensitive_content = data.terraform_remote_state.k8s.outputs.cluster_certificate
    filename = "${path.root}/k8s/ssl/ca.crt"
}

resource "local_file" "client_cert" {
    sensitive_content = data.terraform_remote_state.k8s.outputs.client_certificate
    filename = "${path.root}/k8s/ssl/client.crt"
}

resource "local_file" "client_key" {
    sensitive_content = data.terraform_remote_state.k8s.outputs.client_key
    filename = "${path.root}/k8s/ssl/client.key"
}

# Deployment
data "template_file" "deployment" {
  template = file("${path.module}/templates/deployment.tpl.yaml")
  vars = {
    version = var.app_version
    environment = var.app_environment
  }
}

resource "local_file" "deployment" {
    content = data.template_file.deployment.rendered
    filename = "${path.root}/k8s/deployment.yaml"
}
