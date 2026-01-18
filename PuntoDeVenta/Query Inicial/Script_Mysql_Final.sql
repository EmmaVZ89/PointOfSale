-- =========================================================
-- SCRIPT DEFINITIVO MIGRACIÓN MYSQL
-- Versión: 2.0 - Incluye Clientes y Cancelaciones
-- =========================================================

CREATE DATABASE IF NOT EXISTS PuntoDeVenta;
USE PuntoDeVenta;

-- =========================================================
-- 1. TABLAS BASE
-- =========================================================

CREATE TABLE IF NOT EXISTS Privilegios (
    IdPrivilegio INT AUTO_INCREMENT PRIMARY KEY,
    NombrePrivilegio VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS Grupos (
    IdGrupo INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS Usuarios (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50),
    Apellido VARCHAR(50),
    DNI INT,
    CUIT DECIMAL(20,0),
    Correo VARCHAR(100),
    Telefono VARCHAR(20),
    Fecha_Nac DATE,
    Privilegio INT,
    img LONGBLOB,
    usuario VARCHAR(50),
    contrasenia VARBINARY(255),
    Activo BOOLEAN DEFAULT TRUE,
    Patron VARCHAR(50),
    FOREIGN KEY (Privilegio) REFERENCES Privilegios(IdPrivilegio)
);

CREATE TABLE IF NOT EXISTS Articulos (
    IdArticulo INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100),
    Grupo INT,
    Codigo VARCHAR(100),
    Precio DECIMAL(12,2),
    Activo BOOLEAN DEFAULT TRUE,
    Cantidad DECIMAL(12,2),
    UnidadMedida VARCHAR(10),
    Img LONGBLOB,
    Descripcion VARCHAR(256),
    FOREIGN KEY (Grupo) REFERENCES Grupos(IdGrupo)
);

-- =========================================================
-- 2. TABLA CLIENTES
-- =========================================================

CREATE TABLE IF NOT EXISTS Clientes (
    IdCliente INT AUTO_INCREMENT PRIMARY KEY,
    RazonSocial VARCHAR(150) NOT NULL,
    Documento VARCHAR(15),
    Telefono VARCHAR(20),
    Email VARCHAR(100),
    Activo BOOLEAN DEFAULT TRUE,
    FechaAlta DATETIME DEFAULT NOW()
);

-- =========================================================
-- 3. TABLAS DE INVENTARIO Y VENTAS
-- =========================================================

CREATE TABLE IF NOT EXISTS MovimientosInventario (
    IdMovimiento INT AUTO_INCREMENT PRIMARY KEY,
    IdArticulo INT,
    IdUsuario INT,
    TipoMovimiento VARCHAR(20),
    Cantidad DECIMAL(12,2),
    FechaMovimiento DATETIME DEFAULT NOW(),
    Observacion VARCHAR(255),
    FOREIGN KEY (IdArticulo) REFERENCES Articulos(IdArticulo),
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);

CREATE TABLE IF NOT EXISTS Ventas (
    Id_Venta INT AUTO_INCREMENT PRIMARY KEY,
    No_Factura VARCHAR(20),
    Fecha_Venta DATETIME DEFAULT NOW(),
    Monto_Total DECIMAL(12,2),
    Id_Usuario INT,
    Id_Cliente INT DEFAULT 1,
    Cancelada BOOLEAN DEFAULT FALSE,
    FechaCancelacion DATETIME,
    IdUsuarioCancelo INT,
    MotivoCancelacion VARCHAR(200),
    FOREIGN KEY (Id_Usuario) REFERENCES Usuarios(IdUsuario),
    FOREIGN KEY (Id_Cliente) REFERENCES Clientes(IdCliente),
    FOREIGN KEY (IdUsuarioCancelo) REFERENCES Usuarios(IdUsuario)
);

