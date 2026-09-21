# MyCustomizedFramework

Plantilla de solución **.NET 10** basada en Clean Architecture/Onion Architecture.

## Estructura

```text
src/
  MyCustomizedFramework.Domain/          # Entidades, value objects y reglas de negocio
  MyCustomizedFramework.Application/    # Casos de uso y contratos de persistencia
  MyCustomizedFramework.Infrastructure/  # Implementaciones técnicas y composición de DI
  MyCustomizedFramework.Api/             # HTTP, controllers y configuración de la aplicación
tests/
  MyCustomizedFramework.UnitTests/
  MyCustomizedFramework.IntegrationTests/
```

## Dirección de dependencias

`Domain <- Application <- Infrastructure` y `Application <- Api`.

El dominio no conoce ninguna librería de infraestructura. La API compone las dependencias mediante
`AddInfrastructure`, y la persistencia actual en memoria puede reemplazarse por EF Core, Dapper u
otro adaptador sin modificar los casos de uso.

## Ejecutar

```powershell
dotnet restore
dotnet build
dotnet test .\tests\MyCustomizedFramework.UnitTests\MyCustomizedFramework.UnitTests.csproj
dotnet run --project .\src\MyCustomizedFramework.Api\MyCustomizedFramework.Api.csproj
```

Endpoints de ejemplo:

- `GET /api/products`
- `POST /api/products` con `{ "name": "Keyboard", "price": 25 }`

## Convenciones

- Un caso de uso por archivo dentro de `Application`.
- Las abstracciones viven en `Application`; sus implementaciones en `Infrastructure`.
- Las reglas de negocio se validan en `Domain`.
- Las nuevas funcionalidades deben organizarse por feature (`Products`, `Orders`, etc.), no por
  carpetas técnicas globales.
- Los errores HTTP deben modelarse con `ProblemDetails` y los límites de la aplicación deben
  registrar observabilidad explícita al agregar persistencia real.
