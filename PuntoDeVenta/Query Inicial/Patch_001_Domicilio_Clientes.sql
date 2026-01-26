-- =============================================================================
-- PATCH 001: Agregar campo Domicilio a tabla Clientes
-- Punto de Venta - Distribuidora LA FAMILIA
-- Fecha: 2026-01-19
-- Descripcion: Agrega el campo Domicilio a la tabla Clientes para almacenar
--              la direccion del cliente y mostrarla en tickets/presupuestos.
-- =============================================================================

-- Agregar columna Domicilio si no existe
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'Clientes' AND column_name = 'Domicilio'
    ) THEN
        ALTER TABLE "Clientes" ADD COLUMN "Domicilio" VARCHAR(200);
        RAISE NOTICE 'Columna Domicilio agregada exitosamente a la tabla Clientes';
    ELSE
        RAISE NOTICE 'La columna Domicilio ya existe en la tabla Clientes';
    END IF;
END $$;

-- =============================================================================
-- FIN DEL PATCH
-- =============================================================================
