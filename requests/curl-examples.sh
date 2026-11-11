#!/usr/bin/env bash
# Ejemplos cURL de la prueba tecnica CLTTestApi.
# Uso:
#   bash requests/curl-examples.sh
#   BASE_URL=http://localhost:5036 API_KEY=clt-demo-api-key-2026 bash requests/curl-examples.sh
#
# Requiere la API corriendo (`dotnet run`). Funciona en Git Bash, WSL, Linux y macOS.

set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5036}"
API_KEY="${API_KEY:-clt-demo-api-key-2026}"

EMAIL="juan.$(date +%s)@test.com"
CURRENCY_CODE="T$(date +%H%M%S)"
KEY_HEADER="X-API-KEY: ${API_KEY}"
JSON_HEADER="Content-Type: application/json"

# Extrae el primer "id" numerico de una respuesta JSON compacta.
extract_id() {
  grep -o '"id":[0-9]*' | head -n 1 | cut -d: -f2
}

echo "============================================================"
echo "CLTTestApi - ejemplos cURL"
echo "BASE_URL=$BASE_URL"
echo "============================================================"

echo
echo "1) GET /currencies sin API Key (espera 401)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" "$BASE_URL/currencies"

echo
echo "2) GET /currencies con API Key incorrecta (espera 401)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -H "X-API-KEY: llave-incorrecta" \
  "$BASE_URL/currencies"

echo
echo "3) POST /users"
USER_JSON=$(curl -sS -X POST "$BASE_URL/users" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d "{\"name\":\"Juan\",\"email\":\"$EMAIL\",\"password\":\"Secreto123\"}")
echo "$USER_JSON"
USER_ID=$(printf '%s' "$USER_JSON" | extract_id)
echo "userId=$USER_ID"

echo
echo "4) GET /users"
curl -sS -H "$KEY_HEADER" "$BASE_URL/users"; echo

echo
echo "5) GET /users?isActive=true"
curl -sS -H "$KEY_HEADER" "$BASE_URL/users?isActive=true"; echo

echo
echo "6) GET /users?is_active=false (alias)"
curl -sS -H "$KEY_HEADER" "$BASE_URL/users?is_active=false"; echo

echo
echo "7) GET /users/$USER_ID"
curl -sS -H "$KEY_HEADER" "$BASE_URL/users/$USER_ID"; echo

echo
echo "8) PUT /users/$USER_ID"
curl -sS -X PUT "$BASE_URL/users/$USER_ID" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d "{\"name\":\"Juan P\u00e9rez\",\"email\":\"$EMAIL\",\"isActive\":true}"; echo

echo
echo "9) POST /users/$USER_ID/addresses"
ADDRESS_JSON=$(curl -sS -X POST "$BASE_URL/users/$USER_ID/addresses" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d '{"street":"Calle Falsa 123","city":"Asunci\u00f3n","country":"Paraguay","zipCode":"9999"}')
echo "$ADDRESS_JSON"
ADDRESS_ID=$(printf '%s' "$ADDRESS_JSON" | extract_id)
echo "addressId=$ADDRESS_ID"

echo
echo "10) GET /users/$USER_ID/addresses"
curl -sS -H "$KEY_HEADER" "$BASE_URL/users/$USER_ID/addresses"; echo

echo
echo "11) PUT /addresses/$ADDRESS_ID"
curl -sS -X PUT "$BASE_URL/addresses/$ADDRESS_ID" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d '{"street":"Av. Mcal. L\u00f3pez 456","city":"Asunci\u00f3n","country":"Paraguay","zipCode":"1234"}'; echo

echo
echo "12) POST /users/999999/addresses (espera 404)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -X POST "$BASE_URL/users/999999/addresses" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d '{"street":"Calle 1","city":"Asunci\u00f3n","country":"Paraguay"}'

echo
echo "13) GET /currencies"
curl -sS -H "$KEY_HEADER" "$BASE_URL/currencies"; echo

echo
echo "14) POST /currencies ($CURRENCY_CODE)"
curl -sS -X POST "$BASE_URL/currencies" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d "{\"code\":\"$CURRENCY_CODE\",\"name\":\"Moneda de prueba\",\"rateToBase\":7900}"; echo

echo
echo "15) POST /currencies con rateToBase 0 (espera 400)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -X POST "$BASE_URL/currencies" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d '{"code":"BAD","name":"Inv\u00e1lida","rateToBase":0}'

echo
echo "16) POST /currency/convert USD -> PYG (100)"
curl -sS -X POST "$BASE_URL/currency/convert" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d '{"fromCurrencyCode":"USD","toCurrencyCode":"PYG","amount":100}'; echo

echo
echo "17) POST /currency/convert moneda inexistente (espera 404)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -X POST "$BASE_URL/currency/convert" \
  -H "$KEY_HEADER" \
  -H "$JSON_HEADER" \
  -d '{"fromCurrencyCode":"XXX","toCurrencyCode":"PYG","amount":10}'

echo
echo "18) DELETE /addresses/$ADDRESS_ID"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -X DELETE "$BASE_URL/addresses/$ADDRESS_ID" \
  -H "$KEY_HEADER"

echo
echo "19) DELETE /users/$USER_ID (borra direcciones en cascada)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -X DELETE "$BASE_URL/users/$USER_ID" \
  -H "$KEY_HEADER"

echo
echo "20) GET /users/$USER_ID (espera 404)"
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  "$BASE_URL/users/$USER_ID" \
  -H "$KEY_HEADER"

echo
echo "Flujo completado."
