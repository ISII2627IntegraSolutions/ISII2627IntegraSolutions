namespace AppForSEII.API.Data {
    public class SeedData {
        public static void Initialize(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger logger) {
            List<string> rolesNames = new List<string> { "Administrator", "Employee", "Customer" };

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the roles in the Database.");
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            try {
                SeedUsers(userManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Users in the Database.");
            }

            try {
                SeedGenerosEditorialesYLibros(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Generos, Editoriales and Libros in the Database.");
            }

            var admin = dbContext.Users.OfType<ApplicationUser>().FirstOrDefault(u => u.UserName == "elena@uclm.es");
            var clientes = dbContext.Users.OfType<ApplicationUser>().Where(u => u.UserName != "elena@uclm.es").ToList();

        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager, List<string> roles) {

            foreach (string roleName in roles) {
                //it checks such role does not exist in the database
                if (!roleManager.RoleExistsAsync(roleName).Result) {
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    role.NormalizedName = roleName;
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }

        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, List<string> roles) {
            //first, it checks the user does not already exist in the DB
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //administrator role
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }


            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                //A customer class has been defined because it has different attributes (purchase, rental, etc.)
                ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "OtherPass12$");

                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //customer role
                    userManager.AddToRoleAsync(user, roles[2]).Wait();

                }
            }

            string[,] clientes = {
                { "4", "Lucía", "García López", "lucia@uclm.es", "612345671" },
                { "5", "Carlos", "Martínez Ruiz", "carlos@uclm.es", "612345672" },
                { "6", "María", "Sánchez Gil", "maria@uclm.es", "612345673" },
                { "7", "Javier", "Fernández Mora", "javier@uclm.es", "612345674" },
                { "8", "Laura", "Gómez Pérez", "laura@uclm.es", "612345675" },
                { "9", "Pablo", "Díaz Romero", "pablo@uclm.es", "612345676" },
                { "10", "Sara", "Moreno Cano", "sara@uclm.es", "612345677" },
                { "11", "Andrés", "Navarro Ortiz", "andres@uclm.es", "612345678" }
            };

            for (int i = 0; i < clientes.GetLength(0); i++) {
                if (userManager.FindByNameAsync(clientes[i, 3]).Result == null) {
                    ApplicationUser user = new ApplicationUser(clientes[i, 0], clientes[i, 1], clientes[i, 2], clientes[i, 3]);
                    user.PhoneNumber = clientes[i, 4];
                    user.EmailConfirmed = true;

                    var result = userManager.CreateAsync(user, "Cliente1234%");
                    result.Wait();

                    if (result.IsCompletedSuccessfully) {
                        //customer role
                        userManager.AddToRoleAsync(user, roles[2]).Wait();
                    }
                }
            }

        }

        public static void SeedGenerosEditorialesYLibros(ApplicationDbContext dbcontext) {
            if (dbcontext.Libros.Any())
                return;

            string[] nombresGeneros = ["Novela", "Fantasía", "Ciencia Ficción", "Terror", "Romance", "Historia", "Poesía", "Biografía", "Misterio", "Aventura"];
            string[] nombresEditoriales = ["Planeta", "Anagrama", "Alfaguara", "Santillana", "Penguin Random House", "Tusquets", "Salamandra", "Debolsillo", "Espasa", "Siruela"];

            List<Genero> generos = new List<Genero>();
            foreach (string nombre in nombresGeneros)
                generos.Add(new Genero { Nombre = nombre });

            List<Editorial> editoriales = new List<Editorial>();
            foreach (string nombre in nombresEditoriales)
                editoriales.Add(new Editorial { Nombre = nombre });

            dbcontext.Generos.AddRange(generos);
            dbcontext.Editoriales.AddRange(editoriales);

            dbcontext.Libros.AddRange(
                new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", FechaLanzamiento = new DateTime(1967, 5, 30), PrecioCompra = 19.95, Stock = 12, TipoLibro = "Tapa dura", Editorial = editoriales[2], Genero = generos[0] },
                new Libro { Titulo = "El nombre del viento", Autor = "Patrick Rothfuss", FechaLanzamiento = new DateTime(2007, 3, 27), PrecioCompra = 24.90, Stock = 8, TipoLibro = "Tapa blanda", Editorial = editoriales[6], Genero = generos[1] },
                new Libro { Titulo = "Dune", Autor = "Frank Herbert", FechaLanzamiento = new DateTime(1965, 8, 1), PrecioCompra = 21.50, Stock = 5, TipoLibro = "Bolsillo", Editorial = editoriales[7], Genero = generos[2] },
                new Libro { Titulo = "It", Autor = "Stephen King", FechaLanzamiento = new DateTime(1986, 9, 15), PrecioCompra = 18.00, Stock = 0, TipoLibro = "Bolsillo", Editorial = editoriales[0], Genero = generos[3] },
                new Libro { Titulo = "Orgullo y prejuicio", Autor = "Jane Austen", FechaLanzamiento = new DateTime(1813, 1, 28), PrecioCompra = 12.95, Stock = 7, TipoLibro = "Tapa blanda", Editorial = editoriales[8], Genero = generos[4] },
                new Libro { Titulo = "Sapiens", Autor = "Yuval Noah Harari", FechaLanzamiento = new DateTime(2011, 1, 1), PrecioCompra = 22.90, Stock = 0, TipoLibro = "Tapa dura", Editorial = editoriales[4], Genero = generos[5] },
                new Libro { Titulo = "Veinte poemas de amor", Autor = "Pablo Neruda", FechaLanzamiento = new DateTime(1924, 6, 1), PrecioCompra = 9.95, Stock = 15, TipoLibro = "Bolsillo", Editorial = editoriales[9], Genero = generos[6] },
                new Libro { Titulo = "Steve Jobs", Autor = "Walter Isaacson", FechaLanzamiento = new DateTime(2011, 10, 24), PrecioCompra = 26.50, Stock = 3, TipoLibro = "Tapa dura", Editorial = editoriales[1], Genero = generos[7] },
                new Libro { Titulo = "Diez negritos", Autor = "Agatha Christie", FechaLanzamiento = new DateTime(1939, 11, 6), PrecioCompra = 10.50, Stock = 0, TipoLibro = "Bolsillo", Editorial = editoriales[3], Genero = generos[8] },
                new Libro { Titulo = "La isla del tesoro", Autor = "Robert Louis Stevenson", FechaLanzamiento = new DateTime(1883, 11, 14), PrecioCompra = 11.90, Stock = 9, TipoLibro = "Edición ilustrada", Editorial = editoriales[5], Genero = generos[9] }
            );

            dbcontext.SaveChanges();
        }

    }
}
