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
        var game = CreateValidGame();

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
        Should.Throw<InvalidGameException>(() => new Game(nome, "desc", 10m, DateTime.Today, ClassificacaoEtaria.Livre));
    }

    [Fact]
    public void Dado_PrecoZero_Quando_CriarJogo_Entao_LancaExcecao()
    {
        Should.Throw<InvalidGameException>(() => new Game("X", "desc", 0m, DateTime.Today, ClassificacaoEtaria.Livre));
    }

    [Fact]
    public void Dado_PrecoAcimaDoLimite_Quando_CriarJogo_Entao_LancaExcecao()
    {
        Should.Throw<InvalidGameException>(() => new Game("X", "desc", 10000m, DateTime.Today, ClassificacaoEtaria.Livre));
    }

    [Fact]
    public void Dado_JogoExistente_Quando_AlterarPreco_Entao_AtualizaPrecoEMarcaComoAtualizado()
    {
        var game = CreateValidGame();

        game.AlterarPreco(149.90m);

        game.Preco.ShouldBe(149.90m);
        game.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Dado_PrecoIgualAoAtual_Quando_AlterarPreco_Entao_NaoAtualizaTimestamp()
    {
        var game = CreateValidGame();
        var precoAnterior = game.Preco;

        game.AlterarPreco(precoAnterior);

        game.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Dado_NovoPrecoInvalido_Quando_AlterarPreco_Entao_LancaExcecao()
    {
        var game = CreateValidGame();

        Should.Throw<InvalidGameException>(() => game.AlterarPreco(-1m));
    }

    [Fact]
    public void Dado_JogoAtivo_Quando_Inativar_Entao_FicaInativo()
    {
        var game = CreateValidGame();

        game.Inativar();

        game.Inativo.ShouldBeTrue();
        game.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Dado_JogoJaInativo_Quando_Inativar_Entao_NaoAtualizaTimestamp()
    {
        var game = CreateValidGame();
        game.Inativar();
        var primeiraInativacao = game.UpdatedAt;

        game.Inativar();

        game.UpdatedAt.ShouldBe(primeiraInativacao);
    }

    [Fact]
    public void Dado_NovosDados_Quando_Atualizar_Entao_AtualizaCampos()
    {
        var game = CreateValidGame();

        game.Atualizar("Halo Infinite", "Sequel da franquia.", new DateTime(2021, 12, 8), ClassificacaoEtaria.Dezesseis);

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
        var game = new Game("X", "desc", 10m, DateTime.Today, classificacao);

        game.PodeSerJogadoPor(idade).ShouldBe(esperado);
    }
}
