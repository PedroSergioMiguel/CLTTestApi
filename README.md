# CLTTestApi

API REST en .NET 10 con Minimal APIs para gestión de usuarios, direcciones y conversión de divisas.

## Stack

- .NET 10 (probado con **10.0.404**)
- Minimal APIs
- Entity Framework Core 10 + SQLite
- FluentValidation
- CQRS por carpetas (Commands / Queries)

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- No requiere SQL Server, Docker ni base de datos externa: usa un archivo SQLite local (`clt_test.db`).
- Postman (opcional, para ejecutar la colección de pruebas).

## Cómo correr el proyecto

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Compilar
dotnet build

# 3. Ejecutar (perfil http, puerto 5036)
dotnet run
```

También se puede indicar la URL explícitamente:

```bash
dotnet run --urls http://localhost:5036
```

Al iniciar:

1. Se crea la base `clt_test.db` si no existe.
2. Se aplican las migraciones de EF Core (`Infrastructure/Migrations`).
3. Se siembran las monedas base **PYG** (`RateToBase = 1`) y **USD** (`RateToBase = 7300`) si la tabla está vacía.

Los mensajes `warn` que aparecen al ejecutar las pruebas negativas (401, 404, 400) son esperados y no indican fallos.

Para reiniciar la base desde cero: detener la API y borrar `clt_test.db`, `clt_test.db-shm` y `clt_test.db-wal`.

## API Key

Todas las rutas requieren el header:

```http
X-API-KEY: clt-demo-api-key-2026
```

La clave se configura en `appsettings.json`:

```json
"ApiKey": "clt-demo-api-key-2026"
```

Si el header falta o no coincide, la API responde **401 Unauthorized** con `application/problem+json`.

Ejemplo con `curl`:

```bash
curl -H "X-API-KEY: clt-demo-api-key-2026" http://localhost:5036/users
```

Ejemplo con PowerShell:

```powershell
$headers = @{ "X-API-KEY" = "clt-demo-api-key-2026" }
Invoke-RestMethod -Uri "http://localhost:5036/currencies" -Headers $headers
```

## Endpoints

### Users

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/users` | Crear usuario |
| `GET` | `/users` | Listar usuarios |
| `GET` | `/users?isActive=true\|false` | Listar con filtro (también acepta `is_active`) |
| `GET` | `/users/{id}` | Obtener usuario por id |
| `PUT` | `/users/{id}` | Actualizar `name`, `email` e `isActive` |
| `DELETE` | `/users/{id}` | Eliminar usuario (sus direcciones se eliminan en cascada) |

Body de creación:

```json
{
  "name": "Juan",
  "email": "juan@test.com",
  "password": "Secreto123",
  "isActive": true
}
```

`password` es opcional. Si se envía, se guarda hasheado con `PasswordHasher<User>` y nunca se devuelve en las respuestas.

Respuesta:

```json
{
  "id": 1,
  "name": "Juan",
  "email": "juan@test.com",
  "isActive": true,
  "createdAt": "2026-11-11T12:00:00Z",
  "updatedAt": null
}
```

### Addresses (1:N con Users)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/users/{userId}/addresses` | Crear dirección (404 si el usuario no existe) |
| `GET` | `/users/{userId}/addresses` | Listar direcciones del usuario (404 si el usuario no existe) |
| `PUT` | `/addresses/{id}` | Actualizar dirección |
| `DELETE` | `/addresses/{id}` | Eliminar dirección |

Body:

```json
{
  "street": "Calle Falsa 123",
  "city": "Asunción",
  "country": "Paraguay",
  "zipCode": "9999"
}
```

