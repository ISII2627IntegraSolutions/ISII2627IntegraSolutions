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
    
        // CU3 Subastar libros: tablas Subastas y SubastaItems de la base de datos
    public DbSet<Subasta> Subastas {get; set; }
    public DbSet<SubastaItem> SubastaItems {get; set; }

        // Métodos de pago (herencia TPH: una sola tabla con una columna que dice si es Visa, PayPal o GooglePay)

    public DbSet<MetodosdePago> MetodosdePago {get; set; }
    public DbSet<Visa> Visas {get; set; }
    public DbSet<PayPal> PayPals {get; set; }
    public DbSet<GooglePay> GooglePays {get; set; }

}
}