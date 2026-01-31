-- =============================================
-- Script: Crear tabla de histórico de costos
-- Fecha: 2026-01-31
-- Descripción: Almacena el historial de costos
--              de compra unitario por producto
-- =============================================

-- 1. Crear tabla de histórico
CREATE TABLE IF NOT EXISTS "ProductoCostosHistorico" (
    "IdCostoHistorico" SERIAL PRIMARY KEY,
    "IdArticulo" INTEGER NOT NULL REFERENCES "Articulos"("IdArticulo") ON DELETE CASCADE,
    "CostoUnitario" NUMERIC(12,2) NOT NULL CHECK ("CostoUnitario" >= 0),
    "FechaRegistro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IdUsuarioRegistro" INTEGER REFERENCES "Usuarios"("IdUsuario")
);

-- 2. Índice para búsquedas por producto ordenadas por fecha
CREATE INDEX IF NOT EXISTS "IX_ProductoCostos_Articulo_Fecha"
ON "ProductoCostosHistorico"("IdArticulo", "FechaRegistro" DESC);

-- 3. Agregar columna de costo actual en Articulos (para acceso rápido)
ALTER TABLE "Articulos"
ADD COLUMN IF NOT EXISTS "CostoUnitario" NUMERIC(12,2) DEFAULT NULL;

-- 4. Comentarios de documentación
COMMENT ON TABLE "ProductoCostosHistorico" IS 'Histórico de costos de compra unitario por producto. Solo Admin puede ver/editar.';
COMMENT ON COLUMN "ProductoCostosHistorico"."IdArticulo" IS 'Producto al que pertenece el costo';
COMMENT ON COLUMN "ProductoCostosHistorico"."CostoUnitario" IS 'Costo de compra por unidad';
COMMENT ON COLUMN "ProductoCostosHistorico"."FechaRegistro" IS 'Fecha y hora en que se registró el costo';
COMMENT ON COLUMN "ProductoCostosHistorico"."IdUsuarioRegistro" IS 'Usuario admin que registró el costo';
COMMENT ON COLUMN "Articulos"."CostoUnitario" IS 'Último costo de compra unitario (cache del histórico)';