### Currencies y conversión

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/currencies` | Listar monedas |
| `POST` | `/currencies` | Crear moneda |
| `POST` | `/currency/convert` | Convertir un monto entre monedas |

```json
// POST /currencies
{
  "code": "EUR",
  "name": "Euro",
  "rateToBase": 7900
}
```

```json
// POST /currency/convert
{
  "fromCurrencyCode": "USD",
  "toCurrencyCode": "PYG",
  "amount": 100
}
```

Respuesta:

```json
{
  "fromCurrency": "USD",
  "toCurrency": "PYG",
  "originalAmount": 100,
  "convertedAmount": 730000
}
```

Fórmula:

```text
montoBase       = amount * from.RateToBase
convertedAmount = montoBase / to.RateToBase
```

El resultado se redondea a 4 decimales (`MidpointRounding.AwayFromZero`).

## Validaciones (FluentValidation)

| Caso | Reglas |
|------|--------|
| Crear usuario | `name` obligatorio (máx. 150), `email` obligatorio con formato válido (máx. 255) y único, `password` mínimo 6 si se envía |
| Editar usuario | `id > 0`, mismas reglas de `name`/`email`, email único excluyendo el propio usuario |
| Crear dirección | `userId > 0`, `street`, `city` y `country` obligatorios, `zipCode` máx. 20 |
| Editar dirección | `id > 0` y mismos campos requeridos |
| Crear moneda | `code` obligatorio, máx. 10 y único (normalizado a mayúsculas), `name` obligatorio, `rateToBase > 0` |
| Conversión | códigos obligatorios, `amount > 0`, ambas monedas deben existir |

Los errores de validación devuelven **400** con formato `ValidationProblem` (errores por propiedad), incluida la unicidad de email y de código de moneda. Los recursos inexistentes devuelven **404**. El **409** queda como respaldo para conflictos que se escapan de la validación, por ejemplo una carrera entre dos requests simultáneos.

## Cómo probar la API

### Opción 1: colección Postman (recomendada)

La colección está en:

```text
requests/CLTTestApi.postman_collection.json
```

Pasos:

1. Abrir Postman.
2. **Import → Files** y seleccionar el archivo.
3. Confirmar con **Import**.
4. Abrir la colección **CLTTestApi - Prueba Técnica** y revisar la pestaña **Variables**:

   | Variable | Valor |
   |----------|-------|
   | `baseUrl` | `http://localhost:5036` |
   | `apiKey` | `clt-demo-api-key-2026` |

   No hace falta importar un environment: la colección usa sus propias variables.

5. Verificar que la API esté corriendo (`dotnet run`).
6. Ejecutar la colección completa:

   - Clic en los tres puntos de la colección → **Run collection** → **Run CLTTestApi**.

   O ejecutar carpeta por carpeta, respetando el orden:

   ```text
   01 - Users
   02 - Addresses
   03 - Currencies
   04 - Currency Conversion
   05 - Seguridad
   06 - Limpieza
   ```

Resultado esperado: **23 requests**, **37 assertions** y **0 fallos** (2–3 segundos).

La colección:

- Genera emails y códigos de moneda únicos en cada ejecución, por lo que se puede correr varias veces.
- Guarda `userId` y `addressId` automáticamente para los pasos siguientes.
- Al final ejecuta **06 - Limpieza**, que elimina el usuario y la dirección creados.

> La moneda de prueba no se elimina porque la API no expone `DELETE /currencies`. Es un dato inocuo y desaparece al reiniciar la base.

También se puede ejecutar desde consola con Newman:

```bash
npx --yes newman run requests/CLTTestApi.postman_collection.json
```

Si se cambia el puerto de la API, actualizar la variable `baseUrl` en la colección.

### Opción 2: script cURL

```bash
bash requests/curl-examples.sh

# Personalizando host y clave
BASE_URL=http://localhost:5036 API_KEY=clt-demo-api-key-2026 bash requests/curl-examples.sh
```

El script recorre el flujo completo: 401 sin API Key, CRUD de usuario, direcciones, monedas, conversión y limpieza. Requiere Git Bash, WSL o Linux/macOS.

### Opción 3: archivo .http

`CLTTestApi.http` contiene ejemplos de todos los endpoints para la extensión **REST Client** de Visual Studio Code o Visual Studio.

## Estructura del proyecto

