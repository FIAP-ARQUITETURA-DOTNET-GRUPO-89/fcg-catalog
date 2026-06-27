using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Exceptions;

namespace FcgCatalog.Domain.Entities;

public class Game : BaseEntity
{
    protected Game() { }

    public Game(string nome, string descricao, decimal preco, DateTime dataLancamento, ClassificacaoEtaria classificacaoEtaria)
    {
        ValidarNome(nome);
        ValidarDescricao(descricao);
        ValidarPreco(preco);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
        DataLancamento = dataLancamento;
        ClassificacaoEtaria = classificacaoEtaria;
        Inativo = false;
    }

    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public DateTime DataLancamento { get; private set; }
    public ClassificacaoEtaria ClassificacaoEtaria { get; private set; }
    public bool Inativo { get; private set; }

    public void Atualizar(string nome, string descricao, DateTime dataLancamento, ClassificacaoEtaria classificacaoEtaria)
    {
        ValidarNome(nome);
        ValidarDescricao(descricao);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        DataLancamento = dataLancamento;
        ClassificacaoEtaria = classificacaoEtaria;

        MarkAsUpdated();
    }

    public void AlterarPreco(decimal novoPreco)
    {
        ValidarPreco(novoPreco);

        if (Preco == novoPreco)
        {
            return;
        }

        Preco = novoPreco;
        MarkAsUpdated();
    }

    public void Inativar()
    {
        if (Inativo)
        {
            return;
        }

        Inativo = true;
        MarkAsUpdated();
    }

    public bool PodeSerJogadoPor(int idade)
        => idade >= (int)ClassificacaoEtaria;

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidGameException("O nome do jogo é obrigatório.");
        }

        if (nome.Length > 100)
        {
            throw new InvalidGameException("O nome do jogo deve possuir no máximo 100 caracteres.");
        }
    }

    private static void ValidarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new InvalidGameException("A descrição do jogo é obrigatória.");
        }

        if (descricao.Length > 500)
        {
            throw new InvalidGameException("A descrição do jogo deve possuir no máximo 500 caracteres.");
        }
    }

    private static void ValidarPreco(decimal preco)
    {
        if (preco <= 0)
        {
            throw new InvalidGameException("O preço do jogo deve ser maior que zero.");
        }

        if (preco > 9999.99m)
        {
            throw new InvalidGameException("O preço do jogo não pode ser superior a R$ 9.999,99.");
        }
    }
}
