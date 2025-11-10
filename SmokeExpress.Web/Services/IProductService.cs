// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Contrato para lidar com operações de produtos no contexto administrativo.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Lista todos os produtos cadastrados com suas respectivas categorias.
    /// </summary>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Coleção somente leitura de produtos.</returns>
    Task<IReadOnlyCollection<Product>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista produtos de forma paginada para uso administrativo.
    /// </summary>
    /// <param name="pageNumber">Número da página (inicia em 1).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Resultado paginado com produtos e metadados.</returns>
    Task<PagedResult<Product>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos paginados com filtros opcionais por termo de busca e categoria.
    /// </summary>
    /// <param name="termoBusca">Termo para buscar em Nome ou Descrição (opcional).</param>
    /// <param name="categoriaId">ID da categoria para filtrar (opcional).</param>
    /// <param name="pageNumber">Número da página (inicia em 1).</param>
    /// <param name="pageSize">Itens por página.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado de produtos.</returns>
    [Obsolete("Use BuscarPaginadoAsync com ProductSearchFilters para filtros avançados.")]
    Task<PagedResult<Product>> BuscarPaginadoAsync(string? termoBusca, int? categoriaId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos paginados com filtros avançados (preço, estoque, ordenação).
    /// </summary>
    /// <param name="filters">Filtros de busca avançada.</param>
    /// <param name="pageNumber">Número da página (inicia em 1).</param>
    /// <param name="pageSize">Itens por página.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado de produtos.</returns>
    Task<PagedResult<Product>> BuscarPaginadoAsync(ProductSearchFilters filters, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um produto específico com a respectiva categoria.
    /// </summary>
    /// <param name="id">Identificador do produto.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Produto encontrado ou <c>null</c> quando não existe.</returns>
    Task<Product?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um novo produto após validar informações obrigatórias.
    /// </summary>
    /// <param name="product">Entidade preenchida com os dados do produto.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Produto persistido.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="product"/> é nulo.</exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.ValidationException">
    /// Lançada quando os dados do produto não atendem às regras de negócio.
    /// </exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.BusinessException">Lançada quando ocorre erro ao salvar o produto.</exception>
    Task<Product> CriarAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um produto existente.
    /// </summary>
    /// <param name="product">Entidade com os dados já atualizados.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="product"/> é nulo.</exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.ValidationException">
    /// Lançada quando os dados informados são inválidos.
    /// </exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.NotFoundException">
    /// Lançada quando o produto informado não existe.
    /// </exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.BusinessException">Lançada quando ocorre erro ao salvar a alteração.</exception>
    Task AtualizarAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um produto, respeitando as restrições de pedidos associados.
    /// </summary>
    /// <param name="id">Identificador do produto.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    /// <exception cref="SmokeExpress.Web.Exceptions.ValidationException">
    /// Lançada quando o produto possui vínculos que impedem a exclusão.
    /// </exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.NotFoundException">Lançada quando o produto não existe.</exception>
    /// <exception cref="SmokeExpress.Web.Exceptions.BusinessException">Lançada quando ocorre erro ao remover o produto.</exception>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}



