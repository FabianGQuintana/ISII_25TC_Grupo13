# InmoGestor

[cite_start]**InmoGestor** es un sistema de gestión de alquileres diseñado para el mercado argentino, orientado a automatizar la administración inmobiliaria y mitigar el riesgo de error financiero derivado de la inflación y la indexación compleja[cite: 3, 15, 16].

## Funcionalidades Principales

### 1. Parametrización y Sincronización de Contratos
* [cite_start]**Gestión de Índices:** Automatiza la creación de contratos vinculándose a APIs externas (como DolarApi) para obtener valores oficiales de **ICL, IPC o UVA**[cite: 27, 28, 89].
* [cite_start]**Reglas de Ajuste:** Permite configurar frecuencias de actualización (trimestral, semestral, etc.) de forma dinámica[cite: 27].

### 2. Gestión Financiera y Cobros
* [cite_start]**Cálculo Automático:** Al momento del pago, el motor de cálculo procesa el valor actualizado de la cuota según el índice vigente y suma automáticamente los recargos por **mora**[cite: 29].
* [cite_start]**Flujo de Aprobación Jerárquico:** Los pagos registrados por un **Operador** quedan en estado pendiente hasta ser auditados y confirmados por un usuario con rol **Superior**, momento en el que se genera el recibo oficial[cite: 30, 31, 32].

## Arquitectura del Backend (N-Tier)

[cite_start]El sistema utiliza una arquitectura **Cliente-Servidor** desacoplada, con un backend organizado en capas lógicas estrictas para garantizar mantenibilidad[cite: 96, 97, 100]:

* [cite_start]**Capa de API:** Proporciona los servicios RESTful mediante **ASP.NET Core** y gestiona la seguridad con **JWT**[cite: 112].
* [cite_start]**Capa de Negocio:** Contiene el motor de cálculo indexado, algoritmos de mora y las reglas de dominio[cite: 90, 99].
* [cite_start]**Capa de Datos:** Gestiona la persistencia en **SQL Server** utilizando consultas nativas optimizadas[cite: 112].
* [cite_start]**Capa de Entidades:** Define los modelos de datos (como `Persona`, `Contrato`, `Cuota`) transversales a toda la solución[cite: 99, 139, 145, 151].

## Stack Tecnológico

* [cite_start]**Backend:** .NET 8 (C#)[cite: 112].
* [cite_start]**Frontend:** React 18 con TypeScript y Vite[cite: 114].
* [cite_start]**UI Library:** Material UI (MUI)[cite: 115].
* [cite_start]**Base de Datos:** SQL Server[cite: 112].