```text
CLTTestApi/
├── Domain/
│   ├── User.cs
│   ├── Address.cs
│   └── Currency.cs
├── Infrastructure/
│   ├── AppDbContext.cs
│   ├── DbSeeder.cs
│   └── Migrations/                 # Migración InitialCreate
├── Application/
│   ├── Common/                     # Excepciones y extensión de validación
│   ├── Users/
│   │   ├── Commands/               # CreateUser, UpdateUser, DeleteUser + validadores
│   │   ├── Queries/                # GetUsers, GetUserById
│   │   └── UserResponse.cs
│   ├── Addresses/
│   │   ├── Commands/               # CreateAddress, UpdateAddress, DeleteAddress + validadores
│   │   ├── Queries/                # GetAddressesByUser
│   │   └── AddressResponse.cs
│   ├── Currencies/
│   │   ├── Commands/               # CreateCurrency + validador
│   │   ├── Queries/                # GetCurrencies
│   │   └── CurrencyResponse.cs
│   ├── CurrencyConversion/         # ConvertCurrency + validador
│   └── DependencyInjection.cs      # Registro de handlers y validadores
├── Endpoints/                      # Minimal APIs (Users, Addresses, Currencies)
├── Middleware/
│   ├── ApiKeyMiddleware.cs         # Seguridad global por X-API-KEY
│   └── GlobalExceptionHandler.cs   # ProblemDetails (400/404/409/500)
├── requests/
│   ├── CLTTestApi.postman_collection.json
│   └── curl-examples.sh
├── Program.cs
├── appsettings.json                # ApiKey y cadena de conexión SQLite
└── CLTTestApi.http                 # Ejemplos de requests
```

Los endpoints solo orquestan: reciben el request, validan, llaman al handler y traducen el resultado a HTTP. La lógica vive en `Application` y la persistencia en `Infrastructure`. Los handlers se registran explícitamente en DI; no se usa una librería de mediator.

## Migraciones de EF Core (SQLite)

La aplicación aplica las migraciones automáticamente al iniciar. Para hacerlo manualmente:

```bash
# Restaurar la herramienta local dotnet-ef (definida en dotnet-tools.json)
dotnet tool restore

# Aplicar migraciones
dotnet ef database update

# Crear una nueva migración tras cambiar el modelo
dotnet ef migrations add NombreDeLaMigracion --output-dir Infrastructure/Migrations
```

`dotnet ef` requiere el SDK de .NET y el paquete `Microsoft.EntityFrameworkCore.Design` (ya incluido). Si la base fue creada antes de agregar migraciones, hay que borrarla para que `Migrate()` genere el esquema desde cero.

## Decisiones y alcance

Implementado:

- CRUD completo de Users y Addresses (1:N), y creación/listado de monedas.
- Conversión de divisas con `RateToBase` y seed de PYG/USD.
- Seguridad global por API Key con comparación en tiempo constante.
- CQRS por carpetas (Commands/Queries) con handlers independientes.
- FluentValidation para creación/edición de usuarios, direcciones, monedas y conversión.
- EF Core 10 + SQLite, índices únicos (`Users.Email`, `Currencies.Code`), FK con borrado en cascada y migración inicial.
- Manejo centralizado de errores con `ProblemDetails` y OpenAPI en Development.

Fuera de alcance:

- No se implementa autenticación de usuarios (login/JWT): la seguridad pedida es únicamente por API Key.
- El `password` es opcional porque el contrato de ejemplo solo envía `name` y `email`. Si se envía, se almacena hasheado y nunca se devuelve.
- La API Key se guarda en `appsettings.json` por requisito de la prueba. En producción debería usarse un secret manager o variables de entorno.
- No hay paginación en los listados.
- No se incluye un proyecto xUnit: la colección Postman cubre el flujo completo con tests automáticos.

## Solución de problemas

| Problema | Solución |
|----------|----------|
| `dotnet` no se reconoce | Instalar .NET SDK 10 y abrir una terminal nueva. Verificar con `dotnet --version`. |
| Puerto 5036 ocupado | Cerrar la otra instancia o ejecutar `dotnet run --urls http://localhost:5050` y actualizar `baseUrl`. |
| `401 Unauthorized` | Enviar el header `X-API-KEY: clt-demo-api-key-2026`. |
| `Database is locked` | Cerrar otras instancias de la API que estén usando `clt_test.db`. |
| `dotnet ef` no existe | Ejecutar `dotnet tool restore`. |
| Acentos raros con `curl` en Git Bash | Usar la colección Postman o `requests/curl-examples.sh`, que envía los acentos como escapes Unicode. |
