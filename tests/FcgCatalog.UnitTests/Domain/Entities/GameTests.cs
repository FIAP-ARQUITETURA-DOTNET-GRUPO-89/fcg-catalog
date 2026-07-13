using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Exceptions;
using Shouldly;

namespace FcgCatalog.UnitTests.Domain.Entities;

public class GameTests
{
    private static Game CreateValidGame() =>
        new("Halo", "Jogo de tiro em primeira pessoa.", 199.90m, new DateTime(2001, 11, 15), ClassificacaoEtaria.Dezesseis);

    [Fact]
    public void Dado_DadosValidos_Quando_CriarJogo_Entao_PropriedadesSaoPreenchidas()
    {
        // Arrange

        // Act
        var game = CreateValidGame();

        // Assert
        game.Id.ShouldNotBe(Guid.Empty);
        game.Nome.ShouldBe("Halo");
        game.Descricao.ShouldBe("Jogo de tiro em primeira pessoa.");
        game.Preco.ShouldBe(199.90m);
        game.ClassificacaoEtaria.ShouldBe(ClassificacaoEtaria.Dezesseis);
        game.Inativo.ShouldBeFalse();
        game.UpdatedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Dado_NomeInvalido_Quando_CriarJogo_Entao_LancaExcecao(string nome)
    {
        // Arrange

        // Act
        var action = () => new Game(nome, "desc", 10m, DateTime.Today, ClassificacaoEtaria.Livre);

        // Assert
        Should.Throw<InvalidGameException>(action);
    }

    [Fact]
    public void Dado_PrecoZero_Quando_CriarJogo_Entao_LancaExcecao()
    {
        // Arrange

        // Act
        var action = () => new Game("X", "desc", 0m, DateTime.Today, ClassificacaoEtaria.Livre);

        // Assert
        Should.Throw<InvalidGameException>(action);
    }

    [Fact]
    public void Dado_PrecoAcimaDoLimite_Quando_CriarJogo_Entao_LancaExcecao()
    {
        // Arrange

        // Act
        var action = () => new Game("X", "desc", 10000m, DateTime.Today, ClassificacaoEtaria.Livre);

        // Assert
        Should.Throw<InvalidGameException>(action);
    }

    [Fact]
    public void Dado_JogoExistente_Quando_AlterarPreco_Entao_AtualizaPrecoEMarcaComoAtualizado()
    {
        // Arrange
        var game = CreateValidGame();

        // Act
        game.AlterarPreco(149.90m);

        // Assert
        game.Preco.ShouldBe(149.90m);
        game.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Dado_PrecoIgualAoAtual_Quando_AlterarPreco_Entao_NaoAtualizaTimestamp()
    {
        // Arrange
        var game = CreateValidGame();
        var precoAnterior = game.Preco;

        // Act
        game.AlterarPreco(precoAnterior);

        // Assert
        game.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Dado_NovoPrecoInvalido_Quando_AlterarPreco_Entao_LancaExcecao()
    {
        // Arrange
        var game = CreateValidGame();

        // Act
        var action = () => game.AlterarPreco(-1m);

        // Assert
        Should.Throw<InvalidGameException>(action);
    }

    [Fact]
    public void Dado_JogoAtivo_Quando_Inativar_Entao_FicaInativo()
    {
        // Arrange
        var game = CreateValidGame();

        // Act
        game.Inativar();

        // Assert
        game.Inativo.ShouldBeTrue();
        game.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Dado_JogoJaInativo_Quando_Inativar_Entao_NaoAtualizaTimestamp()
    {
        // Arrange
        var game = CreateValidGame();
        game.Inativar();
        var primeiraInativacao = game.UpdatedAt;

        // Act
        game.Inativar();

        // Assert
        game.UpdatedAt.ShouldBe(primeiraInativacao);
    }

    [Fact]
    public void Dado_NovosDados_Quando_Atualizar_Entao_AtualizaCampos()
    {
        // Arrange
        var game = CreateValidGame();

        // Act
        game.Atualizar(
            "Halo Infinite",
            "Sequel da franquia.",
            new DateTime(2021, 12, 8),
            ClassificacaoEtaria.Dezesseis);

        // Assert
        game.Nome.ShouldBe("Halo Infinite");
        game.Descricao.ShouldBe("Sequel da franquia.");
        game.UpdatedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(18, ClassificacaoEtaria.Dezoito, true)]
    [InlineData(17, ClassificacaoEtaria.Dezoito, false)]
    [InlineData(0, ClassificacaoEtaria.Livre, true)]
    public void Dado_Idade_Quando_PodeSerJogadoPor_Entao_RetornaResultadoEsperado(int idade, ClassificacaoEtaria classificacao, bool esperado)
    {
        // Arrange
        var game = new Game("X", "desc", 10m, DateTime.Today, classificacao);

        // Act
        var result = game.PodeSerJogadoPor(idade);

        // Assert
        result.ShouldBe(esperado);
    }
}
