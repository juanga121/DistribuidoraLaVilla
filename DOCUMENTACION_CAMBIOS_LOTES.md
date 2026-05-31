# Documentación de Cambios: Implementación de CantidadInicial y CantidadDisponible

## Resumen
Se agregaron las propiedades `CantidadInicial` y `CantidadDisponible` a las entidades de lotes (LotesMateriaPrima y LotesProductos) siguiendo la arquitectura hexagonal del proyecto.

---

## 1. CAPA DOMAIN (Entidades)

### 1.1 LotesMateriaPrimaEntity.cs
**Ubicación:** `DistribuidoraLaVilla.Domain\Entities\LotesMateriaPrimaEntity.cs`

**Propiedades agregadas:**
```csharp
[Column("cantidad_inicial")]
public decimal CantidadInicial { get; set; }

[Column("cantidad_disponible")]
public decimal CantidadDisponible { get; set; }
```

### 1.2 LotesProductosEntity.cs
**Ubicación:** `DistribuidoraLaVilla.Domain\Entities\Productos\LotesProductosEntity.cs`

**Propiedades agregadas:**
```csharp
[Column("cantidad_inicial")]
public decimal CantidadInicial { get; set; }

[Column("cantidad_disponible")]
public decimal CantidadDisponible { get; set; }
```

---

## 2. CAPA APPLICATION (Validadores)

### 2.1 LotesMateriaPrimaDTOValidator.cs
**Ubicación:** `DistribuidoraLaVilla.Application\Validators\LotesMateriaPrimaDTOValidator.cs`

**Validaciones implementadas:**
- `Cantidad > 0` (requerido)
- `CostoUnitario > 0`
- `IdMarca > 0`
- `IdMateria > 0`
- `IdProveedor` no vacío
- `FechaVencimiento` posterior a fecha actual
- `IdUnidadMedida > 0`

### 2.2 LotesProductosDTOValidator.cs
**Ubicación:** `DistribuidoraLaVilla.Application\Validators\LotesProductosDTOValidator.cs`

**Validaciones implementadas:**
- `CantidadUnidades > 0` (requerido)
- `PesoTotal > 0` (requerido)
- `PrecioUnitario > 0`
- `PrecioKilo > 0`
- `IdProducto > 0`
- `IdMarca > 0`
- `IdProveedor` no vacío
- `FechaVencimiento` posterior a fecha actual
- `IdUnidadMedida > 0`

---

## 3. CAPA APPLICATION (Servicios)

### 3.1 LotesMateriaPrimaService.cs
**Ubicación:** `DistribuidoraLaVilla.Application\Services\MateriaPrima\LotesMateriaPrimaService.cs`

**Cambios en CrearLoteMateriaPrimaAsync:**
```csharp
// Validación con FluentValidation
var validator = new LotesMateriaPrimaDTOValidator();
var validationResult = await validator.ValidateAsync(lotesMateriaPrimaDTO);

if (!validationResult.IsValid)
{
	throw new ValidationException(validationResult.Errors);
}

// Al crear el lote
CantidadInicial = lotesMateriaPrimaDTO.Cantidad,
CantidadDisponible = lotesMateriaPrimaDTO.Cantidad,
```

**Cambios en ActualizarLoteMateriaPrima:**
```csharp
// Validación con FluentValidation
var validator = new LotesMateriaPrimaDTOValidator();
var validationResult = await validator.ValidateAsync(lotesMateriaPrimaDTO);

if (!validationResult.IsValid)
{
	throw new ValidationException(validationResult.Errors);
}

// Calcular diferencia para ajustar cantidad disponible
var diferenciaCantidad = lotesMateriaPrimaDTO.Cantidad - lote.Cantidad;
lote.CantidadInicial = lotesMateriaPrimaDTO.Cantidad;
lote.CantidadDisponible += diferenciaCantidad;
```

### 3.2 LotesProductosService.cs
**Ubicación:** `DistribuidoraLaVilla.Application\Services\Productos\LotesProductosService.cs`

**Cambios en CrearLoteProductoAsync:**
```csharp
// Validación con FluentValidation
var validator = new LotesProductosDTOValidator();
var validationResult = await validator.ValidateAsync(lotesProductosDTO);

if (!validationResult.IsValid)
{
	throw new ValidationException(validationResult.Errors);
}

// Al crear el lote (usa PesoTotal como cantidad)
CantidadInicial = lotesProductosDTO.PesoTotal,
CantidadDisponible = lotesProductosDTO.PesoTotal,
```

**Cambios en ActualizarLoteProducto:**
```csharp
// Validación con FluentValidation
var validator = new LotesProductosDTOValidator();
var validationResult = await validator.ValidateAsync(lotesProductosDTO);

if (!validationResult.IsValid)
{
	throw new ValidationException(validationResult.Errors);
}

// Calcular diferencia para ajustar cantidad disponible
var diferenciaPeso = lotesProductosDTO.PesoTotal - lote.PesoTotal;
lote.CantidadInicial = lotesProductosDTO.PesoTotal;
lote.CantidadDisponible += diferenciaPeso;
```

---

## 4. CAPA API (Controladores)

