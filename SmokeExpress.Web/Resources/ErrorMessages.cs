// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Resources;

/// <summary>
/// Mensagens de erro centralizadas do sistema.
/// Todas as mensagens de erro devem ser definidas aqui para garantir consistência.
/// </summary>
public static class ErrorMessages
{
    #region Validação

    /// <summary>
    /// UserId inválido ou vazio.
    /// </summary>
    public const string InvalidUserId = "UserId inválido.";

    /// <summary>
    /// UserId não pode ser vazio.
    /// </summary>
    public const string UserIdCannotBeEmpty = "UserId não pode ser vazio.";

    /// <summary>
    /// Carrinho de compras vazio.
    /// </summary>
    public const string EmptyCart = "Carrinho vazio.";

    /// <summary>
    /// Quantidade inválida em um ou mais itens.
    /// </summary>
    public const string InvalidQuantity = "Quantidade inválida em um ou mais itens.";

    /// <summary>
    /// Rating deve estar entre 0 e 5.
    /// </summary>
    public const string InvalidRating = "Rating deve estar entre 0 e 5.";

    /// <summary>
    /// Produto não pode ser nulo.
    /// </summary>
    public const string ProductCannotBeNull = "Produto não pode ser nulo.";

    /// <summary>
    /// Categoria não pode ser nula.
    /// </summary>
    public const string CategoryCannotBeNull = "Categoria não pode ser nula.";

    /// <summary>
    /// Endereço não pode ser nulo.
    /// </summary>
    public const string AddressCannotBeNull = "Endereço não pode ser nulo.";

    /// <summary>
    /// O nome do produto é obrigatório.
    /// </summary>
    public const string ProductNameRequired = "O nome do produto é obrigatório.";

    /// <summary>
    /// O nome da categoria é obrigatório.
    /// </summary>
    public const string CategoryNameRequired = "O nome da categoria é obrigatório.";

    /// <summary>
    /// O preço do produto não pode ser negativo.
    /// </summary>
    public const string ProductPriceCannotBeNegative = "O preço do produto não pode ser negativo.";

    /// <summary>
    /// O estoque do produto não pode ser negativo.
    /// </summary>
    public const string ProductStockCannotBeNegative = "O estoque do produto não pode ser negativo.";

    #endregion

    #region Negócio

    /// <summary>
    /// Erro ao criar produto. Tente novamente.
    /// </summary>
    public const string ErrorCreatingProduct = "Erro ao criar produto. Tente novamente.";

    /// <summary>
    /// Erro ao atualizar produto. Tente novamente.
    /// </summary>
    public const string ErrorUpdatingProduct = "Erro ao atualizar produto. Tente novamente.";

    /// <summary>
    /// Erro ao remover produto. Tente novamente.
    /// </summary>
    public const string ErrorRemovingProduct = "Erro ao remover produto. Tente novamente.";

    /// <summary>
    /// Erro ao criar categoria. Tente novamente.
    /// </summary>
    public const string ErrorCreatingCategory = "Erro ao criar categoria. Tente novamente.";

    /// <summary>
    /// Erro ao atualizar categoria. Tente novamente.
    /// </summary>
    public const string ErrorUpdatingCategory = "Erro ao atualizar categoria. Tente novamente.";

    /// <summary>
    /// Erro ao remover categoria. Tente novamente.
    /// </summary>
    public const string ErrorRemovingCategory = "Erro ao remover categoria. Tente novamente.";

    /// <summary>
    /// Erro ao criar endereço. Tente novamente.
    /// </summary>
    public const string ErrorCreatingAddress = "Erro ao criar endereço. Tente novamente.";

    /// <summary>
    /// Erro ao atualizar endereço. Tente novamente.
    /// </summary>
    public const string ErrorUpdatingAddress = "Erro ao atualizar endereço. Tente novamente.";

    /// <summary>
    /// Erro ao remover endereço. Tente novamente.
    /// </summary>
    public const string ErrorRemovingAddress = "Erro ao remover endereço. Tente novamente.";

    /// <summary>
    /// Erro ao definir endereço como padrão. Tente novamente.
    /// </summary>
    public const string ErrorSettingDefaultAddress = "Erro ao definir endereço como padrão. Tente novamente.";

    /// <summary>
    /// Erro ao processar pedido. Tente novamente.
    /// </summary>
    public const string ErrorProcessingOrder = "Erro ao processar pedido. Tente novamente.";

    /// <summary>
    /// Erro ao atualizar status do pedido. Tente novamente.
    /// </summary>
    public const string ErrorUpdatingOrderStatus = "Erro ao atualizar status do pedido. Tente novamente.";

