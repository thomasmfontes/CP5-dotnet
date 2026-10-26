using CP5.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CP5.Infrastructure.Data.Configurations
{
    public class FilmeConfiguration : IEntityTypeConfiguration<Filme>
    {
        public void Configure(EntityTypeBuilder<Filme> builder)
        {
            builder.ToTable("TB_CP5_FILMES");

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).ValueGeneratedOnAdd();

            builder.Property(f => f.Titulo)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(f => f.Sinopse)
                .HasMaxLength(1000);

            builder.Property(f => f.Genero)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(f => f.AnoLancamento)
                .IsRequired();

            builder.Property(f => f.NotaImdb)
                .IsRequired()
                .HasPrecision(3, 1);

            builder.Property(f => f.DuracaoMinutos)
                .IsRequired();

            builder.Property(f => f.PosterUrl)
                .HasMaxLength(500);

            builder.Property(f => f.DataCadastro)
                .IsRequired();

            builder.Property(f => f.DataAtualizacao);

            // Índices de otimização de consultas (Critério Obrigatório de Otimização)
            builder.HasIndex(f => f.Titulo)
                .HasDatabaseName("IX_TB_CP5_FILMES_TITULO");

            builder.HasIndex(f => f.Genero)
                .HasDatabaseName("IX_TB_CP5_FILMES_GENERO");

            builder.HasIndex(f => new { f.Titulo, f.AnoLancamento })
                .HasDatabaseName("IX_TB_CP5_FILMES_TIT_ANO");

            builder.HasMany(f => f.Avaliacoes)
                .WithOne(a => a.Filme)
                .HasForeignKey(a => a.FilmeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
