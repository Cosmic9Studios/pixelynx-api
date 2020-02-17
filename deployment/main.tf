terraform {
    backend "gcs" {
        prefix = "gcp/k8s/pixelynx-api"
    }
}

data "terraform_remote_state" "k8s" {
  backend = "gcs"
  config = {
    bucket = var.app_environment == "Staging" ? "staging-pixelynx-state" : "pixelynx-state"
    prefix = "gcp/app"
  }
}

data "google_client_config" "default" {}

provider "kubectl" {
    load_config_file       = false
    host                   = "https://${data.terraform_remote_state.k8s.outputs.endpoint}"
    token                  = data.google_client_config.default.access_token
    cluster_ca_certificate = data.terraform_remote_state.k8s.outputs.cluster_ca_certificate
}

provider "kubernetes" {
    load_config_file       = false
    host                   = "https://${data.terraform_remote_state.k8s.outputs.endpoint}"
    token                  = data.google_client_config.default.access_token
    cluster_ca_certificate = data.terraform_remote_state.k8s.outputs.cluster_ca_certificate
}

provider "random" {}

resource "random_pet" "secret" {}

resource "kubernetes_secret" "secret" {
  metadata {
    name = random_pet.secret.id
  }

  data = {
    ".dockerconfigjson" = jsonencode({
      "auths" : {
        "https://index.docker.io/v1/" : {
          email    = "phenry@cosmic9studios.com"
          username = "phenry20"
          password = var.docker_pass
          auth     = base64encode(join(":", ["phenry20", var.docker_pass]))
        }
      }
    })
  }

  type = "kubernetes.io/dockerconfigjson"
}

resource "kubectl_manifest" "deployment" {
  yaml_body = templatefile("${path.module}/manifests/deployment.yaml", {
    environment = var.app_environment 
    version = var.app_version
    secret = random_pet.secret.id
  })

  depends_on = [kubernetes_secret.secret]
}
