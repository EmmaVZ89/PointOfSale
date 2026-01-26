-- =============================================
-- Patch 002: Tabla ProductoPresentaciones
-- Permite manejar multiples presentaciones y precios por producto
-- El campo Precio de Articulos se mantiene para compatibilidad con WPF (precio unidad)
-- =============================================

-- Crear tabla de presentaciones
CREATE TABLE IF NOT EXISTS "ProductoPresentaciones" (
    "IdPresentacion" SERIAL PRIMARY KEY,
    "IdArticulo" INTEGER NOT NULL,
    "Nombre" VARCHAR(50) NOT NULL,           -- Ej: "Unidad", "Pack x6", "Caja x24"
    "CantidadUnidades" INTEGER NOT NULL,     -- Cantidad de unidades base (1, 6, 12, 18, 24)
    "Precio" DECIMAL(10,2) NOT NULL,         -- Precio de esta presentacion
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Presentacion_Articulo" FOREIGN KEY ("IdArticulo")
        REFERENCES "Articulos"("IdArticulo") ON DELETE CASCADE
);

-- Indice para busquedas rapidas por producto
CREATE INDEX IF NOT EXISTS "IX_Presentaciones_IdArticulo" ON "ProductoPresentaciones"("IdArticulo");

-- Insertar presentacion "Unidad" para todos los productos existentes
-- Esto mantiene compatibilidad: cada producto tendra al menos la presentacion unitaria
INSERT INTO "ProductoPresentaciones" ("IdArticulo", "Nombre", "CantidadUnidades", "Precio", "Activo")
SELECT "IdArticulo", 'Unidad', 1, "Precio", TRUE
FROM "Articulos"
WHERE NOT EXISTS (
    SELECT 1 FROM "ProductoPresentaciones" pp
    WHERE pp."IdArticulo" = "Articulos"."IdArticulo" AND pp."CantidadUnidades" = 1
);

-- Comentarios para documentacion
COMMENT ON TABLE "ProductoPresentaciones" IS 'Presentaciones y precios por producto (unidad, pack, caja, etc.)';
COMMENT ON COLUMN "ProductoPresentaciones"."CantidadUnidades" IS 'Cantidad de unidades base que contiene esta presentacion';
COMMENT ON COLUMN "ProductoPresentaciones"."Precio" IS 'Precio de venta para esta presentacion';