    /// <summary>
    /// Erro ao criar avaliação. Tente novamente.
    /// </summary>
    public const string ErrorCreatingReview = "Erro ao criar avaliação. Tente novamente.";

    /// <summary>
    /// Quantidade acima do estoque para '{0}'. Disponível: {1}.
    /// </summary>
    public const string InsufficientStock = "Quantidade acima do estoque para '{0}'. Disponível: {1}.";

    /// <summary>
    /// Não é possível excluir o produto '{0}' pois ele possui pedidos associados.
    /// </summary>
    public const string CannotDeleteProductWithOrders = "Não é possível excluir o produto '{0}' pois ele possui pedidos associados.";

    /// <summary>
    /// Não é possível excluir a categoria '{0}' pois ela possui produtos associados.
    /// </summary>
    public const string CannotDeleteCategoryWithProducts = "Não é possível excluir a categoria '{0}' pois ela possui produtos associados.";

    #endregion

    #region Não Encontrado

    /// <summary>
    /// Produto não encontrado.
    /// </summary>
    public const string ProductNotFound = "Produto não encontrado.";

    /// <summary>
    /// Produto com ID {0} não encontrado.
    /// </summary>
    public const string ProductWithIdNotFound = "Produto com ID {0} não encontrado.";

    /// <summary>
    /// Categoria com ID {0} não encontrada.
    /// </summary>
    public const string CategoryWithIdNotFound = "Categoria com ID {0} não encontrada.";

    /// <summary>
    /// Um ou mais produtos não foram encontrados.
    /// </summary>
    public const string ProductsNotFound = "Um ou mais produtos não foram encontrados.";

    /// <summary>
    /// Pedido não encontrado.
    /// </summary>
    public const string OrderNotFound = "Pedido não encontrado.";

    /// <summary>
    /// Pedido com ID {0} não encontrado ou não pertence ao usuário.
    /// </summary>
    public const string OrderNotFoundOrNotOwned = "Pedido com ID {0} não encontrado ou não pertence ao usuário.";

    /// <summary>
    /// Produto com ID {0} não encontrado.
    /// </summary>
    public const string ProductWithIdNotFoundForReview = "Produto com ID {0} não encontrado.";

    #endregion

    #region Endereço

    /// <summary>
    /// Endereço de entrega é obrigatório.
    /// </summary>
    public const string AddressRequired = "Endereço de entrega é obrigatório.";

    /// <summary>
    /// O campo Rua do endereço de entrega é obrigatório.
    /// </summary>
    public const string AddressStreetRequired = "O campo Rua do endereço de entrega é obrigatório.";

    /// <summary>
    /// O campo Cidade do endereço de entrega é obrigatório.
    /// </summary>
    public const string AddressCityRequired = "O campo Cidade do endereço de entrega é obrigatório.";

    /// <summary>
    /// O campo Bairro do endereço de entrega é obrigatório.
    /// </summary>
    public const string AddressNeighborhoodRequired = "O campo Bairro do endereço de entrega é obrigatório.";

    /// <summary>
    /// O campo Rua do endereço é obrigatório.
    /// </summary>
    public const string AddressStreetRequiredSimple = "O campo Rua do endereço é obrigatório.";

    /// <summary>
    /// O campo Cidade do endereço é obrigatório.
    /// </summary>
    public const string AddressCityRequiredSimple = "O campo Cidade do endereço é obrigatório.";

    /// <summary>
    /// O campo Bairro do endereço é obrigatório.
    /// </summary>
    public const string AddressNeighborhoodRequiredSimple = "O campo Bairro do endereço é obrigatório.";

    /// <summary>
    /// Por favor, selecione um endereço de entrega válido antes de finalizar o pedido.
    /// </summary>
    public const string InvalidDeliveryAddress = "Por favor, selecione um endereço de entrega válido antes de finalizar o pedido.";

    #endregion

    #region Argumentos

    /// <summary>
    /// {0} não pode ser vazio.
    /// </summary>
    public const string ParameterCannotBeEmpty = "{0} não pode ser vazio.";

    /// <summary>
    /// {0} não pode ser negativo.
    /// </summary>
    public const string ParameterCannotBeNegative = "{0} não pode ser negativo.";

    #endregion

    #region Configuração

    /// <summary>
    /// A string de conexão 'DefaultConnection' não foi configurada.
    /// </summary>
    public const string ConnectionStringNotConfigured = "A string de conexão 'DefaultConnection' não foi configurada.";

    #endregion
}