CREATE TABLE IF NOT EXISTS Ventas_Detalle (
    Id_Detalle INT AUTO_INCREMENT PRIMARY KEY,
    Id_Venta INT,
    Id_Articulo INT,
    Cantidad DECIMAL(12,2),
    Precio_Venta DECIMAL(12,2),
    Monto_Total DECIMAL(12,2),
    FOREIGN KEY (Id_Venta) REFERENCES Ventas(Id_Venta),
    FOREIGN KEY (Id_Articulo) REFERENCES Articulos(IdArticulo)
);

-- =========================================================
-- 4. DATOS INICIALES
-- =========================================================

INSERT INTO Privilegios (NombrePrivilegio) VALUES ('Administrador'), ('Vendedor');
INSERT INTO Grupos (Nombre) VALUES ('General'), ('Bebidas'), ('Alimentos'), ('Limpieza');

-- Cliente por defecto: Consumidor Final (ID = 1)
INSERT INTO Clientes (RazonSocial, Documento) VALUES ('Consumidor Final', '0');

-- Usuario Admin (User: admin / Pass: admin / Patrón: PuntoDeVenta)
INSERT INTO Usuarios (Nombre, Apellido, DNI, CUIT, Correo, Telefono, Fecha_Nac, usuario, contrasenia, Privilegio, Activo, Patron)
VALUES ('Admin', 'Sistema', 111111, 111111, 'admin@sistema.com', '111111', '2000-01-01', 'admin', AES_ENCRYPT('admin', 'PuntoDeVenta'), 1, TRUE, 'PuntoDeVenta');

-- =========================================================
-- 5. STORED PROCEDURES BASE
-- =========================================================

DELIMITER $$

-- Login
CREATE PROCEDURE sp_u_validar(IN p_usuario VARCHAR(50), IN p_contra VARCHAR(500), IN p_patron VARCHAR(50))
BEGIN
    SELECT IdUsuario, Privilegio, Activo
    FROM Usuarios
    WHERE usuario = p_usuario
      AND CAST(AES_DECRYPT(contrasenia, p_patron) AS CHAR) = p_contra;
END$$

-- Dashboard: Ventas Semanales (excluye canceladas)
CREATE PROCEDURE sp_d_ventassemanales()
BEGIN
    SELECT
        DATE(v.Fecha_Venta) as Fecha,
        SUM(v.Monto_Total) as Total
    FROM Ventas v
    WHERE v.Fecha_Venta >= DATE_SUB(CURDATE(), INTERVAL 7 DAY)
      AND (v.Cancelada = FALSE OR v.Cancelada IS NULL)
    GROUP BY DATE(v.Fecha_Venta)
    ORDER BY Fecha ASC;
END$$

-- POS: Procesar Detalle
CREATE PROCEDURE sp_c_venta_detalle(
    IN p_codigo VARCHAR(100),
    IN p_cantidad DECIMAL(12,2),
    IN p_factura VARCHAR(20),
    IN p_total DECIMAL(12,2)
)
BEGIN
    DECLARE v_id_articulo INT;
    DECLARE v_id_venta INT;
    DECLARE v_id_usuario INT;

    SELECT IdArticulo INTO v_id_articulo FROM Articulos WHERE Codigo = p_codigo LIMIT 1;
    SELECT Id_Venta, Id_Usuario INTO v_id_venta, v_id_usuario FROM Ventas WHERE No_Factura = p_factura LIMIT 1;

    IF v_id_articulo IS NOT NULL AND v_id_venta IS NOT NULL THEN
        INSERT INTO Ventas_Detalle (Id_Venta, Id_Articulo, Cantidad, Monto_Total)
        VALUES (v_id_venta, v_id_articulo, p_cantidad, p_total);

        UPDATE Articulos SET Cantidad = Cantidad - p_cantidad WHERE IdArticulo = v_id_articulo;

        INSERT INTO MovimientosInventario (IdArticulo, IdUsuario, TipoMovimiento, Cantidad, Observacion)
        VALUES (v_id_articulo, v_id_usuario, 'SALIDA', p_cantidad, CONCAT('Venta Factura: ', p_factura));
    END IF;
END$$

