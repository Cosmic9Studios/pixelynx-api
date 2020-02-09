terraform {
    backend "gcs" {
        bucket = "staging-pixelynx-state"
        prefix = "gcp/k8s/pixelynx-api"
    }
}