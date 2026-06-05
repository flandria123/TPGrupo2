# Trabajo Práctico: Arquitectura de Microservicios E-Commerce

**Materia:** Construcción de Aplicaciones Informáticas  
**Institución:** Universidad de Buenos Aires (UBA)  
**Año:** 2026  

## 👥 Integrantes del Grupo
* Christian Gabriel Jackson
* Tomas Ponti
* Tomas Ustimczuk

---

## 📝 Descripción General
Este proyecto implementa un sistema de E-Commerce basado en una arquitectura de microservicios distribuidos [cite: 1]. La solución expone 5 REST APIs independientes desarrolladas en **C# con .NET Core 8**, cumpliendo con los contratos de diseño, manejo de excepciones de dominio, y observabilidad exigidos por la cátedra [cite: 1, 2].

## 🏗️ Arquitectura y Tecnologías
El proyecto fue construido siguiendo estrictas reglas de separación de responsabilidades y buenas prácticas:
* **Framework:** ASP.NET Core 8 (Minimal APIs / Controllers) [cite: 1, 36].
* **Persistencia:** Base de datos embebida **SQLite** mapeada a través del micro-ORM **Dapper**. No requiere instalación de servidor externo [cite: 62].
* **Comunicación HTTP:** Implementación de `IHttpClientFactory` para la comunicación segura y eficiente entre microservicios (Ej: Cart -> Products) [cite: 36, 82].
* **Manejo de Errores Global:** Ausencia total de bloques `try-catch` en la capa de negocio. Se delega la captura a un `IExceptionHandler` global que formatea las respuestas bajo el estándar **RFC 7231 Problem Details**, utilizando excepciones de dominio (`NotFoundException`, `BusinessRuleException`) [cite: 44, 47, 83].
* **Observabilidad:** 
  * Logging estructurado con **Serilog** (Doble Sink: Consola para errores y Archivo `audit.log` para tracking de requests) [cite: 54, 56].
  * Monitoreo en tiempo real con **Health Checks** (`/health` y `/health-ui`) [cite: 34, 69].

---

## 🔌 Mapeo de Puertos y Servicios
Cada microservicio corre de forma independiente en su propio puerto local:

| Microservicio | Puerto Local | Swagger UI | Health Check UI |
| :--- | :--- | :--- | :--- |
| **Products API** | `7001` | `https://localhost:7001/swagger` | `https://localhost:7001/health-ui` |
| **Users API** | `7002` | `https://localhost:7002/swagger` | `https://localhost:7002/health-ui` |
| **Orders API** | `7003` | `https://localhost:7003/swagger` | `https://localhost:7003/health-ui` |
| **Cart API** | `7004` | `https://localhost:7004/swagger` | `https://localhost:7004/health-ui` |
| **Notifications API** | `7005` | `https://localhost:7005/swagger` | `https://localhost:7005/health-ui` |

---

## ⚙️ Prerrequisitos
No es necesario instalar motores de bases de datos complejos. El proyecto requiere únicamente:
* **SDK de .NET 8** instalado.
* **Visual Studio** (o IDE compatible).
* Las dependencias de NuGet se restaurarán automáticamente al compilar. Los paquetes principales utilizados son [cite: 36, 74, 75]:
  * `Microsoft.Data.Sqlite` y `Dapper`
  * `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
  * `Swashbuckle.AspNetCore`
  * `AspNetCore.HealthChecks.UI`
  * `IdentityModel` y `KubernetesClient`

---

## 🚀 Pasos de Ejecución
La solución está configurada para inicializar y crear las bases de datos `app.db` automáticamente de forma local durante el arranque [cite: 64].

Para probar el flujo completo del E-Commerce (comunicación entre APIs), es necesario levantar los 5 microservicios simultáneamente:

1. Abrir el archivo `ECommerce.sln` con Visual Studio.
2. Hacer clic derecho sobre la Solución (`ECommerce`) en el Explorador de Soluciones y seleccionar **"Propiedades"** (Properties).
3. Ir a la sección **"Proyecto de inicio"** (Startup Project).
4. Seleccionar la opción **"Proyectos de inicio múltiples"** (Multiple startup projects).
5. Cambiar la acción de los 5 proyectos (`Products.API`, `Users.API`, `Orders.API`, `Cart.API`, `Notifications.API`) al valor **"Iniciar"** (Start).
6. Hacer clic en "Aceptar".
7. Presionar **F5** o hacer clic en el botón de **"Iniciar"** en la barra superior.
8. Se abrirán 5 ventanas de consola (Logs de Serilog) y 5 pestañas en tu navegador apuntando a la documentación de Swagger de cada API [cite: 54, 73].

---

## 🗺️ Diagrama de Arquitectura Lógico

```mermaid
graph TD
    %% Definición de estilos
    classDef api fill:#08427b,stroke:#000,stroke-width:2px,color:#fff
    classDef db fill:#005f73,stroke:#000,stroke-width:1px,color:#fff
    classDef ui fill:#e9c46a,stroke:#000,stroke-width:2px,color:#000

    %% Cliente
    Client((Swagger UI / Cliente)):::ui

    %% Microservicios
    subgraph Arquitectura de Microservicios UBA
        direction LR
        P[Products.API<br/>Puerto: 7001]:::api
        U[Users.API<br/>Puerto: 7002]:::api
        O[Orders.API<br/>Puerto: 7003]:::api
        C[Cart.API<br/>Puerto: 7004]:::api
        N[Notifications.API<br/>Puerto: 7005]:::api
    end

    %% Bases de Datos
    subgraph Bases de Datos SQLite Local
        DB_P[(app.db)]:::db
        DB_U[(app.db)]:::db
        DB_O[(app.db)]:::db
        DB_C[(app.db)]:::db
        DB_N[(app.db)]:::db
    end

    %% Relaciones Cliente -> APIs
    Client -->|HTTP| P
    Client -->|HTTP| U
    Client -->|HTTP| O
    Client -->|HTTP| C
    Client -->|HTTP| N

    %% Comunicación interna entre Microservicios
    O -.->|Consulta Stock| P
    O -.->|Valida Usuario| U
    C -.->|Consulta Stock| P

    %% Relaciones APIs -> DBs
    P --- DB_P
    U --- DB_U
    O --- DB_O
    C --- DB_C
    N --- DB_N

--------------------------------------------------------------------------------