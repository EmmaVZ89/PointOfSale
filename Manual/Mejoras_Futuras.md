# Mejoras Futuras - Sistema Punto de Venta

Este documento lista las potenciales mejoras al sistema, organizadas por prioridad y complejidad.
El sistema actual es una version basica funcional. Estas mejoras lo convertiran en una solucion mas completa.

---

## Indice

1. [Mejoras de Alta Prioridad](#1-mejoras-de-alta-prioridad)
2. [Mejoras de Media Prioridad](#2-mejoras-de-media-prioridad)
3. [Mejoras de Baja Prioridad](#3-mejoras-de-baja-prioridad)
4. [Mejoras Avanzadas (Largo Plazo)](#4-mejoras-avanzadas-largo-plazo)
5. [Consideraciones Tecnicas](#5-consideraciones-tecnicas)

---

## 1. Mejoras de Alta Prioridad

Mejoras que agregarian valor inmediato al sistema con complejidad moderada.

### 1.1 Multiples Formas de Pago

**Estado actual:** Solo maneja pago en efectivo.

**Mejora propuesta:**
- Agregar opciones: Efectivo, Tarjeta Debito, Tarjeta Credito, Transferencia, QR
- Registrar metodo de pago en cada venta
- Permitir pagos mixtos (parte efectivo, parte tarjeta)
- Reportes por forma de pago

**Complejidad:** Media
**Tablas afectadas:** Ventas (agregar columna MetodoPago o tabla intermedia)

---

### 1.2 Cierre de Caja / Arqueo

**Estado actual:** No hay control de apertura/cierre de caja.

**Mejora propuesta:**
- Apertura de caja con monto inicial
- Cierre de caja con arqueo (conteo de efectivo)
- Registro de diferencias (sobrantes/faltantes)
- Movimientos de caja (retiros, ingresos)
- Reporte de caja por turno/dia

**Complejidad:** Media-Alta
**Tablas nuevas:** Cajas, MovimientosCaja, AperturasCierres

---

### 1.3 Precios de Costo y Margen de Ganancia

**Estado actual:** Solo se registra el precio de venta.

**Mejora propuesta:**
- Agregar campo PrecioCosto a Articulos
- Calcular margen de ganancia automatico
- Reportes de rentabilidad por producto
- Alertas cuando el margen es bajo

**Complejidad:** Baja
**Tablas afectadas:** Articulos (agregar columna PrecioCosto)

---

### 1.4 Alertas de Stock Minimo

**Estado actual:** Se muestra en dashboard productos con stock < 10.

**Mejora propuesta:**
- Permitir definir stock minimo por producto
- Notificaciones visuales al iniciar sesion
- Reporte de productos bajo stock minimo
- Opcion de enviar alerta por email

**Complejidad:** Baja-Media
**Tablas afectadas:** Articulos (agregar columna StockMinimo)

---

### 1.5 Lector de Codigo de Barras

**Estado actual:** Busqueda manual de productos.

**Mejora propuesta:**
- Soporte para lectores USB (funcionan como teclado)
- Detectar codigo escaneado en campo de busqueda
- Agregar producto automaticamente al carrito
- Generar codigos de barras para productos sin codigo

**Complejidad:** Baja (lectores USB no requieren driver especial)
**Nota:** El campo Codigo ya existe, solo adaptar la interfaz

---

### 1.6 Descuentos

**Estado actual:** No hay sistema de descuentos.

**Mejora propuesta:**
- Descuento por porcentaje o monto fijo
- Descuento por producto individual
- Descuento general a la venta
- Registrar descuento en base de datos para reportes

**Complejidad:** Media
**Tablas afectadas:** Ventas (agregar Descuento), Ventas_Detalle (opcional)

---

## 2. Mejoras de Media Prioridad

Mejoras que agregarian funcionalidad importante pero no son criticas.

### 2.1 Cuentas Corrientes de Clientes

**Estado actual:** Todas las ventas son al contado.

**Mejora propuesta:**
- Permitir ventas a credito (fiado)
- Saldo pendiente por cliente
- Limite de credito configurable
- Registro de pagos parciales
- Reporte de deudores
- Estado de cuenta por cliente

**Complejidad:** Alta
**Tablas nuevas:** CuentasCorrientes, Pagos

---

### 2.2 Gestion de Proveedores

**Estado actual:** No hay registro de proveedores.

**Mejora propuesta:**
- ABM de proveedores (similar a clientes)
- Asociar productos a proveedores
- Historial de compras por proveedor
- Datos de contacto y condiciones de pago

**Complejidad:** Media
**Tablas nuevas:** Proveedores, ArticulosProveedores

---

### 2.3 Ordenes de Compra

**Estado actual:** El stock se ajusta manualmente.

**Mejora propuesta:**
- Crear ordenes de compra a proveedores
- Estados: Pendiente, Parcial, Recibida, Cancelada
- Recepcion de mercaderia actualiza stock
- Historial de compras

**Complejidad:** Alta
**Tablas nuevas:** OrdenesCompra, OrdenesCompraDetalle

---

### 2.4 Impresion de Etiquetas

**Estado actual:** No hay generacion de etiquetas.

**Mejora propuesta:**
- Generar etiquetas con codigo de barras
- Formato configurable (codigo, nombre, precio)
- Impresion en hojas A4 o impresora de etiquetas
- Seleccion masiva de productos

**Complejidad:** Media
**Bibliotecas sugeridas:** ZXing.NET para codigo de barras

---

### 2.5 Historial de Precios

**Estado actual:** Los cambios de precio no se registran.

**Mejora propuesta:**
- Registrar cada cambio de precio con fecha
- Consultar precio en una fecha especifica
- Reporte de variacion de precios
- Util para analisis de inflacion

**Complejidad:** Baja-Media
**Tablas nuevas:** HistorialPrecios

---

### 2.6 Notas de Credito

**Estado actual:** Solo existe cancelacion total de venta.

**Mejora propuesta:**
- Devolucion parcial de productos
- Generar nota de credito
- Afecta stock y reportes
- Opcion de reembolso o credito a cuenta

**Complejidad:** Media-Alta
**Tablas nuevas:** NotasCredito, NotasCreditoDetalle

---

### 2.7 Promociones y Combos

**Estado actual:** No hay sistema de promociones.

**Mejora propuesta:**
- Promociones por fecha (ej: descuento fin de semana)
- Combos (2x1, llevando X unidades)
- Promociones por cliente VIP
- Precios especiales por volumen

**Complejidad:** Alta
**Tablas nuevas:** Promociones, PromocionesArticulos, CondicionesPromocion

---

## 3. Mejoras de Baja Prioridad

Mejoras que son utiles pero pueden esperar.

### 3.1 Backup Automatico

**Estado actual:** Backup manual.

**Mejora propuesta:**
- Programar backups automaticos
- Guardar en carpeta local o nube
- Notificar si falla el backup
- Rotacion de backups antiguos

**Complejidad:** Media
**Nota:** PostgreSQL tiene herramientas nativas (pg_dump)

---

### 3.2 Logs y Auditoria

**Estado actual:** No hay registro de acciones.

**Mejora propuesta:**
- Registrar todas las operaciones criticas
- Quien, cuando, que se modifico
- Consulta de historial por entidad
- Util para detectar errores o fraudes

**Complejidad:** Media
**Tablas nuevas:** LogsAuditoria

---

### 3.3 Multi-Moneda

**Estado actual:** Solo pesos argentinos.

**Mejora propuesta:**
- Soporte para dolares (comun en Argentina)
- Cotizacion del dia configurable
- Ventas en dolares con conversion automatica
- Reportes en ambas monedas

**Complejidad:** Media-Alta
**Tablas afectadas:** Ventas, configuracion general

---

### 3.4 Exportacion Contable

**Estado actual:** Solo exporta CSV y PDF.

**Mejora propuesta:**
- Formato para software contable
- Libro IVA Ventas
- Resumen mensual por alicuotas
- Exportar a formatos estandar (ej: TXT para AFIP)

**Complejidad:** Media
**Nota:** Requiere conocimiento de formatos contables argentinos

---

### 3.5 Configuracion Personalizable

**Estado actual:** Datos del negocio hardcodeados.

**Mejora propuesta:**
- Pantalla de configuracion general
- Nombre del negocio, direccion, telefono, CUIT
- Logo personalizable
- Formato de factura/presupuesto
- Parametros del sistema (stock minimo default, etc.)

**Complejidad:** Baja-Media
**Tablas nuevas:** Configuracion

---

### 3.6 Conversion de Unidades

**Estado actual:** Cada producto tiene una unidad fija.

**Mejora propuesta:**
- Vender pack completo o unidades sueltas
- Ej: Cerveza en pack de 6 o individual
- Conversion automatica de stock
- Precios diferentes por presentacion

**Complejidad:** Alta
**Tablas nuevas:** Presentaciones, ArticulosPresentaciones

---

## 4. Mejoras Avanzadas (Largo Plazo)

Requieren cambios significativos o integraciones externas.

### 4.1 Facturacion Electronica AFIP

**Estado actual:** Solo genera presupuestos (no tiene validez fiscal).

**Mejora propuesta:**
- Integracion con Web Services de AFIP
- Emision de Factura A, B, C electronica
- CAE (Codigo de Autorizacion Electronico)
- Nota de credito electronica
- Punto de venta fiscal

**Complejidad:** Muy Alta
**Requisitos:**
- Certificado digital de AFIP
- Conocimiento de WSDL/SOAP
- Manejo de errores de AFIP
- Testing con ambiente de homologacion

**Bibliotecas sugeridas:** PyAFIPWS (Python) o implementacion propia en C#

---

### 4.2 Multiples Sucursales

**Estado actual:** Sistema mono-sucursal.

**Mejora propuesta:**
- Registro de varias sucursales/depositos
- Stock por sucursal
- Transferencias entre sucursales
- Reportes consolidados o por sucursal
- Usuarios asignados a sucursales

**Complejidad:** Muy Alta
**Tablas nuevas:** Sucursales, StockSucursal, Transferencias

---

### 4.3 Version Web / API REST

**Estado actual:** Aplicacion de escritorio Windows.

**Mejora propuesta:**
- API REST para exponer funcionalidades
- Frontend web (React, Angular, Vue)
- Acceso desde cualquier dispositivo
- Dashboard online
- App movil complementaria

**Complejidad:** Muy Alta
**Tecnologias sugeridas:** ASP.NET Core Web API, React/Angular

---

### 4.4 Integracion Mercado Pago

**Estado actual:** No hay integracion con pasarelas de pago.

**Mejora propuesta:**
- Cobro con QR de Mercado Pago
- Point (lector de tarjetas)
- Registro automatico del pago
- Conciliacion de cobros

**Complejidad:** Alta
**Nota:** Mercado Pago tiene SDK para .NET

---

### 4.5 Integracion con E-commerce

**Estado actual:** Solo venta presencial.

**Mejora propuesta:**
- Sincronizar productos con Mercado Libre
- Sincronizar con tienda Shopify/WooCommerce
- Stock unificado online/offline
- Recibir pedidos web

**Complejidad:** Muy Alta

---

### 4.6 Business Intelligence / Reportes Avanzados

**Estado actual:** Reportes basicos.

**Mejora propuesta:**
- Dashboard con graficos interactivos
- Analisis de tendencias
- Prediccion de ventas
- Segmentacion de clientes (RFM)
- Exportar a Power BI / Tableau

**Complejidad:** Alta
**Tecnologias sugeridas:** LiveCharts avanzado, Power BI Embedded

---

## 5. Consideraciones Tecnicas

### 5.1 Arquitectura Actual

El sistema sigue una arquitectura de 3 capas:
- **Capa Presentacion:** WPF (.NET Framework 4.7.2)
- **Capa Negocio:** Logica de negocio en C#
- **Capa Datos:** Acceso a PostgreSQL via Npgsql

### 5.2 Recomendaciones de Escalabilidad

1. **Migracion a .NET 6/7/8:** Mayor rendimiento y soporte a largo plazo
2. **Patron Repository:** Abstraer acceso a datos para facilitar cambios
3. **Inyeccion de Dependencias:** Mejorar testabilidad
4. **Entity Framework Core:** ORM para simplificar queries
5. **Async/Await:** Ya implementado parcialmente, completar en toda la app

### 5.3 Seguridad

Mejoras de seguridad recomendadas:
- Cambiar patron de encriptacion por bcrypt/Argon2
- Implementar timeout de sesion
- Bloqueo de cuenta por intentos fallidos
- HTTPS si se implementa version web
- Sanitizacion de inputs (ya se usa parametros)

### 5.4 UX/UI

Mejoras de experiencia de usuario:
- Atajos de teclado mas completos
- Modo offline con sincronizacion
- Accesibilidad (lectores de pantalla)
- Tutoriales interactivos / ayuda contextual
- Modo kiosko (pantalla completa)

---

## Resumen de Prioridades

| Mejora | Prioridad | Complejidad | Impacto |
|--------|-----------|-------------|---------|
| Multiples formas de pago | Alta | Media | Alto |
| Cierre de caja | Alta | Media-Alta | Alto |
| Precios de costo | Alta | Baja | Medio |
| Alertas stock minimo | Alta | Baja | Medio |
| Lector codigo barras | Alta | Baja | Alto |
| Descuentos | Alta | Media | Medio |
| Cuentas corrientes | Media | Alta | Alto |
| Proveedores | Media | Media | Medio |
| Ordenes de compra | Media | Alta | Medio |
| Facturacion AFIP | Media | Muy Alta | Muy Alto |
| Version web | Baja | Muy Alta | Alto |
| Multi-sucursal | Baja | Muy Alta | Alto |

---

## Proximos Pasos Sugeridos

1. **Corto plazo (1-2 meses):**
   - Implementar multiples formas de pago
   - Agregar precios de costo
   - Mejorar alertas de stock

2. **Mediano plazo (3-6 meses):**
   - Sistema de cierre de caja
   - Descuentos basicos
   - Gestion de proveedores

3. **Largo plazo (6-12 meses):**
   - Cuentas corrientes
   - Facturacion electronica AFIP
   - Version web o movil

---

*Documento actualizado: Enero 2026*
*Sistema: Punto de Venta - Distribuidora LA FAMILIA*
