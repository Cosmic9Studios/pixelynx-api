remote_state {
  backend = "gcs"
  config = {
    bucket         = "c9s-state"
    prefix         = "prod/nomad-assetstore"
  } 
}


terraform {
  source = "git::https://github.com/Cosmic9Studios/terraform-nomad-job.git?ref=v1.0.0"
}

inputs = {
  address = "https://nomad.pixelynx.com"
  file_path = "${get_terragrunt_dir()}/../files/assetstore.hcl"
}