# 📦 MÓDULO COMPLETO: MovimientosMateriaPrima

## Arquitectura Hexagonal - .NET 8

---

## 📋 ÍNDICE

1. [Estructura del Módulo](#estructura-del-módulo)
2. [Entidades y DTOs](#entidades-y-dtos)
3. [Reglas de Negocio](#reglas-de-negocio)
4. [Endpoints API](#endpoints-api)
5. [Ejemplos de Uso](#ejemplos-de-uso)
6. [Scripts SQL](#scripts-sql)

---

## 🏗️ ESTRUCTURA DEL MÓDULO

### **Arquitectura por Capas**

```
┌─────────────────────────────────────────────────────────┐
│                    API LAYER                            │
│  ✅ MovimientosMateriaPrimaController                   │
│  - Endpoints REST                                       │
│  - Documentación Swagger                                │
│  - Manejo de excepciones                                │
└────────────────────┬────────────────────────────────────┘
					 │
┌────────────────────▼────────────────────────────────────┐
│               APPLICATION LAYER                         │
│  ✅ MovimientosMateriaPrimaService                      │
│  ✅ MovimientoMateriaPrimaDTOValidator                  │
│  - Lógica de negocio                                    │
│  - Validaciones FluentValidation                        │
│  - Cálculo de stock                                     │
└────────────────────┬────────────────────────────────────┘
					 │
┌────────────────────▼────────────────────────────────────┐
│             INFRASTRUCTURE LAYER                        │
│  ✅ IGenericRepository<T, TKey>                         │
│  ✅ GenericRepository<T, TKey>                          │
│  - Acceso a datos EF Core                               │
│  - DataContext                                          │
└────────────────────┬────────────────────────────────────┘
					 │
┌────────────────────▼────────────────────────────────────┐
│                 DOMAIN LAYER                            │
│  ✅ MovimientosMateriaPrimaEntity                       │
│  ✅ MovimientoMateriaPrimaDTO                           │
│  ✅ MovimientoMateriaPrimaResponseDTO                   │
│  ✅ TipoMovimientoMateriaPrima (Enum)                   │
│  - Sin dependencias externas                            │
└─────────────────────────────────────────────────────────┘
```

---

## 📊 ENTIDADES Y DTOS

### **1. MovimientosMateriaPrimaEntity**
```csharp
[Table("movimientos_materia_prima")]
public class MovimientosMateriaPrimaEntity
{
	[Key]
	[Column("id_movimiento")]
	public int Id { get; set; }

	[Column("id_lote_materia")]
	public int IdLoteMateria { get; set; }

	[Column("tipo_movimiento")]
	public int IdTipoMovimiento { get; set; }

	[Column("fecha")]
	public DateTime Fecha { get; set; }

	[Column("cantidad")]
	public decimal Cantidad { get; set; }

	[Column("id_unidad_medida")]
	public int IdUnidadMedida { get; set; }

	[Column("id_usuario")]
	public Guid IdUsuario { get; set; }

	[Column("observacion")]
	public string? Observacion { get; set; }
}
```

### **2. MovimientoMateriaPrimaDTO** (Input)
```csharp
public class MovimientoMateriaPrimaDTO
{
	public int IdLoteMateria { get; set; }
	public int IdTipoMovimiento { get; set; }
	public decimal Cantidad { get; set; }
	public int IdUnidadMedida { get; set; }
	public Guid IdUsuario { get; set; }
	public string? Observacion { get; set; }
}
```

### **3. MovimientoMateriaPrimaResponseDTO** (Output)
```csharp
public class MovimientoMateriaPrimaResponseDTO
{
	public int Id { get; set; }
	public int IdLoteMateria { get; set; }
	public int IdTipoMovimiento { get; set; }
	public string? TipoMovimientoNombre { get; set; }
	public DateTime Fecha { get; set; }
	public decimal Cantidad { get; set; }
	public int IdUnidadMedida { get; set; }
	public Guid IdUsuario { get; set; }
	public string? Observacion { get; set; }
	public decimal StockAnterior { get; set; }
	public decimal StockNuevo { get; set; }
}
```

### **4. TipoMovimientoMateriaPrima (Enum)**
```csharp
public enum TipoMovimientoMateriaPrima
{
	Entrada = 1,      // Suma al stock
	Consumo = 2,      // Resta del stock
	Ajuste = 3,       // Establece valor exacto
	Devolucion = 4,   // Suma al stock
	Vencimiento = 5   // Resta del stock
}
```

---

## 🎯 REGLAS DE NEGOCIO

### **Cálculo de Stock por Tipo de Movimiento**

| Tipo | ID | Operación | Fórmula | Descripción |
|------|----|-----------|---------| ------------|
| **Entrada** | 1 | SUMA | `Stock + Cantidad` | Entrada de nueva materia prima |
| **Consumo** | 2 | RESTA | `Stock - Cantidad` | Uso en producción |
| **Ajuste** | 3 | ESTABLECE | `Cantidad` | Corrección de inventario (valor absoluto) |
| **Devolución** | 4 | SUMA | `Stock + Cantidad` | Retorno al inventario |
| **Vencimiento** | 5 | RESTA | `Stock - Cantidad` | Baja por caducidad |

### **Validaciones Implementadas**

✅ **Cantidad > 0**  
✅ **IdLoteMateria válido (> 0)**  
✅ **IdTipoMovimiento entre 1 y 5**  
✅ **IdUnidadMedida > 0**  
✅ **IdUsuario no vacío**  
✅ **Observación máximo 500 caracteres**  
✅ **Stock resultante NO negativo**  
✅ **Lote debe existir**  

---

## 🔌 ENDPOINTS API

### **Base URL:** `/api/MovimientosMateriaPrima`

---

### **1. Crear Movimiento** ⭐ PRINCIPAL

```http
POST /api/MovimientosMateriaPrima/CrearMovimiento
Content-Type: application/json

{
  "idLoteMateria": 1,
  "idTipoMovimiento": 2,
  "cantidad": 50.5,
  "idUnidadMedida": 1,
  "idUsuario": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "observacion": "Consumo para producción"
}
```

**Respuesta 200 OK:**
```json
{
  "mensaje": "Movimiento creado exitosamente",
  "movimiento": {
	"id": 15,
	"idLoteMateria": 1,
	"idTipoMovimiento": 2,
	"tipoMovimientoNombre": "Consumo",
	"fecha": "2025-01-24T12:30:00",
	"cantidad": 50.5,
	"idUnidadMedida": 1,
	"idUsuario": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	"observacion": "Consumo para producción",
	"stockAnterior": 200.0,
	"stockNuevo": 149.5
  }
}
```

**Respuesta 400 Bad Request (Stock insuficiente):**
```json
{
  "mensaje": "Error al crear el movimiento",
  "detalle": "Stock insuficiente. Stock disponible: 20.0, Cantidad solicitada: 50.5. El movimiento resultaría en un stock negativo."
}
```

**Respuesta 400 Bad Request (Validación):**
```json
{
  "errores": [
	"La cantidad debe ser mayor a 0",
	"El tipo de movimiento debe estar entre 1 (Entrada) y 5 (Vencimiento)"
  ]
}
```

---

### **2. Obtener Todos los Movimientos**

```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientos
```

**Respuesta 200 OK:**
```json
[
  {
	"id": 1,
	"idLoteMateria": 1,
	"idTipoMovimiento": 1,
	"fecha": "2025-01-20T10:00:00",
	"cantidad": 100.0,
	"idUnidadMedida": 1,
	"idUsuario": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	"observacion": "Entrada inicial"
  },
  {
	"id": 2,
	"idLoteMateria": 1,
	"idTipoMovimiento": 2,
	"fecha": "2025-01-22T14:30:00",
	"cantidad": 25.5,
	"idUnidadMedida": 1,
	"idUsuario": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	"observacion": "Consumo producción"
  }
]
```

---

### **3. Obtener Movimiento por ID**

```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientoPorId/{id}
```

**Ejemplo:**
```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientoPorId/15
```

---

### **4. Obtener Movimientos por Lote**

```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientosPorLote/{idLote}
```

**Ejemplo:**
```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientosPorLote/1
```

**Descripción:** Obtiene el historial completo de movimientos de un lote específico.

---

### **5. Obtener Movimientos por Tipo**

```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientosPorTipo/{idTipoMovimiento}
```

**Ejemplo:**
```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientosPorTipo/2
```

**Descripción:** Filtra todos los movimientos de un tipo específico (ej: solo consumos).

---

### **6. Obtener Movimientos por Fechas**

```http
GET /api/MovimientosMateriaPrima/ObtenerMovimientosPorFechas?fechaInicio=2025-01-01&fechaFin=2025-01-31
```

**Descripción:** Obtiene movimientos en un rango de fechas.

---

### **7. Obtener Tipos de Movimiento** 📋

```http
GET /api/MovimientosMateriaPrima/ObtenerTiposMovimiento
```

**Respuesta 200 OK:**
```json
[
  { "id": 1, "nombre": "Entrada", "descripcion": "Entrada de materia prima al inventario" },
  { "id": 2, "nombre": "Consumo", "descripcion": "Consumo de materia prima en producción" },
  { "id": 3, "nombre": "Ajuste", "descripcion": "Ajuste de inventario (establece cantidad exacta)" },
  { "id": 4, "nombre": "Devolución", "descripcion": "Devolución de materia prima al inventario" },
  { "id": 5, "nombre": "Vencimiento", "descripcion": "Baja por vencimiento de materia prima" }
]
```

---

## 💡 EJEMPLOS DE USO

### **Ejemplo 1: Entrada de Materia Prima**

```json
POST /api/MovimientosMateriaPrima/CrearMovimiento

{
  "idLoteMateria": 5,
  "idTipoMovimiento": 1,
  "cantidad": 150.0,
  "idUnidadMedida": 1,
  "idUsuario": "user-guid-here",
  "observacion": "Entrada de compra #12345"
}
```

**Resultado:**  
✅ Stock: `200.0` → `350.0` (+150)

---

### **Ejemplo 2: Consumo en Producción**

```json
POST /api/MovimientosMateriaPrima/CrearMovimiento

{
  "idLoteMateria": 5,
  "idTipoMovimiento": 2,
  "cantidad": 75.5,
  "idUnidadMedida": 1,
  "idUsuario": "user-guid-here",
  "observacion": "Producción orden #456"
}
```

**Resultado:**  
✅ Stock: `350.0` → `274.5` (-75.5)

---

### **Ejemplo 3: Ajuste de Inventario**

```json
POST /api/MovimientosMateriaPrima/CrearMovimiento

{
  "idLoteMateria": 5,
  "idTipoMovimiento": 3,
  "cantidad": 300.0,
  "idUnidadMedida": 1,
  "idUsuario": "user-guid-here",
  "observacion": "Ajuste por conteo físico"
}
```

**Resultado:**  
✅ Stock: `274.5` → `300.0` (establece exacto)

---

### **Ejemplo 4: Devolución**

```json
POST /api/MovimientosMateriaPrima/CrearMovimiento

{
  "idLoteMateria": 5,
  "idTipoMovimiento": 4,
  "cantidad": 20.0,
  "idUnidadMedida": 1,
  "idUsuario": "user-guid-here",
  "observacion": "Devolución de producción"
}
```

**Resultado:**  
✅ Stock: `300.0` → `320.0` (+20)

---

### **Ejemplo 5: Baja por Vencimiento**

```json
POST /api/MovimientosMateriaPrima/CrearMovimiento

{
  "idLoteMateria": 5,
  "idTipoMovimiento": 5,
  "cantidad": 50.0,
  "idUnidadMedida": 1,
  "idUsuario": "user-guid-here",
  "observacion": "Vencimiento fecha límite superada"
}
```

**Resultado:**  
✅ Stock: `320.0` → `270.0` (-50)

---

## 🗄️ SCRIPTS SQL

### **1. Crear Tabla (si no existe)**

```sql
CREATE TABLE movimientos_materia_prima (
	id_movimiento INT IDENTITY(1,1) PRIMARY KEY,
	id_lote_materia INT NOT NULL,
	tipo_movimiento INT NOT NULL,
	fecha DATETIME NOT NULL,
	cantidad DECIMAL(18,2) NOT NULL,
	id_unidad_medida INT NOT NULL,
	id_usuario UNIQUEIDENTIFIER NOT NULL,
	observacion NVARCHAR(500) NULL,

	CONSTRAINT FK_Movimientos_Lotes FOREIGN KEY (id_lote_materia) 
		REFERENCES lotes_materia_prima(id_lote_materia),

	CONSTRAINT FK_Movimientos_TipoMovimiento FOREIGN KEY (tipo_movimiento) 
		REFERENCES tipo_movimiento_materia_prima(id_tipo_movimiento),

	CONSTRAINT FK_Movimientos_UnidadMedida FOREIGN KEY (id_unidad_medida) 
		REFERENCES unidad_medida(id_unidad_medida),

	CONSTRAINT CHK_Cantidad_Positiva CHECK (cantidad > 0)
);
```

### **2. Crear Tabla de Tipos de Movimiento**

```sql
CREATE TABLE tipo_movimiento_materia_prima (
	id_tipo_movimiento INT PRIMARY KEY,
	nombre NVARCHAR(50) NOT NULL,
	descripcion NVARCHAR(255) NULL
);

-- Insertar tipos de movimiento
INSERT INTO tipo_movimiento_materia_prima (id_tipo_movimiento, nombre, descripcion) VALUES
(1, 'Entrada', 'Entrada de materia prima al inventario'),
(2, 'Consumo', 'Consumo de materia prima en producción'),
(3, 'Ajuste', 'Ajuste de inventario'),
(4, 'Devolución', 'Devolución de materia prima al inventario'),
(5, 'Vencimiento', 'Baja por vencimiento de materia prima');
```

### **3. Índices Recomendados**

```sql
-- Índice para búsquedas por lote
CREATE NONCLUSTERED INDEX IX_Movimientos_IdLote 
ON movimientos_materia_prima(id_lote_materia);

-- Índice para búsquedas por fecha
CREATE NONCLUSTERED INDEX IX_Movimientos_Fecha 
ON movimientos_materia_prima(fecha DESC);

-- Índice para búsquedas por tipo de movimiento
CREATE NONCLUSTERED INDEX IX_Movimientos_Tipo 
ON movimientos_materia_prima(tipo_movimiento);
```

### **4. Vista para Consultas (Opcional)**

```sql
CREATE VIEW vw_movimientos_detalle AS
SELECT 
	m.id_movimiento,
	m.id_lote_materia,
	l.cantidad AS lote_cantidad_total,
	l.cantidad_disponible AS lote_stock_actual,
	m.tipo_movimiento,
	t.nombre AS tipo_movimiento_nombre,
	m.fecha,
	m.cantidad,
	m.id_unidad_medida,
	u.nombre AS unidad_medida_nombre,
	m.id_usuario,
	m.observacion,
	mp.nombre AS materia_prima_nombre
FROM movimientos_materia_prima m
INNER JOIN lotes_materia_prima l ON m.id_lote_materia = l.id_lote_materia
INNER JOIN tipo_movimiento_materia_prima t ON m.tipo_movimiento = t.id_tipo_movimiento
INNER JOIN unidad_medida u ON m.id_unidad_medida = u.id_unidad_medida
INNER JOIN materia_prima mp ON l.id_materia = mp.id_materia;
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

| # | Componente | Archivo | Estado |
|---|------------|---------|--------|
| 1 | Entidad Domain | `MovimientosMateriaPrimaEntity.cs` | ✅ |
| 2 | Enum | `TipoMovimientoMateriaPrima.cs` | ✅ |
| 3 | DTO Input | `MovimientoMateriaPrimaDTO.cs` | ✅ |
| 4 | DTO Output | `MovimientoMateriaPrimaResponseDTO.cs` | ✅ |
| 5 | Validator | `MovimientoMateriaPrimaDTOValidator.cs` | ✅ |
| 6 | Service | `MovimientosMateriaPrimaService.cs` | ✅ |
| 7 | Controller | `MovimientosMateriaPrimaController.cs` | ✅ |
| 8 | Dependency Injection | `Injections.cs` | ✅ |
| 9 | Repository (Genérico) | Ya existe | ✅ |
| 10 | Compilación | Build exitoso | ✅ |

---

## 🎓 CONCEPTOS CLAVE

### **Arquitectura Hexagonal**
- ✅ **Domain**: Entidades puras sin dependencias
- ✅ **Application**: Lógica de negocio y validaciones
- ✅ **Infrastructure**: Acceso a datos (EF Core)
- ✅ **API**: Controladores y endpoints REST

### **Patrones Implementados**
- ✅ **Repository Pattern**: `IGenericRepository<T, TKey>`
- ✅ **DTO Pattern**: Separación entre entidades y DTOs
- ✅ **Dependency Injection**: Services registrados en IoC container
- ✅ **FluentValidation**: Validaciones declarativas
- ✅ **CQRS Light**: Separación de comandos (Create) y queries (Get)

### **Validaciones de Stock**
```csharp
// ANTES de crear el movimiento
if (nuevoStock < 0)
{
	throw new Exception("Stock insuficiente...");
}

// DESPUÉS de validar, se actualiza
lote.CantidadDisponible = nuevoStock;
```

---

## 📞 SOPORTE Y TESTING

### **Testing con Swagger**
1. Ejecutar la API
2. Navegar a `/swagger`
3. Probar endpoint `POST /CrearMovimiento`
4. Verificar stock en `GET /ObtenerMovimientosPorLote/{id}`

### **Postman Collection** (Ejemplo)
```json
{
  "info": { "name": "MovimientosMateriaPrima API" },
  "item": [
	{
	  "name": "Crear Movimiento Entrada",
	  "request": {
		"method": "POST",
		"url": "{{baseUrl}}/api/MovimientosMateriaPrima/CrearMovimiento",
		"body": {
		  "mode": "raw",
		  "raw": "{ \"idLoteMateria\": 1, \"idTipoMovimiento\": 1, \"cantidad\": 100, \"idUnidadMedida\": 1, \"idUsuario\": \"guid\", \"observacion\": \"Test\" }"
		}
	  }
	}
  ]
}
```

---

## 🚀 DEPLOYMENT

### **Pasos para Producción**
1. ✅ Ejecutar scripts SQL en BD
2. ✅ Verificar conexión DB en `appsettings.json`
3. ✅ Compilar proyecto: `dotnet build`
4. ✅ Publicar: `dotnet publish -c Release`
5. ✅ Configurar IIS/Kestrel
6. ✅ Verificar Swagger en `/swagger`

---

**✅ Módulo MovimientosMateriaPrima completamente implementado y funcional**
