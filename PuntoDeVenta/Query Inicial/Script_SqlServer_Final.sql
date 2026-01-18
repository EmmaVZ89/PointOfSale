-- =========================================================
-- SCRIPT DEFINITIVO MIGRACIÓN SQL SERVER
-- Versión: 2.0 - Incluye Clientes y Cancelaciones
-- =========================================================

CREATE DATABASE PuntoDeVenta;
GO
USE PuntoDeVenta;
GO

-- =========================================================
-- 1. TABLAS BASE
-- =========================================================

CREATE TABLE Privilegios (
    IdPrivilegio INT PRIMARY KEY IDENTITY(1,1),
    NombrePrivilegio VARCHAR(50)
);

CREATE TABLE Grupos (
    IdGrupo INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50)
);

CREATE TABLE Usuarios (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50),
    Apellido VARCHAR(50),
    DNI INT,
    CUIT NUMERIC(18,0),
    Correo VARCHAR(100),
    Telefono VARCHAR(20),
    Fecha_Nac DATE,
    Privilegio INT REFERENCES Privilegios(IdPrivilegio),
    img VARBINARY(MAX),
    usuario VARCHAR(50),
    contrasenia VARBINARY(MAX),
    Activo BIT DEFAULT 1,
    Patron VARCHAR(50)
);

CREATE TABLE Articulos (
    IdArticulo INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100),
    Grupo INT REFERENCES Grupos(IdGrupo),
    Codigo VARCHAR(100),
    Precio DECIMAL(12,2),
    Activo BIT DEFAULT 1,
    Cantidad DECIMAL(12,2),
    UnidadMedida VARCHAR(10),
    Img VARBINARY(MAX),
    Descripcion VARCHAR(256)
);

-- =========================================================
-- 2. TABLA CLIENTES
-- =========================================================

CREATE TABLE Clientes (
    IdCliente INT PRIMARY KEY IDENTITY(1,1),
    RazonSocial VARCHAR(150) NOT NULL,
    Documento VARCHAR(15),
    Telefono VARCHAR(20),
    Email VARCHAR(100),
    Activo BIT DEFAULT 1,
    FechaAlta DATETIME DEFAULT GETDATE()
);
GO

-- =========================================================
-- 3. TABLAS DE INVENTARIO Y VENTAS
-- =========================================================

CREATE TABLE MovimientosInventario (
    IdMovimiento INT PRIMARY KEY IDENTITY(1,1),
    IdArticulo INT REFERENCES Articulos(IdArticulo),
    IdUsuario INT REFERENCES Usuarios(IdUsuario),
    TipoMovimiento VARCHAR(20),
    Cantidad DECIMAL(12,2),
    FechaMovimiento DATETIME DEFAULT GETDATE(),
    Observacion VARCHAR(255)
);

CREATE TABLE Ventas (
    Id_Venta INT PRIMARY KEY IDENTITY(1,1),
    No_Factura VARCHAR(20),
    Fecha_Venta DATETIME DEFAULT GETDATE(),
    Monto_Total DECIMAL(12,2),
    Id_Usuario INT REFERENCES Usuarios(IdUsuario),
    Id_Cliente INT REFERENCES Clientes(IdCliente) DEFAULT 1,
    Cancelada BIT DEFAULT 0,
    FechaCancelacion DATETIME,
    IdUsuarioCancelo INT REFERENCES Usuarios(IdUsuario),
    MotivoCancelacion VARCHAR(200)
);

CREATE TABLE Ventas_Detalle (
    Id_Detalle INT PRIMARY KEY IDENTITY(1,1),
    Id_Venta INT REFERENCES Ventas(Id_Venta),
    Id_Articulo INT REFERENCES Articulos(IdArticulo),
    Cantidad DECIMAL(12,2),
    Precio_Venta DECIMAL(12,2),
    Monto_Total DECIMAL(12,2)
);
GO

