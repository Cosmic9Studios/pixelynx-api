variable "app_version" {
    type = string
}

variable "app_environment" {
    type = string
}

variable "docker_pass" {
    type = string
}

variable "project" {
    default = "pixelynx-staging"
}

variable "db_name" {
    default = "pxl-db"
}