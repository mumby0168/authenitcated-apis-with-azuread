resource "random_uuid" "api_scope" {}
resource "random_uuid" "api_all_role_id" {}

resource "azuread_application" "api" {
  display_name    = "Azure AD Auth API (${var.env_code})"
  identifier_uris = [lower("api://azure-ad-auth-api-${var.env_code}.net")]

  api {
    oauth2_permission_scope {
      id                         = random_uuid.api_scope.result
      admin_consent_description  = "Allows access to the api"
      admin_consent_display_name = "Acceess the api"
      enabled                    = true
      type                       = "User"
      user_consent_description   = "Allows access to the api"
      user_consent_display_name  = "Acceess the api"
      value                      = "api_access"
    }
  }

  # This role is used to just access the API in general.
  app_role {
    allowed_member_types = ["Application"]
    description          = "Services can access the API"
    display_name         = "API Access"
    enabled              = true
    id                   = random_uuid.api_all_role_id.result
    value                = "Api.All"
  }
}

resource "azuread_service_principal" "api" {
  application_id               = azuread_application.api.application_id
  app_role_assignment_required = true
}

# web app 

resource "random_uuid" "webappreader_role_id" {}
resource "random_uuid" "webappcontributor_role_id" {}
resource "random_uuid" "webappowner_role_id" {}

resource "azuread_application" "webapp" {
  display_name            = "Azure AD Auth Web App (${var.env_code})"
  group_membership_claims = ["ApplicationGroup"]

  web {
    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
    redirect_uris = [
      lower("https://localhost:5555/signin-oidc"),
      lower("https://localhost:5045/signin-azuread"),
      lower("https://app-${var.project}-webapp-${var.env_code}-${var.location_code}.azurewebsites.net/signin-azuread"),
      lower("https://app-${var.project}-webapp-${var.env_code}-${var.location_code}/signin-azuread"),
    ]
  }

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" #microsoft graph id
    resource_access {
      id   = "5f8c59db-677d-491f-a6b8-5f174b11ec1d" # Read all AD groups  
      type = "Scope"
    }
    resource_access {
      id   = "06da0dbc-49e2-44d2-8312-53f166ab848a" # Read AD directory data
      type = "Scope"
    }
    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # User.Read delegate permissions id
      type = "Scope"
    }
  }

  optional_claims {
    access_token {
      additional_properties = ["dns_domain_and_sam_account_name", ]
      essential             = false
      name                  = "groups"
    }
    id_token {
      additional_properties = []
      essential             = false
      name                  = "groups"
    }
  }

  # User Roles

  app_role {
    allowed_member_types = ["User"]
    description          = "Allows the user to read data on the packing dashboard."
    display_name         = "Reader"
    id                   = random_uuid.webappreader_role_id.result
    value                = "Reader"
  }

  app_role {
    allowed_member_types = ["User"]
    description          = "Allows the user to contribute to certain parts of the packing dashboard."
    display_name         = "Contributor"
    id                   = random_uuid.webappcontributor_role_id.result
    value                = "Contributor"
  }

  app_role {
    allowed_member_types = ["User"]
    description          = "Allows the user to control all parts of the packing dashboard."
    display_name         = "Owner"
    id                   = random_uuid.webappowner_role_id.result
    value                = "Owner"
  }
}