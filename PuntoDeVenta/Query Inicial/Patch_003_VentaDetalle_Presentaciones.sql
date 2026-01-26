-- =============================================
-- Patch 003: Agregar soporte de presentaciones a Ventas_Detalle
-- Permite rastrear que presentacion se vendio para calcular correctamente
-- las unidades al cancelar ventas
-- COMPATIBLE CON WPF: Columnas con DEFAULT, WPF sigue funcionando sin cambios
-- =============================================

-- Agregar columna IdPresentacion (nullable para ventas viejas)
ALTER TABLE "Ventas_Detalle"
ADD COLUMN IF NOT EXISTS "IdPresentacion" INTEGER NULL;

-- Agregar columna CantidadUnidadesPorPresentacion (default 1 para ventas viejas)
ALTER TABLE "Ventas_Detalle"
ADD COLUMN IF NOT EXISTS "CantidadUnidadesPorPresentacion" INTEGER DEFAULT 1;

-- Comentarios para documentacion
COMMENT ON COLUMN "Ventas_Detalle"."IdPresentacion" IS 'ID de la presentacion vendida (NULL para ventas legacy de WPF)';
COMMENT ON COLUMN "Ventas_Detalle"."CantidadUnidadesPorPresentacion" IS 'Unidades por presentacion (1=unidad, 6=pack x6, etc). Default 1 para compatibilidad con WPF';

-- Indice opcional para consultas por presentacion
CREATE INDEX IF NOT EXISTS "IX_VentaDetalle_IdPresentacion" ON "Ventas_Detalle"("IdPresentacion");
