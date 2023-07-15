resource "azurerm_resource_group" "azure_ad_auth" {
    name     = "rg-${var.project}-${var.env_code}-${var.location_code}"
    location = var.location
}

resource "azurerm_service_plan" "azure_ad_auth" {
    name                = "asp-${var.project}-${var.env_code}-${var.location_code}"
    resource_group_name = azurerm_resource_group.azure_ad_auth.name
    location            = azurerm_resource_group.azure_ad_auth.location
    os_type             = "Linux"
    sku_name            = "B1"
}

# BFF =======================================================================================================

resource "azurerm_user_assigned_identity" "bff" {
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_resource_group.azure_ad_auth.location
  name                = "uai-${var.project}-bff-${var.env_code}-${var.location_code}"
}

resource "azurerm_linux_web_app" "bff" {
  name                = "app-${var.project}-bff-${var.env_code}-${var.location_code}"
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_service_plan.azure_ad_auth.location
  service_plan_id     = azurerm_service_plan.azure_ad_auth.id

  site_config {
    application_stack {
      dotnet_version = "7.0"
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.bff.id]
  }

  app_settings = {
    "AZURE_CLIENT_ID"                                    = azurerm_user_assigned_identity.dixons_portal.client_id
  }
}

# API =======================================================================================================

resource "azurerm_user_assigned_identity" "api" {
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_resource_group.azure_ad_auth.location
  name                = "uai-${var.project-api}-${var.env_code}-${var.location_code}"
}

resource "azurerm_linux_web_app" "api" {
  name                = "app-${var.project}-api-${var.env_code}-${var.location_code}"
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_service_plan.azure_ad_auth.location
  service_plan_id     = azurerm_service_plan.azure_ad_auth.id

  site_config {
    application_stack {
      dotnet_version = "7.0"
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  app_settings = {
    "AZURE_CLIENT_ID"                                    = azurerm_user_assigned_identity.dixons_portal.client_id
  }
}