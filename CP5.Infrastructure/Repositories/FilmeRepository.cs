using CP5.Domain.Common;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using CP5.Domain.Interfaces;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CP5.Infrastructure.Repositories
{
    public class FilmeRepository : IFilmeRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FilmeRepository> _logger;

        public FilmeRepository(AppDbContext context, ILogger<FilmeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PageResult<Filme>> ObterTodosPaginadoAsync(int pageNumber, int pageSize, string? genero = null)
        {
            // Otimização: Uso de AsNoTracking() para consultas somente leitura
            var query = _context.Filmes
                .AsNoTracking()
                .Include(f => f.Avaliacoes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(genero))
            {
                var generoLower = genero.Trim().ToLower();
                query = query.Where(f => f.Genero.ToLower() == generoLower);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Filme>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<Filme?> ObterPorIdAsync(int id)
        {
            return await _context.Filmes
                .AsNoTracking()
                .Include(f => f.Avaliacoes)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> ExistePorTituloEAnoAsync(string titulo, int anoLancamento, int? ignorarId = null)
        {
            var query = _context.Filmes.AsNoTracking();
            if (ignorarId.HasValue)
            {
                query = query.Where(f => f.Id != ignorarId.Value);
            }

            return await query.AnyAsync(f =>
                f.Titulo.ToLower() == titulo.Trim().ToLower() &&
                f.AnoLancamento == anoLancamento);
        }

        // =========================================================================
        // ABORDAGEM 1: Retorno de Erro no Repository (OperationResult)
        // O repositório encapsula o erro em OperationResult sem lançar exceção.
        // =========================================================================
        public async Task<OperationResult<Filme>> AdicionarComResultadoAsync(Filme filme)
        {
            _logger.LogInformation("Repository: Inserindo filme via retorno de OperationResult: {Titulo}", filme.Titulo);

            try
            {
                // Verificação de consistência diretamente no repositório
                var duplicado = await _context.Filmes.AnyAsync(f =>
                    f.Titulo.ToLower() == filme.Titulo.ToLower() &&
                    f.AnoLancamento == filme.AnoLancamento);

                if (duplicado)
                {
                    _logger.LogWarning("Repository: Conflito detectado para o filme '{Titulo}' ({Ano})", filme.Titulo, filme.AnoLancamento);
                    return OperationResult<Filme>.Conflict(
                        $"[Repository Error] Conflito de unicidade: já existe um filme cadastrado com o título '{filme.Titulo}' no ano {filme.AnoLancamento}.");
                }

                await _context.Filmes.AddAsync(filme);
                await _context.SaveChangesAsync();

                return OperationResult<Filme>.Created(filme, "Filme persistido com sucesso pelo repositório.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Falha de persistência ao salvar filme: {Message}", ex.Message);
                return OperationResult<Filme>.InternalError($"[Repository Error] Erro ao salvar o registro no banco: {ex.Message}");
            }
        }

        // =========================================================================
        // ABORDAGEM 2: Lançamento de Erro Customizado no Repository (Exceptions)
        // O repositório lança explicitamente exceptions customizadas de domínio/repo.
        // =========================================================================
        public async Task<Filme> BuscarComErroLancadoAsync(int id)
        {
            _logger.LogInformation("Repository: Buscando filme ID {Id} com abordagem de lançamento de exceção", id);

            var filme = await _context.Filmes
                .AsNoTracking()
                .Include(f => f.Avaliacoes)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (filme == null)
            {
                _logger.LogWarning("Repository: Lançando EntityNotFoundException para ID {Id}", id);
                throw new EntityNotFoundException(nameof(Filme), id);
            }

            return filme;
        }

        public async Task AtualizarComLancamentoAsync(Filme filme)
        {
            _logger.LogInformation("Repository: Atualizando filme ID {Id} com verificação de exceção", filme.Id);

            var existe = await _context.Filmes.AnyAsync(f => f.Id == filme.Id);
            if (!existe)
            {
                _logger.LogWarning("Repository: Lançando EntityNotFoundException na atualização do ID {Id}", filme.Id);
                throw new EntityNotFoundException(nameof(Filme), filme.Id);
            }

            try
            {
                _context.Filmes.Update(filme);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Repository: Erro de banco ao atualizar filme ID {Id}", filme.Id);
                throw new RepositoryOperationException("AtualizarFilme", $"Falha ao persistir atualização do filme ID {filme.Id}", ex);
            }
        }

        public async Task RemoverComLancamentoAsync(int id)
        {
            _logger.LogInformation("Repository: Removendo filme ID {Id} com lançamento de exceção se não existir", id);

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null)
            {
                _logger.LogWarning("Repository: Lançando EntityNotFoundException na remoção do ID {Id}", id);
                throw new EntityNotFoundException(nameof(Filme), id);
            }

            try
            {
                _context.Filmes.Remove(filme);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Repository: Erro de banco ao remover filme ID {Id}", id);
                throw new RepositoryOperationException("RemoverFilme", $"Falha ao excluir filme ID {id}", ex);
            }
        }
    }
}
