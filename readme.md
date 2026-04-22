# InmoGestor

**InmoGestor** es un sistema de gestión de alquileres diseñado para el mercado argentino, orientado a automatizar la administración inmobiliaria y mitigar el riesgo de error financiero derivado de la inflación y la indexación compleja

## Funcionalidades Principales

### 1. Parametrización y Sincronización de Contratos
* **Gestión de Índices:** Automatiza la creación de contratos vinculándose a APIs externas para obtener valores oficiales de **ICL, IPC**
* **Reglas de Ajuste:** Permite configurar frecuencias de actualización (trimestral, semestral, etc.) de forma dinámica

### 2. Gestión Financiera y Cobros
* **Cálculo Automático:** Al momento del pago, el motor de cálculo procesa el valor actualizado de la cuota según el índice vigente y suma automáticamente los recargos por **mora**
* **Flujo de Aprobación Jerárquico:** Los pagos registrados por un **Operador** quedan en estado pendiente hasta ser auditados y confirmados por un usuario con rol **Superior**, momento en el que se genera el recibo oficial

## Arquitectura del Backend (N-Tier)

El sistema utiliza una arquitectura **Cliente-Servidor** desacoplada, con un backend organizado en capas lógicas estrictas para garantizar mantenibilidad:

* **Capa de API:** Proporciona los servicios RESTful mediante **ASP.NET Core** y gestiona la seguridad con **JWT**.
* **Capa de Negocio:** Contiene el motor de cálculo indexado, algoritmos de mora y las reglas de dominio.
* **Capa de Datos:** Gestiona la persistencia en **MySQL** utilizando consultas nativas optimizadas.
* **Capa de Entidades:** Define los modelos de datos (como `Persona`, `Contrato`, `Cuota`) transversales a toda la solución.

## Stack Tecnológico

* **Backend:** .NET 8 (C#).
* **Frontend:** React 18 con TypeScript y Vite.
* **UI Library:** Material UI (MUI).
* **Base de Datos:** SQL Server.
