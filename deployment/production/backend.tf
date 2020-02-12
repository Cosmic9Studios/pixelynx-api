terraform {
    backend "gcs" {
        bucket = "pixelynx-state"
        prefix = "gcp/k8s/pixelynx-web"
    }
}