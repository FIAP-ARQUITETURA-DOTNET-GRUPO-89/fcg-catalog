using FcgCatalog.Domain.Entities;
using FcgCatalog.SharedKernel.Exceptions;
using Shouldly;

namespace FcgCatalog.UnitTests.Domain.Entities;

public class GameReviewTests
{
    private static GameReview CreateValidGameReview() =>
        new(Guid.NewGuid(), Guid.NewGuid(), 5, "Excelente jogo, super recomendo!");

    [Fact]
    public void Dado_DadosValidos_Quando_CriarGameReview_Entao_PropriedadesSaoPreenchidas()
    {
        // Arrange
        var jogoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var review = new GameReview(jogoId, usuarioId, 4, " Muito bom ");

        // Assert
        review.Id.ShouldNotBe(Guid.Empty);
        review.JogoId.ShouldBe(jogoId);
        review.UsuarioId.ShouldBe(usuarioId);
        review.Nota.ShouldBe(4);
        review.Comentario.ShouldBe("Muito bom"); // Testa o Trim()
        review.CreatedAt.ShouldNotBe(default);
        review.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Dado_JogoIdVazio_Quando_CriarGameReview_Entao_LancaBusinessException()
    {
        // Arrange & Act
        var action = () => new GameReview(Guid.Empty, Guid.NewGuid(), 5, "Comentário válido");

        // Assert
        var exception = Should.Throw<BusinessException>(action);
        exception.Message.ShouldBe("O identificador do jogo é obrigatório.");
    }

    [Fact]
    public void Dado_UsuarioIdVazio_Quando_CriarGameReview_Entao_LancaBusinessException()
    {
        // Arrange & Act
        var action = () => new GameReview(Guid.NewGuid(), Guid.Empty, 5, "Comentário válido");

        // Assert
        var exception = Should.Throw<BusinessException>(action);
        exception.Message.ShouldBe("O identificador do usuário é obrigatório.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Dado_NotaForaDoIntervalo_Quando_CriarGameReview_Entao_LancaBusinessException(int notaInvalida)
    {
        // Arrange & Act
        var action = () => new GameReview(Guid.NewGuid(), Guid.NewGuid(), notaInvalida, "Comentário válido");

        // Assert
        var exception = Should.Throw<BusinessException>(action);
        exception.Message.ShouldBe("A nota da avaliação deve estar entre 1 e 5.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Dado_ComentarioVazio_Quando_CriarGameReview_Entao_LancaBusinessException(string comentarioInvalido)
    {
        // Arrange & Act
        // Usamos pragma warning disable para permitir passar null explicitamente no teste
#pragma warning disable CS8604 // Possível argumento de referência nula.
        var action = () => new GameReview(Guid.NewGuid(), Guid.NewGuid(), 5, comentarioInvalido);
#pragma warning restore CS8604

        // Assert
        var exception = Should.Throw<BusinessException>(action);
        exception.Message.ShouldBe("O comentário da avaliação é obrigatório.");
    }

    [Fact]
    public void Dado_ComentarioComMaisDe1000Caracteres_Quando_CriarGameReview_Entao_LancaBusinessException()
    {
        // Arrange
        var comentarioLongo = new string('a', 1001);

        // Act
        var action = () => new GameReview(Guid.NewGuid(), Guid.NewGuid(), 5, comentarioLongo);

        // Assert
        var exception = Should.Throw<BusinessException>(action);
        exception.Message.ShouldBe("O comentário deve possuir no máximo 1000 caracteres.");
    }

    [Fact]
    public void Dado_ReviewExistente_Quando_Atualizar_Entao_AtualizaCamposEMarcaComoAtualizado()
    {
        // Arrange
        var review = CreateValidGameReview();

        // Act
        review.Atualizar(3, " Razoável ");

        // Assert
        review.Nota.ShouldBe(3);
        review.Comentario.ShouldBe("Razoável");
        review.UpdatedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Dado_NotaInvalidaNaAtualizacao_Quando_Atualizar_Entao_LancaBusinessException(int notaInvalida)
    {
        // Arrange
        var review = CreateValidGameReview();

        // Act
        var action = () => review.Atualizar(notaInvalida, "Comentário válido");

        // Assert
        Should.Throw<BusinessException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Dado_ComentarioInvalidoNaAtualizacao_Quando_Atualizar_Entao_LancaBusinessException(string comentarioInvalido)
    {
        // Arrange
        var review = CreateValidGameReview();

        // Act
        var action = () => review.Atualizar(5, comentarioInvalido);

        // Assert
        Should.Throw<BusinessException>(action);
    }
}
