using CP5.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CP5.Infrastructure.Data.Configurations
{
    public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
    {
        public void Configure(EntityTypeBuilder<Avaliacao> builder)
        {
            builder.ToTable("TB_CP5_AVALIACOES");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedOnAdd();

            builder.Property(a => a.FilmeId)
                .IsRequired();

            builder.Property(a => a.Usuario)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(a => a.Comentario)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.Nota)
                .IsRequired();

            builder.Property(a => a.DataAvaliacao)
                .IsRequired();

            // Índices de otimização de consultas
            builder.HasIndex(a => a.FilmeId)
                .HasDatabaseName("IX_TB_CP5_AVALIACOES_FILMEID");

            builder.HasIndex(a => a.Usuario)
                .HasDatabaseName("IX_TB_CP5_AVALIACOES_USUARIO");
        }
    }
}
