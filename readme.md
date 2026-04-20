# InmoGestor

InmoGestor es una plataforma integral para la administracion de alquileres diseñada para automatizar la gestion inmobiliaria y asegurar la precision financiera en contextos de actualizacion constante.

---

## Funcionalidades Principales

### 1. Generacion de Contratos
Automatiza la creacion de contratos de alquiler mediante la conexion a una API externa para obtener indices oficiales (ICL o IPC). Esta funcionalidad permite establecer las bases para los ajustes automaticos segun la normativa vigente.

### 2. Gestion de Pagos y Mora
Centraliza la recaudacion de cuotas integrando:
* **Calculo de Mora:** Computo automatico de intereses por pagos fuera de termino.
* **Actualizacion de Indices:** Conexion a la API en el momento del cobro para aplicar el indice de ajuste vigente, garantizando una trazabilidad financiera exacta.

---

## Arquitectura del Sistema

El backend esta desarrollado bajo un esquema de **Arquitectura N-Tier (N-Capas)** y de **cliente/servidor**, estructurado de la siguiente manera:

* **Capa de API:** Gestiona los endpoints y la comunicacion con el frontend.
* **Capa de Negocio:** Contiene las reglas de dominio, calculos de mora y logica de actualizacion de indices.
* **Capa de Datos:** Administra la persistencia y la comunicacion con la base de datos (Supabase).
* **Capa de Entidades:** Define los modelos de datos transversales que representan los objetos de dominio en todas las capas.

---

## Stack Tecnologico

* **Frontend:** React / React Native.
* **Backend:** .NET (C#).
* **Base de Datos:** PostgreSQL (Supabase).
* **Integraciones:** API de indices economicos oficiales.
