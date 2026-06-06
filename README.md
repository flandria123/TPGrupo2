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
    %% Definición del Cliente
    Client(("Swagger UI / Cliente"))

    %% Definición de los Microservicios
    subgraph Arquitectura de Microservicios UBA
        %% Fila Superior (Nivel 1)
        OrdersAPI["Orders.API\nPuerto: 7003"]
        CartAPI["Cart.API\nPuerto: 7004"]
        
        %% Fila Inferior (Nivel 2)
        UsersAPI["Users.API\nPuerto: 7002"]
        ProductsAPI["Products.API\nPuerto: 7001"]
        NotificationsAPI["Notifications.API\nPuerto: 7005"]
    end

    %% Peticiones HTTP del Cliente a los Microservicios
    %% Usamos "-->" (corto) para forzar que Orders y Cart queden arriba
    Client -->|HTTP| OrdersAPI
    Client -->|HTTP| CartAPI
    
    %% Usamos "--->" (largo) para forzar que el resto baje un escalón
    Client --->|HTTP| UsersAPI
    Client --->|HTTP| ProductsAPI
    Client --->|HTTP| NotificationsAPI

    %% Comunicación interna entre Microservicios (Validaciones cruzadas)
    %% Ahora las flechas bajarán limpiamente sin atravesar a Orders
    OrdersAPI -.->|Consulta Stock| ProductsAPI
    OrdersAPI -.->|Valida Usuario| UsersAPI
    CartAPI -.->|Consulta Stock| ProductsAPI
    
    %% Bases de Datos
    subgraph Bases de Datos SQLite Local
        DB_O[("app.db")]
        DB_C[("app.db")]
        DB_U[("app.db")]
        DB_P[("app.db")]
        DB_N[("app.db")]
    end

    %% Conexión a DBs
    OrdersAPI --- DB_O
    CartAPI --- DB_C
    UsersAPI --- DB_U
    ProductsAPI --- DB_P
    NotificationsAPI --- DB_N

    %% Estilos (Opcional, para que se vea azul como en tu foto)
    style ProductsAPI fill:#1e3a8a,stroke:#000,color:#fff
    style UsersAPI fill:#1e3a8a,stroke:#000,color:#fff
    style OrdersAPI fill:#1e3a8a,stroke:#000,color:#fff
    style CartAPI fill:#1e3a8a,stroke:#000,color:#fff
    style NotificationsAPI fill:#1e3a8a,stroke:#000,color:#fff
    style Client fill:#fcd34d,stroke:#b45309,color:#000
    style DB_O fill:#fef3c7,stroke:#000
    style DB_C fill:#fef3c7,stroke:#000
    style DB_U fill:#fef3c7,stroke:#000
    style DB_P fill:#fef3c7,stroke:#000
    style DB_N fill:#fef3c7,stroke:#000