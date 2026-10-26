using CP5.Domain.Common;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using CP5.Domain.Interfaces;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CP5.Infrastructure.Repositories
{
    public class AvaliacaoRepository : IAvaliacaoRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AvaliacaoRepository> _logger;

        public AvaliacaoRepository(AppDbContext context, ILogger<AvaliacaoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PageResult<Avaliacao>> ObterPorFilmeIdPaginadoAsync(int filmeId, int pageNumber, int pageSize)
        {
            var query = _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.FilmeId == filmeId);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.DataAvaliacao)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Avaliacao>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<Avaliacao?> ObterPorIdAsync(int id)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<OperationResult<Avaliacao>> AdicionarAsync(Avaliacao avaliacao)
        {
            _logger.LogInformation("Repository: Adicionando avaliação para filme ID {FilmeId}", avaliacao.FilmeId);

            try
            {
                var filmeExiste = await _context.Filmes.AnyAsync(f => f.Id == avaliacao.FilmeId);
                if (!filmeExiste)
                {
                    return OperationResult<Avaliacao>.NotFound($"Filme com ID {avaliacao.FilmeId} não existe.");
                }

                await _context.Avaliacoes.AddAsync(avaliacao);
                await _context.SaveChangesAsync();

                return OperationResult<Avaliacao>.Created(avaliacao, "Avaliação persistida com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Erro ao persistir avaliação: {Message}", ex.Message);
                return OperationResult<Avaliacao>.InternalError($"Erro ao salvar avaliação: {ex.Message}");
            }
        }

        public async Task RemoverAsync(int id)
        {
            _logger.LogInformation("Repository: Removendo avaliação ID {Id}", id);

            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null)
            {
                throw new EntityNotFoundException(nameof(Avaliacao), id);
            }

            _context.Avaliacoes.Remove(avaliacao);
            await _context.SaveChangesAsync();
        }
    }
}
