using AgendaContactos.Back.Models;
using Microsoft.EntityFrameworkCore;
namespace AgendaContactos.Back.Repositories;

/// <summary>
/// Contexto de base de datos principal de la aplicación, encargado de gestionar
/// la persistencia y el mapeo de entidades mediante Entity Framework Core y SQLite.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Obtiene o establece el conjunto de datos de contactos almacenados en la base de datos.
    /// </summary>
    public DbSet<Contact> Contacts { get; set; } = null!;
    
    /// <summary>
    /// Asegura que el directorio físico de almacenamiento exista y crea la base de datos 
    /// y sus tablas si aún no han sido creadas.
    /// </summary>
    public void EnsureCreated()
    {
        if (!Directory.Exists(Configuration.Config.DataBaseFolder))
        {
            Directory.CreateDirectory(Configuration.Config.DataBaseFolder);
        }
        Database.EnsureCreated();
    }

    /// <summary>
    /// Configura el modelo relacional y las restricciones de las entidades antes de construir la base de datos.
    /// </summary>
    /// <param name="modelBuilder">Constructor utilizado para configurar las entidades y sus relaciones.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            // Configura el número de teléfono como clave primaria
            entity.HasKey(e => e.PhoneNumber);
        
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(16)
                .IsRequired();
            
            // Crea un índice único en la base de datos para garantizar que no existan emails duplicados
            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();
            
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.Alias)
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}