-- =========================================================
-- 4. DATOS INICIALES
-- =========================================================

INSERT INTO Privilegios (NombrePrivilegio) VALUES ('Administrador'), ('Vendedor');
INSERT INTO Grupos (Nombre) VALUES ('General'), ('Bebidas'), ('Alimentos'), ('Limpieza');

-- Cliente por defecto: Consumidor Final (ID = 1)
INSERT INTO Clientes (RazonSocial, Documento) VALUES ('Consumidor Final', '0');

-- Usuario Admin (User: admin / Pass: admin / Patrón: PuntoDeVenta)
INSERT INTO Usuarios (Nombre, Apellido, DNI, CUIT, Correo, Telefono, Fecha_Nac, usuario, contrasenia, Privilegio, Activo, Patron)
VALUES ('Admin', 'Sistema', 111111, 111111, 'admin@sistema.com', '111111', '2000-01-01', 'admin', ENCRYPTBYPASSPHRASE('PuntoDeVenta', 'admin'), 1, 1, 'PuntoDeVenta');
GO

-- =========================================================
-- 5. STORED PROCEDURES BASE
-- =========================================================

-- Login
CREATE PROCEDURE sp_u_validar
    @Usuario VARCHAR(50),
    @Contra VARCHAR(500),
    @Patron VARCHAR(50)
AS BEGIN
    SELECT IdUsuario, Privilegio, Activo
    FROM Usuarios
    WHERE usuario = @Usuario
    AND CONVERT(VARCHAR(MAX), DECRYPTBYPASSPHRASE(@Patron, contrasenia)) = @Contra
END
GO

-- Dashboard: Ventas Semanales (excluye canceladas)
CREATE PROCEDURE sp_d_ventassemanales
AS BEGIN
    ;WITH Dates AS (
        SELECT CAST(GETDATE() AS DATE) AS Fecha
        UNION ALL
        SELECT DATEADD(DAY, -1, Fecha)
        FROM Dates
        WHERE Fecha > DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
    )
    SELECT
        d.Fecha,
        ISNULL(SUM(v.Monto_Total), 0) as Total
    FROM Dates d
    LEFT JOIN Ventas v ON CAST(v.Fecha_Venta AS DATE) = d.Fecha
        AND (v.Cancelada = 0 OR v.Cancelada IS NULL)
    GROUP BY d.Fecha
    ORDER BY d.Fecha ASC
END
GO

-- POS: Procesar Detalle
CREATE PROCEDURE sp_c_venta_detalle
    @Codigo VARCHAR(100),
    @Cantidad DECIMAL(12,2),
    @Factura VARCHAR(20),
    @Total DECIMAL(12,2)
AS BEGIN
    DECLARE @IdArticulo INT
    DECLARE @IdVenta INT
    DECLARE @IdUsuario INT

    SELECT TOP 1 @IdArticulo = IdArticulo FROM Articulos WHERE Codigo = @Codigo
    SELECT TOP 1 @IdVenta = Id_Venta, @IdUsuario = Id_Usuario FROM Ventas WHERE No_Factura = @Factura

    IF @IdArticulo IS NOT NULL AND @IdVenta IS NOT NULL
    BEGIN
        INSERT INTO Ventas_Detalle (Id_Venta, Id_Articulo, Cantidad, Monto_Total)
        VALUES (@IdVenta, @IdArticulo, @Cantidad, @Total)

        UPDATE Articulos SET Cantidad = Cantidad - @Cantidad WHERE IdArticulo = @IdArticulo

        INSERT INTO MovimientosInventario (IdArticulo, IdUsuario, TipoMovimiento, Cantidad, Observacion)
        VALUES (@IdArticulo, @IdUsuario, 'SALIDA', @Cantidad, 'Venta Factura: ' + @Factura)
    END
END
GO

-- =========================================================
-- 6. STORED PROCEDURES PARA CANCELACIONES
-- =========================================================

