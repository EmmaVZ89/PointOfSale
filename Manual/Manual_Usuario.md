# Manual de Usuario - Sistema Punto de Venta
## Distribuidora de Bebidas LA FAMILIA

**Version:** 3.0
**Fecha:** Enero 2026

---

## Contenido

1. [Inicio de Sesion](#1-inicio-de-sesion)
2. [Panel Principal (Dashboard)](#2-panel-principal)
3. [Punto de Venta (POS)](#3-punto-de-venta)
4. [Productos](#4-productos)
5. [Clientes](#5-clientes)
6. [Inventario](#6-inventario)
7. [Reportes](#7-reportes)
8. [Cancelaciones](#8-cancelaciones)
9. [Usuarios](#9-usuarios)
10. [Personalizacion](#10-personalizacion)

---

## 1. Inicio de Sesion

Al abrir la aplicacion se muestra la pantalla de login.

**Credenciales por defecto:**
- Usuario: `admin`
- Contraseña: `admin`

Ingrese sus credenciales y presione **Ingresar**.

> **Nota:** Las contraseñas estan encriptadas en la base de datos para mayor seguridad.

---

## 2. Panel Principal

El Dashboard muestra un resumen del dia actual:

### Indicadores principales

| Indicador | Descripcion |
|-----------|-------------|
| Ventas del dia | Total facturado en el dia de hoy |
| Cantidad de ventas | Numero de operaciones realizadas |
| Productos activos | Total de productos en catalogo |
| Stock bajo | Productos con menos de 10 unidades |

### Grafico de tendencia semanal

Muestra las ventas de los ultimos 7 dias en un grafico de barras para visualizar la tendencia del negocio.

### Top 5 productos

Lista de los 5 productos mas vendidos del dia, ideal para identificar los articulos de mayor rotacion.

### Ultimos movimientos de inventario

Muestra los movimientos de stock mas recientes (entradas y salidas).

---

## 3. Punto de Venta

### Realizar una venta

1. **Seleccionar cliente** (opcional)
   - Use el desplegable superior para seleccionar el cliente
   - Por defecto esta seleccionado "Consumidor Final"
   - Puede buscar clientes escribiendo en el desplegable

2. **Agregar productos**
   - Seleccione el producto del desplegable o escriba para buscar
   - El sistema muestra automaticamente el precio y stock disponible
   - Ingrese la cantidad deseada
   - Presione **Agregar** o la tecla Enter

3. **Revisar carrito**
   - Los productos agregados aparecen en la grilla central
   - Se muestra codigo, descripcion, cantidad, precio unitario y subtotal
   - El total se actualiza automaticamente

4. **Eliminar producto del carrito**
   - Use el boton **X** en la fila del producto que desea quitar

5. **Cobrar la venta**
   - Ingrese el monto que paga el cliente en el campo "Paga con"
   - El sistema calcula el vuelto automaticamente
   - Presione el boton **Cobrar**

6. **Opciones de presupuesto**
   - Al cobrar aparece un dialogo con tres opciones:
     - **Imprimir**: Abre el PDF en el visor predeterminado. Presione Ctrl+P para imprimir.
     - **Guardar**: Permite elegir ubicacion y nombre del archivo PDF
     - **Cancelar**: Cierra el dialogo sin accion

7. **Formato del presupuesto PDF**
   - Logo y datos del local (Distribuidora de Bebidas LA FAMILIA)
   - Numero de presupuesto y fecha
   - Datos del cliente seleccionado
   - Listado de productos con codigo, descripcion, cantidad, precio unitario e importe
   - Total en numeros y letras
   - Formato A4 profesional

8. **Anular venta antes de cobrar**
   - El boton **Anular** vacia el carrito completamente
   - Use esta opcion si desea empezar de nuevo

---

## 4. Productos

*(Solo administradores)*

### Ver productos

La pantalla muestra todos los productos con:
- Codigo
- Nombre
- Categoria
- Precio
- Stock actual
- Estado (Activo/Inactivo)

### Buscar productos

Use el campo de busqueda para filtrar por codigo o nombre.

### Crear producto

1. Presione **Nuevo Producto**
2. Complete los campos obligatorios (*):
   - Nombre del producto
   - Codigo (unico)
   - Precio de venta
   - Categoria
   - Stock inicial
   - Unidad de medida (Unidad, Kg, Litro, etc.)
3. Opcionalmente agregue una imagen y descripcion
4. Presione **Crear**

### Modificar producto

1. Presione el icono de edicion en la fila del producto
2. Modifique los datos necesarios
3. Presione **Modificar**

### Dar de baja producto

1. Presione el icono de eliminar
2. Confirme la accion
3. El producto pasa a estado "Inactivo" (baja logica)
4. No aparecera en el POS pero se mantiene en reportes historicos

### Reactivar producto

Para productos inactivos, aparece el boton **Reactivar** para volver a habilitarlo.

---

## 5. Clientes

*(Solo administradores)*

### Ver clientes

Lista de clientes con:
- Razon Social
- Documento (DNI/CUIT)
- Telefono
- Email
- Estado

### Buscar clientes

Use el campo de busqueda para filtrar por nombre o documento.

### Crear cliente

1. Presione **Nuevo Cliente**
2. Complete los campos:
   - **Razon Social** (*): Nombre o empresa (obligatorio)
   - **Documento**: DNI o CUIT
   - **Telefono**: Numero de contacto
   - **Email**: Correo electronico
3. Presione **Crear**

### Modificar cliente

1. Presione el icono de edicion
2. Modifique los datos necesarios
3. Presione **Modificar**

> **Nota:** El cliente "Consumidor Final" no puede modificarse ni eliminarse.

### Dar de baja / Reactivar

Funciona igual que productos (baja logica). Los clientes dados de baja no apareceran en el selector del POS.

---

## 6. Inventario

*(Solo administradores)*

### Ver movimientos

Muestra el historial completo de entradas y salidas de stock con:
- Fecha y hora
- Producto
- Tipo (ENTRADA/SALIDA)
- Cantidad
- Usuario que realizo el movimiento
- Observacion

### Filtrar movimientos

- **Fecha desde/hasta**: Filtre por rango de fechas
- **Busqueda**: Busque por nombre de producto u observacion

### Registrar movimiento manual

1. Presione **Nuevo Movimiento**
2. Complete:
   - **Producto**: Seleccione del listado
   - **Tipo**: ENTRADA (compra, devolucion) o SALIDA (perdida, ajuste)
   - **Cantidad**: Unidades a mover
   - **Observacion**: Motivo del movimiento (obligatorio)
3. Presione **Registrar**

El stock del producto se actualiza automaticamente segun el tipo de movimiento.

---

## 7. Reportes

*(Solo administradores)*

### Tipos de reportes disponibles

#### Ventas Detalladas
Listado de cada venta realizada con:
- Numero de factura
- Fecha
- Vendedor
- Cliente
- Productos vendidos
- Cantidades y precios
- Subtotales

#### Stock Valorizado
Estado actual del inventario:
- Codigo y nombre del producto
- Categoria
- Stock actual
- Precio unitario
- Valor total del stock

#### Productos Top
Los 10 productos mas vendidos en el periodo seleccionado, ideal para analisis de rotacion.

#### Reimprimir Ticket
Permite buscar una venta realizada y generar nuevamente el presupuesto en PDF.

### Generar reporte

1. Seleccione el tipo de reporte (tabs superiores)
2. Configure filtros:
   - **Fecha desde/hasta**: Para reportes de Ventas, Top y Reimprimir
   - **Cliente**: Solo para Ventas y Reimprimir (opcional)
3. Presione **Generar Reporte**

### Exportar reporte

- **Exportar CSV**: Archivo separado por comas, compatible con Excel
- **Exportar PDF**: Documento listo para impresion

### Reimprimir presupuesto

1. Seleccione la pestaña **Reimprimir Ticket**
2. Configure el rango de fechas
3. Opcionalmente filtre por cliente
4. Presione **Generar Reporte**
5. Seleccione la venta deseada en la grilla
6. Presione **Reimprimir Ticket**
7. Elija donde guardar el archivo PDF

El presupuesto se genera con el mismo formato profesional que al realizar la venta original.

---

## 8. Cancelaciones

*(Solo administradores)*

Permite anular ventas ya realizadas y devolver el stock automaticamente.

### Buscar venta a cancelar

1. Use el campo de busqueda para encontrar la venta:
   - Por numero de factura
   - Por nombre de vendedor
   - Por nombre de cliente
2. Los resultados muestran solo ventas no canceladas

### Cancelar una venta

1. Seleccione la venta en la grilla
2. Se muestra el detalle de productos de esa venta
3. Ingrese el motivo de cancelacion (obligatorio)
4. Presione **Cancelar Venta**
5. Confirme la operacion

### Resultado de la cancelacion

Al cancelar una venta:
- El stock de cada producto se devuelve automaticamente
- Se registran movimientos de ENTRADA en el inventario
- La venta se marca como "Cancelada" (no se elimina)
- La venta ya no aparece en reportes de ventas
- La operacion no puede deshacerse

> **Advertencia:** La cancelacion de ventas es una operacion sensible. Solo debe utilizarse en casos justificados.

---

## 9. Usuarios

*(Solo administradores)*

### Ver usuarios

Lista de usuarios del sistema con:
- Nombre completo
- Usuario de acceso
- Privilegio
- Estado

### Crear usuario

1. Presione **Nuevo Usuario**
2. Complete todos los datos personales
3. Asigne un nombre de usuario y contraseña
4. Seleccione el privilegio:
   - **Administrador**: Acceso completo a todas las funciones
   - **Vendedor**: Solo acceso a POS y Dashboard
5. Presione **Crear**

### Modificar usuario

1. Presione el icono de edicion
2. Modifique los datos necesarios
3. Presione **Modificar**

### Dar de baja / Reactivar

Similar a otras entidades. Los usuarios dados de baja no podran iniciar sesion.

---

## 10. Personalizacion

### Selector de tema

El sistema incluye tres temas visuales. El selector se encuentra en el header (parte superior derecha):

| Tema | Color principal | Descripcion |
|------|-----------------|-------------|
| Green | Verde (#27AE60) | Tema predeterminado, profesional |
| Dark | Gris oscuro (#2C3E50) | Modo oscuro, reduce fatiga visual |
| Red | Rojo (#E74C3C) | Tema alternativo |

Para cambiar el tema:
1. Ubique los circulos de colores en el header
2. Haga clic en el color deseado
3. El tema se aplica inmediatamente
4. La preferencia se guarda para la proxima sesion

### Menu lateral

- Presione el icono de hamburguesa (tres lineas) para expandir/contraer el menu
- El menu contraido muestra solo iconos
- El menu expandido muestra iconos y texto

---

## Accesos segun privilegio

| Funcion | Administrador | Vendedor |
|---------|:-------------:|:--------:|
| Dashboard | Si | Si |
| Punto de Venta | Si | Si |
| Productos | Si | No |
| Clientes | Si | No |
| Inventario | Si | No |
| Reportes | Si | No |
| Cancelaciones | Si | No |
| Usuarios | Si | No |

---

## Atajos de teclado

| Atajo | Funcion |
|-------|---------|
| Enter | Agregar producto al carrito (en POS) |
| Ctrl+P | Imprimir (desde visor PDF) |

---

## Preguntas frecuentes

### El sistema muestra hora incorrecta
El sistema esta configurado para la zona horaria de Argentina (UTC-3). Si la hora no coincide, contacte al administrador de la base de datos.

### No puedo eliminar un producto/cliente
Los productos y clientes se dan de baja logicamente, no se eliminan. Esto preserva la integridad de los reportes historicos.

### El PDF no muestra el logo
Asegurese de que el archivo de logo este incluido en los recursos de la aplicacion. Contacte al desarrollador si el problema persiste.

### Un vendedor no puede acceder a ciertas funciones
Las funciones de administracion (Productos, Clientes, Inventario, Reportes, Cancelaciones, Usuarios) solo estan disponibles para usuarios con privilegio de Administrador.

---

## Soporte

Para reportar problemas o sugerencias, contacte al administrador del sistema.

**Sistema desarrollado para:** Distribuidora de Bebidas LA FAMILIA
**Tecnologias:** .NET Framework 4.7.2, WPF, PostgreSQL
