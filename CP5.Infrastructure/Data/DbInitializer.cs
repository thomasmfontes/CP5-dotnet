using CP5.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CP5.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Filmes.AnyAsync())
            {
                var filmes = new List<Filme>
                {
                    new Filme(
                        "Interestelar",
                        "Uma equipe de exploradores viaja através de um buraco de minhoca no espaço para garantir a sobrevivência da humanidade.",
                        "Ficção Científica",
                        2014,
                        8.7m,
                        169,
                        "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg"
                    ),
                    new Filme(
                        "O Poderoso Chefão",
                        "O patriarca idoso de uma dinastia do crime organizado transfere o controle de seu império clandestino para seu filho relutante.",
                        "Drama",
                        1972,
                        9.2m,
                        175,
                        "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg"
                    ),
                    new Filme(
                        "Batman: O Cavaleiro das Trevas",
                        "Quando a ameaça conhecida como o Coringa causa estragos e caos no povo de Gotham, Batman deve aceitar um dos maiores testes psicológicos.",
                        "Ação",
                        2008,
                        9.0m,
                        152,
                        "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg"
                    ),
                    new Filme(
                        "A Origem",
                        "Um ladrão que rouba segredos corporativos por meio do uso da tecnologia de compartilhamento de sonhos tem a tarefa inversa de plantar uma ideia.",
                        "Ficção Científica",
                        2010,
                        8.8m,
                        148,
                        "https://image.tmdb.org/t/p/w500/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg"
                    ),
                    new Filme(
                        "Pulp Fiction: Tempo de Violência",
                        "As vidas de dois assassinos da máfia, um boxeador, a esposa de um gângster e dois bandidos se entrelaçam em quatro histórias de violência e redenção.",
                        "Crime",
                        1994,
                        8.9m,
                        154,
                        "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg"
                    )
                };

                await context.Filmes.AddRangeAsync(filmes);
                await context.SaveChangesAsync();

                var avaliacoes = new List<Avaliacao>
                {
                    new Avaliacao(filmes[0].Id, "Thomas Fontes", "Obra-prima do cinema! Trilha de Hans Zimmer inesquecível.", 5),
                    new Avaliacao(filmes[0].Id, "Mariana Silva", "Efeitos visuais e rigor científico extraordinários.", 5),
                    new Avaliacao(filmes[1].Id, "Carlos Eduardo", "Marlon Brando e Al Pacino em suas melhores atuações.", 5),
                    new Avaliacao(filmes[2].Id, "Lucas Mendes", "Heath Ledger entregou a melhor versão do Coringa da história.", 5),
                    new Avaliacao(filmes[3].Id, "Beatriz Ramos", "Roteiro instigante e direção primorosa de Christopher Nolan.", 5)
                };

                await context.Avaliacoes.AddRangeAsync(avaliacoes);
                await context.SaveChangesAsync();
            }

            if (!await context.Users.AnyAsync())
            {
                var users = new List<User>
                {
                    new User("admin", HashPassword("Admin@123"), "Administrador do Sistema", "Admin"),
                    new User("user", HashPassword("User@123"), "Usuário Comum", "User"),
                    new User("thomas", HashPassword("Fiap@2026"), "Thomas Fontes (RM562254)", "Admin")
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}
