terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "3.43.0"
    }
    azuread = {
      source = "hashicorp/azuread"
      version = "2.34.1"
    }
  }
}

data "azurerm_client_config" "current" {}