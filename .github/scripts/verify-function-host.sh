#!/usr/bin/env bash
set -euo pipefail

: "${FUNCTION_APP_NAME:?FUNCTION_APP_NAME is required}"
: "${FUNCTION_APP_RESOURCE_GROUP:?FUNCTION_APP_RESOURCE_GROUP is required}"

hostname=$(az functionapp show \
  --name "$FUNCTION_APP_NAME" --resource-group "$FUNCTION_APP_RESOURCE_GROUP" \
  --query defaultHostName --output tsv --only-show-errors)
if [[ ! "$hostname" =~ ^([a-zA-Z0-9-]+\.)+azurewebsites\.net$ ]]; then
  echo "::error::Invalid Function App hostname."
  exit 1
fi

deadline=$((SECONDS + 300))
while true; do
  remaining=$((deadline - SECONDS))
  if (( remaining <= 0 )); then
    echo "::error::Function App did not become healthy within five minutes."
    exit 1
  fi

  status=$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
    --max-time "$((remaining < 20 ? remaining : 20))" \
    "https://${hostname}/api/v1/health") || status=000
  echo "Function health endpoint returned HTTP ${status}."
  if [[ "$status" == 200 ]]; then
    break
  fi

  remaining=$((deadline - SECONDS))
  if (( remaining > 0 )); then
    sleep "$((remaining < 10 ? remaining : 10))"
  fi
done
