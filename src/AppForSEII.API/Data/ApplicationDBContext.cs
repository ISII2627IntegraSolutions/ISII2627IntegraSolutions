
namespace AppForSEII.API.Data
{

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {

        base.OnModelCreating(builder);


    }


    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Editorial> Editoriales {get; set; }
    public DbSet<Libro> Libros {get; set; }
    public DbSet<Compra> Compras {get; set; }
    public DbSet<ComprarItem> ComprarItems {get; set; }
    public DbSet<Genero> Generos {get; set; }
    public DbSet<Reposicion> Reposiciones {get; set; }
    public DbSet<ReposicionItem> ReposicionItems {get; set; }
    
    public DbSet<Subasta> Subastas {get; set; }
    public DbSet<SubastaItem> SubastaItems {get; set; }




}
}