# AgendaContactos

Este proyecto es una agenda de contactos desarrollada en C# que he diseñado con una estructura modular, separando claramente la lógica de negocio, la persistencia, la validación y la cache. El objetivo principal ha sido construir una aplicación mantenible, testeable y preparada para crecer sin mezclar responsabilidades.

---

## Índice

- [Descripción general](#descripción-general)
- [Objetivos del proyecto](#objetivos-del-proyecto)
- [Arquitectura](#arquitectura)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Flujo de ejecución](#flujo-de-ejecución)
- [Capas y responsabilidades](#capas-y-responsabilidades)
- [Decisiones de negocio](#decisiones-de-negocio)
- [Buenas prácticas aplicadas](#buenas-prácticas-aplicadas)
- [Persistencia](#persistencia)
- [Cache](#cache)
- [Validación y normalización](#validación-y-normalización)
- [Manejo de errores](#manejo-de-errores)
- [Testing](#testing)
- [Posibles mejoras](#posibles-mejoras)

---

## Descripción general

**AgendaContactos** es una aplicación de backend/consola pensada para gestionar contactos. Permite:

- crear contactos,
- consultarlos por ID o alias,
- listarlos con paginación,
- actualizarlos,
- eliminarlos,
- validar las reglas de negocio antes de guardar datos,
- y cachear las consultas más frecuentes para mejorar el rendimiento.

He dividido el proyecto en dos partes principales:

- `AgendaContactos.Back`: contiene la lógica principal, infraestructura, persistencia y ejecución.
- `AgendaContactos.Tests`: contiene las pruebas automatizadas de cache, repositorio, servicios, normalización y validación.

---

## Objetivos del proyecto

Con este proyecto he intentado conseguir varios objetivos técnicos:

1. **Separar responsabilidades**  
   Cada parte del sistema tiene una función concreta: validación, persistencia, cache, servicios, etc.

2. **Aplicar reglas de negocio de forma explícita**  
   He modelado los errores de dominio con tipos propios para que el comportamiento sea claro y controlado.

3. **Mejorar el rendimiento en consultas repetidas**  
   Para ello he incorporado una cache LRU en memoria.

4. **Facilitar las pruebas**  
   He usado interfaces e inyección de dependencias para poder probar cada componente de forma aislada.

5. **Mantener una base escalable**  
   La estructura está pensada para poder añadir nuevas entidades o funcionalidades sin romper el diseño actual.

---

## Arquitectura

El proyecto sigue una arquitectura por capas, donde cada una tiene una responsabilidad bien definida:

- **Entrada / interfaz**: `Interface`
- **Servicios de aplicación**: `Services`
- **Dominio**: `Models`, `Validators`, `Errors`
- **Infraestructura**: `Repositories`, `AppDbContext`, `Cache`, `DependenciesProvider`
- **Utilidades**: `Utils`
- **Configuración**: `Configuration`

### Enfoque arquitectónico

Aunque no he aplicado una Clean Architecture estricta, sí me he basado en varios de sus principios:

- las dependencias apuntan hacia la lógica central,
- los servicios trabajan con interfaces,
- la persistencia queda encapsulada en repositorios,
- y los errores de dominio están tipados y no se gestionan solo con strings.

---

## Estructura del proyecto

### `AgendaContactos.Back`

Contiene la aplicación principal.

#### `Cache/`
Implementación de la cache.

- `Common/ICache.cs`: interfaz genérica para cualquier sistema de cache.
- `LruCache.cs`: implementación concreta de una cache LRU.

#### `Configuration/`
Constantes generales de configuración.

- `Config.cs`: define el código de país por defecto, la carpeta de base de datos y la capacidad de la cache.

#### `DTOs/`
Objetos de transferencia de datos.

- `ContactDto.cs`: modelo de entrada para crear o actualizar contactos.

#### `Errors/`
Jerarquía de errores de dominio.

- `Common/DomainError.cs`
- `Contact/ContactError.cs`
- `DataBase/DataBaseError.cs`

#### `Infraestructure/`
Composición e inyección de dependencias.

- `DependenciesProvider.cs`: registra repositorios, cache, validadores y servicios.

#### `Interface/`
Capa de entrada lógica.

- `Interface.cs`: simula operaciones tipo HTTP como `POST`, `GET`, `PUT` y `DELETE`.

#### `Models/`
Entidades de dominio.

- `Contact.cs`
- `Enums/Verb.cs`

#### `Repositories/`
Acceso a datos.

- `Common/ICrud.cs`
- `Contacts/IContactRepository.cs`
- `Contacts/ContactRepository.cs`
- `AppDbContext.cs`

#### `Services/`
Lógica de aplicación.

- `Crud/Common/ICrudService.cs`
- `Crud/Contacts/IContactsService.cs`
- `Crud/Contacts/ContactService.cs`

#### `Utils/`
Funciones auxiliares.

- `ContactDataNormalizer.cs`: normalización de los datos de entrada.

#### `Validators/`
Reglas de validación del dominio.

- `Common/IValidate.cs`
- `ContactValidator.cs`

#### `Program.cs`
Punto de entrada de la aplicación.

---

### `AgendaContactos.Tests`

Proyecto de pruebas automatizadas.

- `Cache/LruCache.cs`
- `Repository/ContactRepositoryTests.cs`
- `Service/ContactServiceTests.cs`
- `Utils/ContactDataNormalizerTests.cs`
- `Validators/ContactValidatorTests.cs`

---

## Flujo de ejecución

El flujo de una operación en la aplicación es el siguiente:

1. La petición entra por `Interface` o por la capa que consuma el servicio.
2. Los datos se normalizan con `ContactNormalizer`.
3. El contacto se valida con `ContactValidator`.
4. Se comprueba si ya existe el teléfono o el email.
5. Se guarda o actualiza el contacto en la base de datos.
6. Se actualiza la cache LRU.
7. Se devuelve un `Result<T, DomainError>` con éxito o error.

### Ejemplo de alta de contacto

Cuando creo un contacto:

- recibo un `ContactDto`,
- lo normalizo,
- lo valido,
- compruebo que no exista otro contacto con el mismo teléfono ni email,
- lo persisto en SQLite,
- y finalmente lo almaceno en cache.

---

## Capas y responsabilidades

### 1. Capa de dominio

Está formada principalmente por:

- `Models/Contact.cs`
- `Errors/*`
- `Validators/ContactValidator.cs`

#### Responsabilidad
Definir qué es un contacto válido, qué errores pueden producirse y qué reglas debe cumplir la entidad.

#### Lo que he buscado
- que el dominio no dependa de la infraestructura,
- que las reglas estén aisladas,
- y que los errores estén claramente modelados.

---

### 2. Capa de aplicación

Está representada por:

- `Services/Crud/Contacts/ContactService.cs`

#### Responsabilidad
Coordinar los casos de uso:

- normalizar,
- validar,
- comprobar duplicados,
- persistir,
- cachear,
- y devolver resultados funcionales.

#### Idea principal
El servicio no conoce detalles concretos de SQLite ni de EF Core. Esa responsabilidad queda en los repositorios.

---

### 3. Capa de infraestructura

Está formada por:

- `Repositories/Contacts/ContactRepository.cs`
- `Repositories/AppDbContext.cs`
- `Cache/LruCache.cs`
- `Infraestructure/DependenciesProvider.cs`

#### Responsabilidad
Resolver los detalles técnicos:

- acceso a la base de datos,
- consultas con Entity Framework Core,
- almacenamiento en memoria,
- y registro de dependencias.

---

### 4. Capa de entrada

Está representada por:

- `Interface/Interface.cs`
- `Program.cs`

#### Responsabilidad
Servir como punto de entrada de la aplicación y simular operaciones sobre contactos.

---

## Decisiones de negocio

### 1. El teléfono es la clave primaria

He decidido que `PhoneNumber` sea el identificador principal del contacto.

#### Motivo
- es un identificador natural,
- permite consultar y actualizar fácilmente,
- evita añadir un ID artificial innecesario.

#### Consecuencia
Si cambia el teléfono, la actualización debe tratarse como un cambio de clave primaria.

---

### 2. El email debe ser único

No permito que existan dos contactos con el mismo email.

#### Motivo
- evita duplicidades,
- mantiene coherencia en la información,
- y refuerza la integridad del modelo.

---

### 3. El alias no es obligatorio

Si el alias viene vacío o solo con espacios, utilizo el nombre como alias.

#### Motivo
- así siempre hay un valor útil,
- evito guardar alias vacíos,
- y mejoro la experiencia de uso.

---

### 4. Normalización del teléfono

Si el teléfono tiene 9 dígitos y no trae prefijo internacional, le añado `+34`.

#### Motivo
- homogeneizar el formato,
- aceptar entrada local,
- y guardar los teléfonos de forma consistente.

---

### 5. Uso de repositorios para acceso a datos

La capa de servicios no accede directamente a EF Core.

#### Motivo
- desacoplar la lógica de negocio,
- facilitar pruebas unitarias,
- y permitir cambiar la persistencia sin tocar la lógica principal.

---

### 6. Uso de cache LRU

He incorporado una cache LRU para conservar en memoria los contactos más consultados.

#### Motivo
- reducir accesos repetidos a la base de datos,
- mejorar tiempo de respuesta,
- y mantener solo los elementos más recientes o usados.

---

## Buenas prácticas aplicadas

### Separación de responsabilidades
Cada clase tiene una función concreta y bien delimitada.

### Uso de interfaces
Me permite invertir dependencias y facilitar el mocking en pruebas.

### Errores tipados
Los errores no se devuelven como texto suelto, sino como tipos de dominio.

### Resultado funcional
He usado `Result<T, DomainError>` para evitar depender de excepciones en errores esperables.

### Validación antes de persistencia
Primero normalizo y valido; después persisto.

### Normalización de entrada
Homogeneizo datos para reducir inconsistencias y evitar errores de formato.

### Pruebas automatizadas
He cubierto los componentes más importantes con tests.

### Inyección de dependencias
Centralizo la composición de objetos en `DependenciesProvider`.

### Logging estructurado
Uso Serilog para dejar trazas informativas, warnings y errores.

---

## Persistencia

La persistencia está implementada con:

- **Entity Framework Core**
- **SQLite**

### `AppDbContext`
Define el modelo relacional de la aplicación.

#### Configuración principal
- `PhoneNumber` como clave primaria.
- `Email` con índice único.
- límites de longitud en los campos.
- propiedades obligatorias.

### Base de datos
La base de datos se crea en una carpeta local definida en configuración:

- carpeta: `ContactData`
- archivo SQLite: `agenda.db`

---

## Cache

### `LruCache`
He implementado una cache LRU (*Least Recently Used*).

#### Comportamiento
- si añado un elemento nuevo, se guarda en memoria,
- si la cache supera su capacidad, expulsa el menos usado,
- cada acceso con `Obtain` actualiza el orden de uso,
- `Delete` elimina el elemento tanto del diccionario como del orden de referencias.

#### Motivo
La he usado para mejorar el rendimiento en consultas repetidas y reducir accesos a base de datos.

### Capacidad
La capacidad por defecto se define en `Config.CacheCapacity`.

---

## Validación y normalización

### Normalización
`ContactNormalizer` transforma los datos de entrada antes de validarlos:

- recorta espacios,
- elimina guiones en el teléfono,
- añade prefijo `+34` si el número es local,
- convierte el email a minúsculas,
- asigna el nombre como alias si el alias está vacío.

### Validación
`ContactValidator` comprueba que:

- el teléfono tenga formato correcto,
- el nombre no esté vacío y tenga longitud válida,
- el alias cumpla las reglas de longitud,
- el email tenga formato válido.

### Orden de ejecución
Mi orden de trabajo es siempre:

1. normalizar,
2. validar,
3. persistir.

Con esto reduzco errores y consigo una entrada de datos más homogénea.

---

## Manejo de errores

He modelado los errores de forma explícita mediante records:

- `DomainError`
- `ContactError`
- `DataBaseError`

### Ventajas
- los errores son claros y tipados,
- el mensaje es consistente,
- las pruebas son más fáciles,
- y separo errores funcionales de errores técnicos.

### Ejemplos
- contacto no encontrado,
- teléfono duplicado,
- email duplicado,
- validación incorrecta,
- error en base de datos.

---

## Testing

He incluido pruebas para los componentes más importantes del sistema.

### 1. Tests de cache
Comprueban:

- expulsión del elemento menos usado,
- actualización del orden,
- obtención de elementos,
- borrado,
- comportamiento ante capacidad inválida.

### 2. Tests de repositorio
Cubren:

- creación,
- borrado,
- búsqueda por ID,
- búsqueda por alias,
- actualización,
- paginación,
- existencia de email,
- y casos de error.

### 3. Tests de servicio
Verifican:

- creación,
- borrado,
- actualización,
- consulta por ID,
- consulta por alias,
- integración con cache,
- y escenarios de error.

### 4. Tests del normalizador
Comprueban:

- limpieza de datos,
- conversión de email,
- tratamiento del alias,
- formato del teléfono,
- y valores nulos o vacíos.

### 5. Tests del validador
Validan reglas de:

- teléfono,
- nombre,
- alias,
- email.

---

## Posibles mejoras

Aunque el proyecto ya está bastante completo, creo que todavía podría mejorar en varios puntos:

### 1. Separar todavía más el dominio
Podría mover más reglas al núcleo del dominio para reducir lógica en servicios.

### 2. Sustituir configuraciones estáticas
Sería más flexible usar configuración externa o `IOptions<T>`.

### 3. Mejorar la lógica de actualización
Especialmente cuando cambia la clave primaria, para simplificar el comportamiento y hacerlo más robusto.

### 4. Unificar mejor la gestión de errores
Hay algunos flujos que podrían revisarse para que los errores técnicos y funcionales estén aún más claros.

### 5. Añadir una capa de presentación real
Por ejemplo:
- una API REST,
- una minimal API,
- o una interfaz web.

### 6. Corregir algunos nombres
Podría renombrar algunas clases o carpetas para hacerlas más consistentes:
- `Infraestructure` → `Infrastructure`
- `ContactAlredyExist` → `ContactAlreadyExist`

### 7. Ampliar documentación XML
Podría extenderla a toda la solución para mejorar mantenibilidad.

### 8. Mejorar la estrategia de cache
Podría invalidar o actualizar elementos de forma más precisa en operaciones de actualización o borrado.

---

## Resumen

Con este proyecto he intentado construir una agenda de contactos con una arquitectura clara, modular y fácil de testear. Los aspectos más importantes que he aplicado son:

- separación de responsabilidades,
- validación explícita,
- normalización previa,
- uso de resultados funcionales,
- persistencia con EF Core + SQLite,
- cache LRU en memoria,
- y pruebas automatizadas sobre los componentes clave.

En conjunto, creo que el proyecto sirve como una base sólida para evolucionar hacia una aplicación más completa, como una API o un sistema de gestión de contactos más grande.

---

## Licencia

No he definido todavía una licencia para el proyecto.