-- =========================================================
-- 6. STORED PROCEDURES PARA CANCELACIONES
-- =========================================================

-- Cancelar Venta
CREATE PROCEDURE sp_cancelar_venta(
    IN p_id_venta INT,
    IN p_id_usuario INT,
    IN p_motivo VARCHAR(200),
    IN p_fecha DATETIME
)
BEGIN
    DECLARE v_fecha DATETIME;
    DECLARE v_id_articulo INT;
    DECLARE v_cantidad DECIMAL(12,2);
    DECLARE done INT DEFAULT FALSE;

    DECLARE cur_detalle CURSOR FOR
        SELECT Id_Articulo, Cantidad FROM Ventas_Detalle WHERE Id_Venta = p_id_venta;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    SET v_fecha = IFNULL(p_fecha, NOW());

    -- Verificar que la venta existe y no está cancelada
    IF NOT EXISTS (SELECT 1 FROM Ventas WHERE Id_Venta = p_id_venta AND (Cancelada = FALSE OR Cancelada IS NULL)) THEN
        SELECT FALSE AS Resultado;
    ELSE
        -- Procesar devolución de stock
        OPEN cur_detalle;
        read_loop: LOOP
            FETCH cur_detalle INTO v_id_articulo, v_cantidad;
            IF done THEN
                LEAVE read_loop;
            END IF;

            -- Devolver stock
            UPDATE Articulos SET Cantidad = Cantidad + v_cantidad WHERE IdArticulo = v_id_articulo;

            -- Registrar movimiento
            INSERT INTO MovimientosInventario (IdArticulo, IdUsuario, TipoMovimiento, Cantidad, FechaMovimiento, Observacion)
            VALUES (v_id_articulo, p_id_usuario, 'ENTRADA', v_cantidad, v_fecha, CONCAT('Cancelación venta #', p_id_venta));
        END LOOP;
        CLOSE cur_detalle;

        -- Marcar venta como cancelada
        UPDATE Ventas
        SET Cancelada = TRUE,
            FechaCancelacion = v_fecha,
            IdUsuarioCancelo = p_id_usuario,
            MotivoCancelacion = p_motivo
        WHERE Id_Venta = p_id_venta;

        SELECT TRUE AS Resultado;
    END IF;
END$$

-- Buscar Ventas para Cancelar
CREATE PROCEDURE sp_buscar_ventas_cancelables(IN p_buscar VARCHAR(100))
BEGIN
    SELECT
        v.Id_Venta,
        v.No_Factura,
        v.Fecha_Venta,
        v.Monto_Total,
        u.usuario AS Vendedor,
        IFNULL(c.RazonSocial, 'Consumidor Final') AS Cliente
    FROM Ventas v
    INNER JOIN Usuarios u ON v.Id_Usuario = u.IdUsuario
    LEFT JOIN Clientes c ON v.Id_Cliente = c.IdCliente
    WHERE (v.Cancelada = FALSE OR v.Cancelada IS NULL)
      AND (v.No_Factura LIKE CONCAT('%', p_buscar, '%')
           OR u.usuario LIKE CONCAT('%', p_buscar, '%')
           OR IFNULL(c.RazonSocial, '') LIKE CONCAT('%', p_buscar, '%'))
    ORDER BY v.Fecha_Venta DESC
    LIMIT 100;
END$$

-- Obtener Detalle de Venta
CREATE PROCEDURE sp_detalle_venta(IN p_id_venta INT)
BEGIN
    SELECT
        a.Codigo,
        a.Nombre AS Producto,
        vd.Cantidad,
        IFNULL(vd.Precio_Venta, a.Precio) AS PrecioUnit,
        vd.Monto_Total AS Subtotal
    FROM Ventas_Detalle vd
    INNER JOIN Articulos a ON vd.Id_Articulo = a.IdArticulo
    WHERE vd.Id_Venta = p_id_venta;
END$$

DELIMITER ;

-- =========================================================
-- FIN DEL SCRIPT
-- =========================================================
