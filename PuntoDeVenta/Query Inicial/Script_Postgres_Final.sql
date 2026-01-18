-- =============================================================================
-- SCRIPT DEFINITIVO POSTGRESQL (Neon/Supabase/Local)
-- Punto de Venta - Distribuidora LA FAMILIA
-- Version: 3.0 - Incluye Clientes, Cancelaciones y Zona Horaria Argentina
-- Fecha: 2026-01-16
-- =============================================================================

-- =============================================================================
-- 1. EXTENSIONES
-- =============================================================================
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- =============================================================================
-- 2. FUNCIONES DE ZONA HORARIA (Argentina UTC-3)
-- =============================================================================

-- Obtener timestamp actual en hora Argentina
CREATE OR REPLACE FUNCTION ahora_argentina()
RETURNS TIMESTAMP AS $$
BEGIN
    RETURN (NOW() AT TIME ZONE 'America/Argentina/Buenos_Aires');
END;
$$ LANGUAGE plpgsql;

-- Obtener fecha actual en Argentina
CREATE OR REPLACE FUNCTION fecha_argentina()
RETURNS DATE AS $$
BEGIN
    RETURN (CURRENT_TIMESTAMP AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- 3. TABLAS BASE
-- =============================================================================

CREATE TABLE IF NOT EXISTS "Privilegios" (
    "IdPrivilegio" SERIAL PRIMARY KEY,
    "NombrePrivilegio" VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS "Grupos" (
    "IdGrupo" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS "Usuarios" (
    "IdUsuario" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(50),
    "Apellido" VARCHAR(50),
    "DNI" INT,
    "CUIT" NUMERIC,
    "Correo" VARCHAR(100),
    "Telefono" VARCHAR(20),
    "Fecha_Nac" DATE,
    "Privilegio" INT REFERENCES "Privilegios"("IdPrivilegio"),
    "img" BYTEA,
    "usuario" VARCHAR(50),
    "contrasenia" BYTEA,
    "Activo" BOOLEAN DEFAULT TRUE,
    "Patron" VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS "Articulos" (
    "IdArticulo" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100),
    "Grupo" INT REFERENCES "Grupos"("IdGrupo"),
    "Codigo" VARCHAR(100),
    "Precio" NUMERIC(12,2),
    "Activo" BOOLEAN DEFAULT TRUE,
    "Cantidad" NUMERIC(12,2),
    "UnidadMedida" VARCHAR(10),
    "Img" BYTEA,
    "Descripcion" VARCHAR(256)
);

-- =============================================================================
-- 4. TABLA CLIENTES
-- =============================================================================

CREATE TABLE IF NOT EXISTS "Clientes" (
    "IdCliente" SERIAL PRIMARY KEY,
    "RazonSocial" VARCHAR(150) NOT NULL,
    "Documento" VARCHAR(15),
    "Telefono" VARCHAR(20),
    "Email" VARCHAR(100),
    "Activo" BOOLEAN DEFAULT TRUE,
    "FechaAlta" TIMESTAMP DEFAULT (NOW() AT TIME ZONE 'America/Argentina/Buenos_Aires')
);

-- =============================================================================
-- 5. TABLAS DE INVENTARIO Y VENTAS
-- =============================================================================

CREATE TABLE IF NOT EXISTS "MovimientosInventario" (
    "IdMovimiento" SERIAL PRIMARY KEY,
    "IdArticulo" INT REFERENCES "Articulos"("IdArticulo"),
    "IdUsuario" INT REFERENCES "Usuarios"("IdUsuario"),
    "TipoMovimiento" VARCHAR(20),
    "Cantidad" NUMERIC(12,2),
    "FechaMovimiento" TIMESTAMP DEFAULT (NOW() AT TIME ZONE 'America/Argentina/Buenos_Aires'),
    "Observacion" VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS "Ventas" (
    "Id_Venta" SERIAL PRIMARY KEY,
    "No_Factura" VARCHAR(20),
    "Fecha_Venta" TIMESTAMP DEFAULT (NOW() AT TIME ZONE 'America/Argentina/Buenos_Aires'),
    "Monto_Total" NUMERIC(12,2),
    "Id_Usuario" INT REFERENCES "Usuarios"("IdUsuario"),
    "Id_Cliente" INT REFERENCES "Clientes"("IdCliente") DEFAULT 1,
    "Cancelada" BOOLEAN DEFAULT FALSE,
    "FechaCancelacion" TIMESTAMP,
    "IdUsuarioCancelo" INT REFERENCES "Usuarios"("IdUsuario"),
    "MotivoCancelacion" VARCHAR(200)
);

CREATE TABLE IF NOT EXISTS "Ventas_Detalle" (
    "Id_Detalle" SERIAL PRIMARY KEY,
    "Id_Venta" INT REFERENCES "Ventas"("Id_Venta"),
    "Id_Articulo" INT REFERENCES "Articulos"("IdArticulo"),
    "Cantidad" NUMERIC(12,2),
    "Precio_Venta" NUMERIC(12,2),
    "Monto_Total" NUMERIC(12,2)
);

-- =============================================================================
-- 6. DATOS INICIALES
-- =============================================================================

-- Privilegios
INSERT INTO "Privilegios" ("NombrePrivilegio")
SELECT 'Administrador' WHERE NOT EXISTS (SELECT 1 FROM "Privilegios" WHERE "NombrePrivilegio" = 'Administrador');
INSERT INTO "Privilegios" ("NombrePrivilegio")
SELECT 'Vendedor' WHERE NOT EXISTS (SELECT 1 FROM "Privilegios" WHERE "NombrePrivilegio" = 'Vendedor');

-- Grupos de productos
INSERT INTO "Grupos" ("Nombre") SELECT 'General' WHERE NOT EXISTS (SELECT 1 FROM "Grupos" WHERE "Nombre" = 'General');
INSERT INTO "Grupos" ("Nombre") SELECT 'Bebidas' WHERE NOT EXISTS (SELECT 1 FROM "Grupos" WHERE "Nombre" = 'Bebidas');
INSERT INTO "Grupos" ("Nombre") SELECT 'Alimentos' WHERE NOT EXISTS (SELECT 1 FROM "Grupos" WHERE "Nombre" = 'Alimentos');
INSERT INTO "Grupos" ("Nombre") SELECT 'Limpieza' WHERE NOT EXISTS (SELECT 1 FROM "Grupos" WHERE "Nombre" = 'Limpieza');

-- Cliente por defecto: Consumidor Final (ID = 1)
INSERT INTO "Clientes" ("RazonSocial", "Documento")
SELECT 'Consumidor Final', '0'
WHERE NOT EXISTS (SELECT 1 FROM "Clientes" WHERE "IdCliente" = 1);

-- Usuario Admin (User: admin / Pass: admin / Patron: PuntoDeVenta)
INSERT INTO "Usuarios" ("Nombre", "Apellido", "DNI", "CUIT", "Correo", "Telefono", "Fecha_Nac", "usuario", "contrasenia", "Privilegio", "Activo", "Patron")
SELECT 'Admin', 'Sistema', 111111, 111111, 'admin@sistema.com', '111111', '2000-01-01', 'admin', pgp_sym_encrypt('admin', 'PuntoDeVenta'), 1, TRUE, 'PuntoDeVenta'
WHERE NOT EXISTS (SELECT 1 FROM "Usuarios" WHERE "usuario" = 'admin');

-- =============================================================================
-- 7. STORED PROCEDURES Y FUNCIONES
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 7.1 LOGIN
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION sp_u_validar(p_usuario VARCHAR, p_contra VARCHAR, p_patron VARCHAR)
RETURNS TABLE("IdUsuario" INT, "Privilegio" INT, "Activo" BOOLEAN) AS $$
BEGIN
    RETURN QUERY
    SELECT u."IdUsuario", u."Privilegio", u."Activo"
    FROM "Usuarios" u
    WHERE u."usuario" = p_usuario
      AND pgp_sym_decrypt(u."contrasenia", p_patron) = p_contra;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------------------------------
-- 7.2 DASHBOARD: Ventas Semanales (excluye canceladas, usa hora Argentina)
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION sp_d_ventassemanales()
RETURNS TABLE("Fecha" DATE, "Total" NUMERIC) AS $$
DECLARE
    fecha_hoy DATE := fecha_argentina();
BEGIN
    RETURN QUERY
    SELECT f.dt::DATE, COALESCE(SUM(v."Monto_Total"), 0)
    FROM generate_series(fecha_hoy - INTERVAL '6 days', fecha_hoy, '1 day') AS f(dt)
    LEFT JOIN "Ventas" v
        ON (v."Fecha_Venta" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE = f.dt::DATE
        AND (v."Cancelada" = FALSE OR v."Cancelada" IS NULL)
    GROUP BY f.dt::DATE
    ORDER BY f.dt::DATE;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------------------------------
-- 7.3 POS: Procesar Detalle Venta y Stock (usa hora Argentina)
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION sp_c_venta_detalle(
    p_codigo VARCHAR,
    p_cantidad NUMERIC,
    p_factura VARCHAR,
    p_total_articulo NUMERIC
)
RETURNS VOID AS $$
DECLARE
    v_id_articulo INT;
    v_id_venta INT;
    v_id_usuario INT;
    v_fecha_argentina TIMESTAMP := ahora_argentina();
BEGIN
    -- Obtener IDs
    SELECT "IdArticulo" INTO v_id_articulo FROM "Articulos" WHERE "Codigo" = p_codigo LIMIT 1;
    SELECT "Id_Venta", "Id_Usuario" INTO v_id_venta, v_id_usuario FROM "Ventas" WHERE "No_Factura" = p_factura LIMIT 1;

    IF v_id_articulo IS NOT NULL AND v_id_venta IS NOT NULL THEN
        -- Insertar Detalle
        INSERT INTO "Ventas_Detalle" ("Id_Venta", "Id_Articulo", "Cantidad", "Monto_Total")
        VALUES (v_id_venta, v_id_articulo, p_cantidad, p_total_articulo);

        -- Descontar Stock
        UPDATE "Articulos" SET "Cantidad" = "Cantidad" - p_cantidad WHERE "IdArticulo" = v_id_articulo;

        -- Movimiento Salida con fecha Argentina
        INSERT INTO "MovimientosInventario" ("IdArticulo", "IdUsuario", "TipoMovimiento", "Cantidad", "FechaMovimiento", "Observacion")
        VALUES (v_id_articulo, v_id_usuario, 'SALIDA', p_cantidad, v_fecha_argentina, 'Venta Factura: ' || p_factura);
    END IF;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------------------------------
-- 7.4 CANCELACIONES: Cancelar Venta (devuelve stock, usa hora Argentina)
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION sp_cancelar_venta(
    p_id_venta INT,
    p_id_usuario INT,
    p_motivo VARCHAR,
    p_fecha TIMESTAMP DEFAULT NULL
) RETURNS BOOLEAN AS $$
DECLARE
    v_detalle RECORD;
    v_fecha TIMESTAMP;
BEGIN
    -- Usar hora Argentina si no se especifica fecha
    v_fecha := COALESCE(p_fecha, ahora_argentina());

    -- Verificar que la venta existe y no esta cancelada
    IF NOT EXISTS (SELECT 1 FROM "Ventas" WHERE "Id_Venta" = p_id_venta AND ("Cancelada" = FALSE OR "Cancelada" IS NULL)) THEN
        RETURN FALSE;
    END IF;

    -- Devolver stock de cada producto
    FOR v_detalle IN
        SELECT "Id_Articulo", "Cantidad" FROM "Ventas_Detalle" WHERE "Id_Venta" = p_id_venta
    LOOP
        UPDATE "Articulos"
        SET "Cantidad" = "Cantidad" + v_detalle."Cantidad"
        WHERE "IdArticulo" = v_detalle."Id_Articulo";

        INSERT INTO "MovimientosInventario"
        ("IdArticulo", "IdUsuario", "TipoMovimiento", "Cantidad", "FechaMovimiento", "Observacion")
        VALUES (v_detalle."Id_Articulo", p_id_usuario, 'ENTRADA', v_detalle."Cantidad", v_fecha,
                'Cancelacion venta #' || p_id_venta);
    END LOOP;

    -- Marcar venta como cancelada
    UPDATE "Ventas"
    SET "Cancelada" = TRUE,
        "FechaCancelacion" = v_fecha,
        "IdUsuarioCancelo" = p_id_usuario,
        "MotivoCancelacion" = p_motivo
    WHERE "Id_Venta" = p_id_venta;

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------------------------------
-- 7.5 CANCELACIONES: Buscar Ventas Cancelables
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION sp_buscar_ventas_cancelables(
    p_buscar VARCHAR
) RETURNS TABLE (
    "Id_Venta" INT,
    "No_Factura" VARCHAR,
    "Fecha_Venta" TIMESTAMP,
    "Monto_Total" NUMERIC,
    "Vendedor" VARCHAR,
    "Cliente" VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        v."Id_Venta",
        v."No_Factura",
        v."Fecha_Venta",
        v."Monto_Total",
        u."usuario"::VARCHAR AS "Vendedor",
        COALESCE(c."RazonSocial", 'Consumidor Final')::VARCHAR AS "Cliente"
    FROM "Ventas" v
    INNER JOIN "Usuarios" u ON v."Id_Usuario" = u."IdUsuario"
    LEFT JOIN "Clientes" c ON v."Id_Cliente" = c."IdCliente"
    WHERE (v."Cancelada" = FALSE OR v."Cancelada" IS NULL)
      AND (v."No_Factura" ILIKE '%' || p_buscar || '%'
           OR u."usuario" ILIKE '%' || p_buscar || '%'
           OR COALESCE(c."RazonSocial", '') ILIKE '%' || p_buscar || '%')
    ORDER BY v."Fecha_Venta" DESC
    LIMIT 100;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------------------------------
-- 7.6 CANCELACIONES: Obtener Detalle de Venta
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION sp_detalle_venta(
    p_id_venta INT
) RETURNS TABLE (
    "Codigo" VARCHAR,
    "Producto" VARCHAR,
    "Cantidad" NUMERIC,
    "PrecioUnit" NUMERIC,
    "Subtotal" NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        a."Codigo"::VARCHAR,
        a."Nombre"::VARCHAR AS "Producto",
        vd."Cantidad",
        COALESCE(vd."Precio_Venta", a."Precio") AS "PrecioUnit",
        vd."Monto_Total" AS "Subtotal"
    FROM "Ventas_Detalle" vd
    INNER JOIN "Articulos" a ON vd."Id_Articulo" = a."IdArticulo"
    WHERE vd."Id_Venta" = p_id_venta;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- 8. INDICES PARA OPTIMIZACION
-- =============================================================================

CREATE INDEX IF NOT EXISTS idx_ventas_fecha ON "Ventas"("Fecha_Venta");
CREATE INDEX IF NOT EXISTS idx_ventas_factura ON "Ventas"("No_Factura");
CREATE INDEX IF NOT EXISTS idx_ventas_cliente ON "Ventas"("Id_Cliente");
CREATE INDEX IF NOT EXISTS idx_ventas_cancelada ON "Ventas"("Cancelada");
CREATE INDEX IF NOT EXISTS idx_movimientos_fecha ON "MovimientosInventario"("FechaMovimiento");
CREATE INDEX IF NOT EXISTS idx_movimientos_articulo ON "MovimientosInventario"("IdArticulo");
CREATE INDEX IF NOT EXISTS idx_articulos_codigo ON "Articulos"("Codigo");
CREATE INDEX IF NOT EXISTS idx_articulos_activo ON "Articulos"("Activo");
CREATE INDEX IF NOT EXISTS idx_clientes_documento ON "Clientes"("Documento");
CREATE INDEX IF NOT EXISTS idx_clientes_activo ON "Clientes"("Activo");

-- =============================================================================
-- FIN DEL SCRIPT
-- =============================================================================
-- NOTAS:
-- 1. Ejecutar este script en una base de datos PostgreSQL limpia
-- 2. Compatible con Neon, Supabase y PostgreSQL local
-- 3. La zona horaria esta configurada para Argentina (UTC-3)
-- 4. Usuario por defecto: admin / admin
-- 5. Las ventas canceladas no se eliminan, solo se marcan como canceladas
-- =============================================================================