### 4.1 LotesMateriaPrimaController.cs
**Ubicación:** `DistribuidoraLaVilla.Api\Controllers\MateriaPrima\LotesMateriaPrimaController.cs`

**Manejo de excepciones agregado:**
```csharp
[HttpPost]
[Route("CrearLoteMateriaPrima")]
public async Task<IActionResult> CrearLoteMateriaPrima([FromBody] LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
{
	try
	{
		await _lotesMateriaPrimaService.CrearLoteMateriaPrimaAsync(lotesMateriaPrimaDTO);
		return Ok(new { mensaje = "Lote de materia prima creado exitosamente" });
	}
	catch (ValidationException ex)
	{
		return BadRequest(new { errores = ex.Errors.Select(e => e.ErrorMessage) });
	}
	catch (Exception ex)
	{
		return StatusCode(500, new { mensaje = "Error al crear el lote", detalle = ex.Message });
	}
}
```

**Similar para ActualizarLoteMateriaPrima.**

### 4.2 LotesProductosController.cs
**Ubicación:** `DistribuidoraLaVilla.Api\Controllers\Productos\LotesProductosController.cs`

**Manejo de excepciones idéntico al de LotesMateriaPrimaController.**

---

## 5. DEPENDENCIAS

### 5.1 Paquete NuGet instalado
```bash
dotnet add DistribuidoraLaVilla.Application\DistribuidoraLaVilla.Application.csproj package FluentValidation
```

**Versión instalada:** FluentValidation 12.1.1

---

## 6. INFRAESTRUCTURA (Base de Datos)

### 6.1 Scripts SQL necesarios

**Para LotesMateriaPrima:**
```sql
ALTER TABLE lotes_materia_prima 
ADD cantidad_inicial DECIMAL(18,2) NULL,
	cantidad_disponible DECIMAL(18,2) NULL;

-- Actualizar registros existentes
UPDATE lotes_materia_prima 
SET cantidad_inicial = cantidad,
	cantidad_disponible = cantidad
WHERE cantidad_inicial IS NULL;
```

**Para LotesProductos:**
```sql
ALTER TABLE lotes_productos 
ADD cantidad_inicial DECIMAL(18,2) NULL,
	cantidad_disponible DECIMAL(18,2) NULL;

-- Actualizar registros existentes
UPDATE lotes_productos 
SET cantidad_inicial = peso_total,
	cantidad_disponible = peso_total
WHERE cantidad_inicial IS NULL;
```

---

## 7. COMPORTAMIENTO IMPLEMENTADO

### 7.1 Al crear un lote:
- `CantidadInicial` = cantidad ingresada (Cantidad para materia prima, PesoTotal para productos)
- `CantidadDisponible` = cantidad ingresada

### 7.2 Al actualizar un lote:
- Se calcula la diferencia entre la nueva cantidad y la cantidad anterior
- `CantidadInicial` se actualiza al nuevo valor
- `CantidadDisponible` se ajusta sumando/restando la diferencia

### 7.3 Validaciones:
- Las cantidades deben ser mayores a 0
- Se validan todos los campos requeridos
- Fecha de vencimiento debe ser futura
- Retorna errores claros en formato JSON

---

## 8. ENDPOINTS NO MODIFICADOS

Los siguientes endpoints mantienen su funcionalidad original sin cambios:
- `GET /ObtenerLotesMateriaPrima`
- `GET /ObtenerLoteMateriaPrimaPorId/{id}`
- `PUT /ActualizarEstadoLoteMateriaPrima`
- `GET /ObtenerLotesMateriaPrimaDisponibles`
- `DELETE /EliminarLoteMateriaPrima/{idLote}`
- `GET /ObtenerLotesProductos`
- `GET /ObtenerLoteProductoPorId/{id}`
- `PUT /ActualizarEstadoLoteProducto`
- `GET /ObtenerLotesProductosDisponibles`
- `DELETE /EliminarLoteProducto/{idLote}`

---

## 9. ARQUITECTURA HEXAGONAL MANTENIDA

✅ **Domain:** Entidades puras sin lógica de negocio  
✅ **Application:** Validadores y servicios con lógica de negocio  
✅ **Infrastructure:** Sin cambios (no se usan migrations)  
✅ **API:** Controladores con manejo de excepciones  

---

## 10. PRUEBAS SUGERIDAS

### 10.1 Crear lote con cantidad válida:
```json
POST /api/LotesMateriaPrima/CrearLoteMateriaPrima
{
  "cantidad": 100,
  "costoUnitario": 25.50,
  "idMarca": 1,
  "idMateria": 5,
  "idProveedor": "guid-here",
  "fechaVencimiento": "2026-12-31",
  "idUnidadMedida": 1
}
```

### 10.2 Crear lote con cantidad inválida (debe fallar):
```json
POST /api/LotesMateriaPrima/CrearLoteMateriaPrima
{
  "cantidad": 0,
  ...
}
```

**Respuesta esperada:**
```json
{
  "errores": [
	"La cantidad debe ser mayor a 0"
  ]
}
```

---

## 11. COMPILACIÓN

✅ **Estado:** Compilación exitosa  
✅ **Sin errores**  
✅ **Sin warnings**
