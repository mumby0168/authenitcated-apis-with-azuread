# General

variable "location" {
  type    = string
  default = "westeurope"
}

variable "env_code" {
  type    = string
  default = "dev"

  validation {
    condition     = contains(["dev", "preprod", "prod"], var.env_code)
    error_message = "Value must be dev, preprod or prod"
  }
}

variable "location_code" {
  type    = string
  default = "euw"
}

variable "project" {
  type    = string
  default = "azureadapiauth"
}
