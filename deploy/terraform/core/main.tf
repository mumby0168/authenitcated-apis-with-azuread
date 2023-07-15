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

# API =======================================================================================================

resource "azurerm_user_assigned_identity" "api" {
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_resource_group.azure_ad_auth.location
  name                = "uai-${var.project}-api-${var.env_code}-${var.location_code}"
}

resource "azurerm_linux_web_app" "api" {
  name                = "app-${var.project}-api-${var.env_code}-${var.location_code}"
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_service_plan.azure_ad_auth.location
  service_plan_id     = azurerm_service_plan.azure_ad_auth.id

  site_config {
    always_on = true
    application_stack {
      docker_image_name = "billymumby/addemoapi:latest"
    }
  }

  logs {
    application_logs {
      file_system_level = "Verbose"

    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  app_settings = {
    "AZURE_CLIENT_ID"            = azurerm_user_assigned_identity.api.client_id
    "DOCKER_REGISTRY_SERVER_URL" = "https://index.docker.io/v1"
  }
}

# reference already created app reg and service principal

data "azuread_application" "api" {
  display_name = "Azure AD Auth API (${var.env_code})"
}

data "azuread_service_principal" "api" {
  application_id = data.azuread_application.api.application_id
}

resource "azuread_app_role_assignment" "webappaccess_to_api" {
  app_role_id         = data.azuread_application.api.app_role_ids["Api.All"]
  principal_object_id = azurerm_user_assigned_identity.webapp.principal_id
  resource_object_id  = data.azuread_service_principal.api.object_id
}


# webapp =======================================================================================================

resource "azurerm_user_assigned_identity" "webapp" {
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_resource_group.azure_ad_auth.location
  name                = "uai-${var.project}-webapp-${var.env_code}-${var.location_code}"
}

resource "azurerm_linux_web_app" "webapp" {
  name                = "app-${var.project}-webapp-${var.env_code}-${var.location_code}"
  resource_group_name = azurerm_resource_group.azure_ad_auth.name
  location            = azurerm_service_plan.azure_ad_auth.location
  service_plan_id     = azurerm_service_plan.azure_ad_auth.id

  site_config {
    always_on = true
    application_stack {
      docker_registry_url = "https://index.docker.io/v1"
      docker_image_name   = "billymumby/addemowebapp:latest"
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.webapp.id]
  }

  app_settings = {
    "AZURE_CLIENT_ID"            = azurerm_user_assigned_identity.webapp.client_id
    "DOCKER_REGISTRY_SERVER_URL" = "https://index.docker.io/v1"
  }
}