-- Cancelar Venta
CREATE PROCEDURE sp_cancelar_venta
    @IdVenta INT,
    @IdUsuario INT,
    @Motivo VARCHAR(200),
    @Fecha DATETIME = NULL
AS BEGIN
    DECLARE @FechaReal DATETIME = ISNULL(@Fecha, GETDATE())
    DECLARE @IdArticulo INT
    DECLARE @CantidadDet DECIMAL(12,2)

    -- Verificar que la venta existe y no está cancelada
    IF NOT EXISTS (SELECT 1 FROM Ventas WHERE Id_Venta = @IdVenta AND (Cancelada = 0 OR Cancelada IS NULL))
    BEGIN
        SELECT CAST(0 AS BIT) AS Resultado
        RETURN
    END

    -- Cursor para devolver stock
    DECLARE detalle_cursor CURSOR FOR
        SELECT Id_Articulo, Cantidad FROM Ventas_Detalle WHERE Id_Venta = @IdVenta

    OPEN detalle_cursor
    FETCH NEXT FROM detalle_cursor INTO @IdArticulo, @CantidadDet

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Devolver stock
        UPDATE Articulos SET Cantidad = Cantidad + @CantidadDet WHERE IdArticulo = @IdArticulo

        -- Registrar movimiento
        INSERT INTO MovimientosInventario (IdArticulo, IdUsuario, TipoMovimiento, Cantidad, FechaMovimiento, Observacion)
        VALUES (@IdArticulo, @IdUsuario, 'ENTRADA', @CantidadDet, @FechaReal, 'Cancelación venta #' + CAST(@IdVenta AS VARCHAR))

        FETCH NEXT FROM detalle_cursor INTO @IdArticulo, @CantidadDet
    END

    CLOSE detalle_cursor
    DEALLOCATE detalle_cursor

    -- Marcar venta como cancelada
    UPDATE Ventas
    SET Cancelada = 1,
        FechaCancelacion = @FechaReal,
        IdUsuarioCancelo = @IdUsuario,
        MotivoCancelacion = @Motivo
    WHERE Id_Venta = @IdVenta

    SELECT CAST(1 AS BIT) AS Resultado
END
GO

-- Buscar Ventas para Cancelar
CREATE PROCEDURE sp_buscar_ventas_cancelables
    @Buscar VARCHAR(100)
AS BEGIN
    SELECT TOP 100
        v.Id_Venta,
        v.No_Factura,
        v.Fecha_Venta,
        v.Monto_Total,
        u.usuario AS Vendedor,
        ISNULL(c.RazonSocial, 'Consumidor Final') AS Cliente
    FROM Ventas v
    INNER JOIN Usuarios u ON v.Id_Usuario = u.IdUsuario
    LEFT JOIN Clientes c ON v.Id_Cliente = c.IdCliente
    WHERE (v.Cancelada = 0 OR v.Cancelada IS NULL)
      AND (v.No_Factura LIKE '%' + @Buscar + '%'
           OR u.usuario LIKE '%' + @Buscar + '%'
           OR ISNULL(c.RazonSocial, '') LIKE '%' + @Buscar + '%')
    ORDER BY v.Fecha_Venta DESC
END
GO

-- Obtener Detalle de Venta
CREATE PROCEDURE sp_detalle_venta
    @IdVenta INT
AS BEGIN
    SELECT
        a.Codigo,
        a.Nombre AS Producto,
        vd.Cantidad,
        ISNULL(vd.Precio_Venta, a.Precio) AS PrecioUnit,
        vd.Monto_Total AS Subtotal
    FROM Ventas_Detalle vd
    INNER JOIN Articulos a ON vd.Id_Articulo = a.IdArticulo
    WHERE vd.Id_Venta = @IdVenta
END
GO

-- =========================================================
-- FIN DEL SCRIPT
-- =========================================================
