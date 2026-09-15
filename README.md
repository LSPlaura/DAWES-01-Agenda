# AgendaContactos

AgendaContactos es una aplicación de consola desarrollada en C# para gestionar contactos con una arquitectura modular y enfoque en mantenibilidad, validación de dominio y testing.

## 1. Objetivo del proyecto

Este proyecto busca implementar una agenda de contactos aplicando buenas prácticas de desarrollo backend:

- separación de responsabilidades por capas,
- reglas de negocio explícitas,
- persistencia con EF Core + SQLite,
- cache en memoria para optimizar lecturas,
- y pruebas automatizadas para asegurar calidad.

## 2. Funcionalidades

La aplicación permite:

- crear contactos,
- obtener contacto por teléfono (ID),
- buscar contactos por alias,
- listar contactos con paginación,
- actualizar contactos (incluyendo cambio de teléfono),
- eliminar contactos.

## 3. Arquitectura y capas

El proyecto está dividido en dos soluciones principales:

- `AgendaContactos.Back`: lógica principal de negocio e infraestructura.
- `AgendaContactos.Tests`: pruebas unitarias y de integración.

### Capas en `AgendaContactos.Back`

- **Interface**: recibe operaciones tipo `POST`, `GET`, `PUT`, `DELETE`.
- **Services**: orquesta casos de uso y reglas de negocio.
- **Validators**: valida formato y reglas de entrada.
- **Utils**: normaliza datos antes de validar/persistir.
- **Repositories**: acceso a datos con Entity Framework Core.
- **Cache**: cache LRU en memoria para consultas frecuentes.
- **Dominio**: incluye entidades (`Models`) y errores de negocio (`Errors`).
- **Configuration**: constantes globales (prefijo país, tamaño cache).

## 4. Flujo de una operación

Ejemplo para crear un contacto:

1. La operación entra por `Interface`.
2. Se construye un `ContactDto`.
3. `ContactService` normaliza datos (`ContactNormalizer`).
4. Se valida el contacto (`ContactValidator`).
5. Se comprueban duplicados (teléfono/email).
6. Se persiste en SQLite (`ContactRepository`).
7. Se guarda en cache LRU.
8. Se devuelve `Result<T, DomainError>` con éxito o error.

## 5. Reglas de negocio principales

- El **teléfono** (`PhoneNumber`) es la clave primaria porque en este dominio actúa como identificador natural del contacto. Esto evita crear un `Id` artificial y simplifica operaciones de búsqueda, actualización y borrado.
- El **email es único** para preservar integridad y evitar contactos duplicados.
- Si el alias llega vacío, se usa el nombre como alias para evitar valores sin utilidad.
- Si el teléfono tiene 9 dígitos sin prefijo, se añade `+34` para homogeneizar el formato internacional.
- En una actualización, si el nuevo teléfono ya existe en base de datos, la operación solo continúa si pertenece al mismo contacto que se está actualizando.
- En una actualización, si el nuevo email ya existe en base de datos, la operación solo continúa si pertenece al mismo contacto que se está actualizando.

## 6. Persistencia y modelado de base de datos

La persistencia usa:

- **Entity Framework Core**
- **SQLite**

### Entidad principal: `Contact`

Campos:

- `PhoneNumber` (PK, requerido, max 16)
- `Name` (requerido, max 100)
- `Alias` (requerido, max 100)
- `Email` (requerido, max 256, índice único)

Estas restricciones se configuran en `AppDbContext`.

## 7. Cache

Se implementa una cache **LRU (Least Recently Used)** porque, en este contexto, interesa mantener en memoria los contactos consultados más recientemente.  
De esta forma, se acelera el acceso a los datos más usados y se reducen lecturas repetidas contra la base de datos.

- capacidad máxima configurable,
- `Add` inserta o actualiza elementos,
- `Obtain` recupera un elemento y actualiza su prioridad de uso,
- `Delete` elimina por clave,
- al superar la capacidad, se expulsa automáticamente el elemento menos utilizado recientemente.

## 8. Validación y normalización

### Normalización (`ContactNormalizer`)

Esta clase prepara y homogeneiza los datos de entrada antes de validarlos y persistirlos.  
Su objetivo es reducir inconsistencias de formato y simplificar la validación posterior.

Comportamiento aplicado:

- recorta espacios en blanco en los campos de texto,
- limpia el teléfono eliminando espacios y guiones,
- añade el prefijo internacional por defecto (`+34`) cuando el teléfono llega en formato local (9 dígitos),
- convierte el email a minúsculas,
- si el alias llega vacío o con espacios, asigna el nombre normalizado como alias,
- si algún campo llega nulo o vacío, devuelve `string.Empty` para evitar valores nulos en el modelo.

### Validación (`ContactValidator`)
- **Teléfono con regex internacional**: se valida con el patrón `^\+[1-9]\d{6,14}$`.
Un teléfono es válido si:
- empieza por `+`,
- después tiene un primer dígito entre `1` y `9` (no puede ser `0`),
- y a continuación contiene entre `6` y `14` dígitos adicionales (`\d`).

Con esta regla, la longitud total tras `+` queda entre **7 y 15 dígitos**, alineada con el estándar internacional (E.164), que permite prefijo de país + número nacional dentro de ese rango.
- **Nombre y alias con longitud válida**: deben tener entre **1 y 100 caracteres**.  
  Al ser una aplicación de uso personal, la validación no restringe tipos concretos de caracteres, por lo que se permiten caracteres especiales.

- **Email con formato correcto**: se valida que tenga estructura `usuario@dominio.extensión`, cumpliendo estas reglas:
   - la parte de **usuario** debe tener al menos 2 caracteres,
   - la parte de **dominio** debe tener al menos 2 caracteres,
   - la **extensión** debe tener al menos 2 caracteres,
   - debe existir un `@` para separar usuario y dominio,
   - debe existir un `.` para separar dominio y extensión.

## 9. Manejo de errores

El manejo de errores sigue un enfoque **ROP (Railway-Oriented Programming)** usando `Result<T, DomainError>`.  
En lugar de usar excepciones para errores de negocio esperables, se devuelven errores tipados y controlados. Esto evita interrumpir el flujo normal de la aplicación ante situaciones habituales (por ejemplo, datos inválidos o contacto no encontrado) y mejora el feedback al usuario.

Además, encapsular los errores en tipos propios facilita:

- mantener mensajes consistentes,
- diferenciar errores funcionales de errores técnicos,
- simplificar pruebas y mantenimiento.

Jerarquía de errores:

- `DomainError` (base común),
- `ContactError` (errores funcionales del dominio de contactos),
- `DataBaseError` (errores técnicos de persistencia).

Ejemplos:
- contacto no encontrado,
- teléfono duplicado,
- email duplicado,
- datos inválidos.

## 10. Testing

`AgendaContactos.Tests` incluye pruebas de:

- cache LRU,
- repositorio con SQLite in-memory,
- servicio con mocks (Moq),
- normalización de datos,
- validación de reglas.

Se prueban escenarios válidos e inválidos para asegurar comportamiento funcional y técnico.

## 11. Tecnologías utilizadas

- **.NET / C#**: plataforma y lenguaje principal del proyecto.
- **Entity Framework Core**: ORM para mapear entidades de dominio y gestionar persistencia sin SQL manual en la lógica de negocio.
- **SQLite**: base de datos ligera embebida, ideal para entorno local y proyectos pequeños/medianos.
- **Serilog**: logging estructurado para trazabilidad de operaciones, validaciones y errores.
- **CSharpFunctionalExtensions**: uso de `Result<T, DomainError>` para modelar flujos de éxito/error sin depender de excepciones en casos esperables.
- **NUnit**: framework de testing para pruebas unitarias e integración.
- **FluentAssertions**: aserciones expresivas y legibles en tests.
- **Moq**: mocking de dependencias para aislar servicios y validar comportamiento.

## 12. Ejecución del proyecto

1. Restaurar dependencias:
   ```bash
   dotnet restore
   ```

2. Ejecutar proyecto principal:
   ```bash
   dotnet run --project AgendaContactos.Back
   ```

3. Ejecutar tests:
   ```bash
   dotnet test
   ```

## 13. Buenas prácticas aplicadas

- separación por capas,
- uso de interfaces e inyección de dependencias,
- validación previa a persistencia,
- errores de dominio tipados,
- uso de cache para rendimiento,
- tests automatizados con cobertura de casos críticos,
- logging estructurado con Serilog.