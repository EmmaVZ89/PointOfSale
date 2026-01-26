-- Script para agregar campos de forma de pago a la tabla Ventas
-- Compatible con el sistema legacy WPF (campos opcionales con valores por defecto)
-- Fecha: 2026-01-25

-- Agregar columna FormaPago (E = Efectivo, T = Transferencia)
-- Por defecto 'E' (Efectivo) para compatibilidad con ventas existentes
ALTER TABLE "Ventas"
ADD COLUMN IF NOT EXISTS "FormaPago" VARCHAR(1) DEFAULT 'E';

-- Agregar columna MontoRecibido (monto que entrega el cliente en efectivo)
-- Por defecto NULL para ventas por transferencia o cuando no se registra
ALTER TABLE "Ventas"
ADD COLUMN IF NOT EXISTS "MontoRecibido" DECIMAL(18,2) DEFAULT NULL;

-- Comentarios para documentacion
COMMENT ON COLUMN "Ventas"."FormaPago" IS 'Forma de pago: E=Efectivo, T=Transferencia';
COMMENT ON COLUMN "Ventas"."MontoRecibido" IS 'Monto recibido del cliente (solo para efectivo)';

-- Actualizar ventas existentes (si hay) para que tengan FormaPago = 'E'
UPDATE "Ventas" SET "FormaPago" = 'E' WHERE "FormaPago" IS NULL;
