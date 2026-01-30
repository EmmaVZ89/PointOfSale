-- Script para agregar campos de presentacion a la tabla Ventas_Detalle
-- Requerido para el sistema Web (soporte de presentaciones)
-- Fecha: 2026-01-29

-- Agregar columna IdPresentacion (referencia a ProductoPresentaciones)
-- Por defecto NULL para compatibilidad con ventas legacy WPF
ALTER TABLE "Ventas_Detalle"
ADD COLUMN IF NOT EXISTS "IdPresentacion" INTEGER DEFAULT NULL;

-- Agregar columna CantidadUnidadesPorPresentacion
-- Por defecto 1 para compatibilidad con ventas legacy WPF (unidad simple)
ALTER TABLE "Ventas_Detalle"
ADD COLUMN IF NOT EXISTS "CantidadUnidadesPorPresentacion" INTEGER DEFAULT 1;

-- Comentarios para documentacion
COMMENT ON COLUMN "Ventas_Detalle"."IdPresentacion" IS 'ID de la presentacion vendida (nullable para WPF legacy)';
COMMENT ON COLUMN "Ventas_Detalle"."CantidadUnidadesPorPresentacion" IS 'Unidades por presentacion (1=unidad, 6=pack x6, etc)';

-- Actualizar registros existentes para que tengan CantidadUnidadesPorPresentacion = 1
UPDATE "Ventas_Detalle"
SET "CantidadUnidadesPorPresentacion" = 1
WHERE "CantidadUnidadesPorPresentacion" IS NULL;
