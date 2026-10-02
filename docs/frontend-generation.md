# Generación de frontend (`crudgen`)

La CLI puede generar, además del backend, el CRUD web de cada tabla: cliente API tipado, store de
estado, y componentes de Lista, Formulario (crear/editar) y Detalle, con rutas.

## Uso

```powershell
# Backend + frontend
crudgen generate --table Products,Orders --with-frontend

# Solo frontend
crudgen generate --table Products --frontend-only --frontend angular --state ngrx

# Solo el kernel compartido (cliente HTTP, controles de formulario); no requiere BD
crudgen init-web --frontend vue
```

Flags: `--with-frontend`, `--frontend-only`, `--frontend`, `--state`, `--styles`, `--force`.
Precedencia: **flag > `crudgen.config.json` > valor por defecto**.

## Configuración

```jsonc
{
  "frontend": {
    "framework": "react",        // "react" | "vue" | "angular"
    "stateManagement": "zustand", // ver matriz; por defecto el primero del framework
    "path": "frontend/src",       // relativo al repo, sin ".."
    "styles": "tailwind",         // por ahora solo "tailwind"
    "apiBaseUrl": "/api"          // prefijo de la API
  }
}
```

## Matriz soportada

| Framework | Estado            | Dependencias que asume el código generado            |
|-----------|-------------------|------------------------------------------------------|
| react     | `zustand`         | react-router-dom v6+, zustand                        |
| vue       | `pinia`           | vue 3, vue-router, pinia                             |
| angular   | `signals` (def.)  | Angular 17.2+, @angular/router, rxjs                 |
| angular   | `ngrx`            | lo anterior + `@ngrx/signals` (`signalStore`)        |

Estilos: Tailwind CSS en todos los casos.

## Qué se genera

```
<path>/
  shared/                 # kernel: se escribe una vez; se omite si existe (salvo --force)
    api/                  # http-client, api-error
    ui/                   # FormField, Text/TextArea/Number/Toggle/DateTime/Select inputs, Alert
  features/<plural-kebab>/
    model/                # tipos y validación
    api/                  # cliente REST de la entidad
    store/                # estado con isLoading y error
    components/           # List, Form, Detail (+ páginas/vistas)
    routes, index
```

## Rutas

Cada ejecución regenera `features/routes.generated.ts` con `featureRoutes` (todas las tablas generadas). Se conecta una sola vez:

- Angular: `export const routes: Routes = [...featureRoutes];` en `app.routes.ts`.
- Vue: `createRouter({ routes: [...featureRoutes] })`.
- React: `createBrowserRouter([...featureRoutes])`.

### Menú

También se reescribe `features/nav.generated.ts` con `featureNav = [{ label, path }]`. En tu sidenav existente recórrelo (o mézclalo: `[...tusItems, ...featureNav]`).

## Mapeo SQL -> TypeScript -> control UI

| Tipo C# de la columna            | TS        | Control                                           |
|----------------------------------|-----------|---------------------------------------------------|
| int, long, short, byte, decimal, double, float | number | number                              |
| bool                             | boolean   | checkbox / toggle                                 |
| DateTime, DateTimeOffset         | string    | datetime                                          |
| TimeSpan                         | string    | time                                              |
| string (nombre con `password`)   | string    | password                                          |
| string (nombre con `email`)      | string    | email                                             |
| string, longitud <= 255          | string    | text                                              |
| string, longitud > 255 o ilimitada | string  | textarea                                          |
| Clave foránea                    | tipo de la columna | select (consulta el endpoint de la tabla relacionada) |
| byte[]                           | string    | excluida del formulario y la lista                |

Reglas: la PK nunca es editable; la lista muestra la PK + hasta 5 columnas (sin textarea, password ni
binarios). El texto mostrado en un select es la primera columna que se llame `name`, `title`,
`description`, `label` o `code`; si no, la primera de texto; si no, la clave.

## Contrato esperado del backend

Ruta `api/{nombre-plural-en-minúsculas}`; `GET` devuelve un arreglo; `POST`/`PUT` reciben todas las
columnas no PK; `DELETE` responde 204; errores en `ProblemDetails`; JSON en camelCase (es lo que
genera el backend de la CLI).

## Limitaciones conocidas

- Sin claves primarias compuestas (la tabla se rechaza).
- Solo claves foráneas de una columna.
- Nombres de columna que no sean identificadores TypeScript válidos no se soportan.
- Columnas `byte[]` no se editan.
- Los selects de relaciones dependen de que exista el endpoint de la tabla relacionada.

## Añadir un nuevo stack

1. Crear plantillas Scriban en `src/MyCustomizedFramework.Infrastructure/CodeGeneration/Templates/Frontend/<stack>/{kernel,feature}`
   (las comunes de TypeScript están en `_shared`).
2. Crear una clase `IFrontendFrameworkPlan` en `CodeGeneration/Frontend/`: `Supports`, `KernelOutputs`
   y `FeatureOutputs` (plantilla -> ruta de salida bajo `Web/`). Ver `VuePiniaPlan` como ejemplo.
3. Registrarla en `ScribanFrontendFileGenerator` (y en `FrontendTemplateCatalog` si aplica).
4. Permitir la combinación en `StatesByFramework` de `tools/.../Configuration/FrontendConfig.cs`.
5. Añadir el caso al theory de `ScribanFrontendFileGeneratorTests` y a `FrontendOptionsResolverTests`.

Notas para plantillas: Scriban convierte miembros PascalCase a snake_case (`EntityName` -> `entity_name`);
evita `{{` literales (JSX `style={{`, interpolación Vue/Angular): usa `v-text`, `[textContent]` o
`{ {{~ expr ~}} }`. Para validar, vuelca la salida con la variable `FRONTEND_DUMP_DIR` al correr los
tests y compílala con `tsc`/`vue-tsc`/`ngc`.


