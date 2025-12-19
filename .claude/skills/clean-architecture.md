# Clean Architecture en .NET

## Principios Fundamentales

### Inversión de Dependencias
Las capas externas dependen de las internas, nunca al revés.

```
Presentation → Application → Domain
Infrastructure → Application → Domain
```

### Separación de Responsabilidades
Cada capa tiene una responsabilidad clara y única.

## Implementación por Capas

### Domain Layer (Núcleo)

```csharp
// Entidad base
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Entidad de dominio
public class Producto : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public Guid CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = default!;
    
    // Lógica de dominio
    public bool TieneStockDisponible(int cantidad) => Stock >= cantidad;
    public void ReducirStock(int cantidad)
    {
        if (!TieneStockDisponible(cantidad))
            throw new DomainException("Stock insuficiente");
        Stock -= cantidad;
    }
}

// Value Object
public record Direccion(string Calle, string Ciudad, string CodigoPostal);

// Interfaz de Repository (contrato)
public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(Guid id);
    Task<IEnumerable<Producto>> GetAllAsync();
    Task<IEnumerable<Producto>> GetByCategoriaAsync(Guid categoriaId);
    Task AddAsync(Producto producto);
    Task UpdateAsync(Producto producto);
    Task DeleteAsync(Guid id);
}
```

### Application Layer

```csharp
// DTO
public record ProductoDto(
    Guid Id,
    string Nombre,
    decimal Precio,
    int Stock,
    string CategoriaNombre
);

public record CreateProductoDto(
    string Nombre,
    decimal Precio,
    int Stock,
    Guid CategoriaId
);

// Validator
public class CreateProductoValidator : AbstractValidator<CreateProductoDto>
{
    public CreateProductoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200);
        
        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0");
        
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("La categoría es requerida");
    }
}

// Service Interface
public interface IProductoService
{
    Task<ProductoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductoDto>> GetAllAsync();
    Task<ProductoDto> CreateAsync(CreateProductoDto dto);
    Task UpdateAsync(Guid id, UpdateProductoDto dto);
    Task DeleteAsync(Guid id);
}

// Service Implementation
public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductoService> _logger;
    
    public ProductoService(
        IProductoRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ProductoService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<ProductoDto> CreateAsync(CreateProductoDto dto)
    {
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Stock = dto.Stock,
            CategoriaId = dto.CategoriaId,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.AddAsync(producto);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Producto creado: {Id}", producto.Id);
        
        return MapToDto(producto);
    }
    
    private static ProductoDto MapToDto(Producto p) =>
        new(p.Id, p.Nombre, p.Precio, p.Stock, p.Categoria?.Nombre ?? "");
}
```

### Infrastructure Layer

```csharp
// DbContext
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

// Entity Configuration
public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("productos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Precio).HasPrecision(18, 2);
        
        builder.HasOne(x => x.Categoria)
               .WithMany(x => x.Productos)
               .HasForeignKey(x => x.CategoriaId);
    }
}

// Repository Implementation
public class ProductoRepository : IProductoRepository
{
    private readonly ApplicationDbContext _context;
    
    public ProductoRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Producto?> GetByIdAsync(Guid id) =>
        await _context.Productos
            .Include(x => x.Categoria)
            .FirstOrDefaultAsync(x => x.Id == id);
    
    public async Task<IEnumerable<Producto>> GetAllAsync() =>
        await _context.Productos
            .Include(x => x.Categoria)
            .ToListAsync();
    
    public async Task AddAsync(Producto producto) =>
        await _context.Productos.AddAsync(producto);
    
    public async Task UpdateAsync(Producto producto) =>
        _context.Productos.Update(producto);
    
    public async Task DeleteAsync(Guid id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
            _context.Productos.Remove(producto);
    }
}

// Unit of Work
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
```

### Dependency Injection

```csharp
// Program.cs
// Infrastructure
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

// Application
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductoValidator>();

// MudBlazor
builder.Services.AddMudServices();
```
