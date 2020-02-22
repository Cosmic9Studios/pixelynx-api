terraform {
    backend "gcs" {
        prefix = "gcp/k8s/pixelynx-api"
    }
}

locals {
    domain = var.app_environment ==  "Staging" ? "staging.pixelynx.com" : "pixelynx.com"
    gsa_name = "pxl-api"
    ksa_name = "pxl-api"
    roles = [
        "roles/iam.serviceAccountCreator",
        "roles/iam.serviceAccountKeyAdmin",
        "roles/container.admin",
        "roles/storage.admin",
        "roles/cloudsql.editor",
        "roles/compute.viewer",
        "roles/cloudkms.cryptoKeyEncrypterDecrypter"
    ]
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

resource "google_service_account" "gsa" {
  account_id   = local.gsa_name
  display_name = "A service account for Pixelynx-Api pod"
}

resource "kubernetes_service_account" "ksa" {
  metadata {
    name = local.ksa_name
    annotations = {
      "iam.gke.io/gcp-service-account" = google_service_account.gsa.email
    }
  }
}

resource "google_project_iam_member" "roles" {
  for_each = toset(local.roles)
  project = var.project
  role    = each.key
  member  = "serviceAccount:${google_service_account.gsa.email}"
}

resource "google_service_account_iam_binding" "sa" {
  service_account_id = google_service_account.gsa.name
  role    = "roles/iam.workloadIdentityUser"

  members = [
    "serviceAccount:${var.project}.svc.id.goog[default/${local.ksa_name}]"
  ]
}

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
    project = var.project
    instance = data.terraform_remote_state.k8s.outputs.db_instance_name
    domain = local.domain
    serviceAccountName = local.ksa_name
  })

  depends_on = [
    kubernetes_secret.secret,
    google_project_iam_member.roles
  ]
}
