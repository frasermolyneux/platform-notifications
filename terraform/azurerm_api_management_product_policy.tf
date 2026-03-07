resource "azurerm_api_management_product_policy" "api_product_policy" {
  product_id = azurerm_api_management_product.api_product.product_id

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <choose>
          <when condition="@(System.Text.RegularExpressions.Regex.IsMatch(context.Request.Url.Path, @"/v\d+(\.\d+)?/(health|info)$") || context.Request.Url.Path.Contains("/openapi"))">
              <!-- Allow anonymous access to health, info and openapi endpoints -->
          </when>
          <otherwise>
              <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
                  <openid-config url="https://login.microsoftonline.com/${data.azuread_client_config.current.tenant_id}/v2.0/.well-known/openid-configuration" />
                  <audiences>
                  <audience>${local.entra_api_identifier_uri}</audience>
                  </audiences>
                  <issuers>
                      <issuer>https://sts.windows.net/${data.azuread_client_config.current.tenant_id}/</issuer>
                  </issuers>
                  <required-claims>
                      <claim name="roles" match="any">
                        ${join("\n                        ", [for role in local.app_roles : "<value>${role.value}</value>"])}
                      </claim>
                  </required-claims>
              </validate-jwt>
          </otherwise>
      </choose>
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
      <cache-store duration="3600" />
  </outbound>
  <on-error />
</policies>
XML
}
