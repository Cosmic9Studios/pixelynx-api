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
  file_path = "${get_terragrunt_dir()}/../files/api.hcl"
  vars = {
      docker_user = "phenry20"
      docker_pass = "4c9c1ba12ce6b2fbedfa9c0848e8c5b0ac8e277f"
      version = "latest"
  }
}