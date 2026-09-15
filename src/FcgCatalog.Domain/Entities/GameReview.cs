using FcgCatalog.SharedKernel.Exceptions;

namespace FcgCatalog.Domain.Entities;

public class GameReview : BaseEntity
{
    protected GameReview() { }

    public GameReview(Guid jogoId, Guid usuarioId, int nota, string comentario)
    {
        ValidarJogoId(jogoId);
        ValidarUsuarioId(usuarioId);
        ValidarNota(nota);
        ValidarComentario(comentario);

        JogoId = jogoId;
        UsuarioId = usuarioId;
        Nota = nota;
        Comentario = comentario.Trim();
    }

    public Guid JogoId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public int Nota { get; private set; }
    public string Comentario { get; private set; } = null!;

    public void Atualizar(int nota, string comentario)
    {
        ValidarNota(nota);
        ValidarComentario(comentario);

        Nota = nota;
        Comentario = comentario.Trim();

        MarkAsUpdated();
    }

    private static void ValidarJogoId(Guid jogoId)
    {
        if (jogoId == Guid.Empty)
        {
            throw new BusinessException("O identificador do jogo é obrigatório.");
        }
    }

    private static void ValidarUsuarioId(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new BusinessException("O identificador do usuário é obrigatório.");
        }
    }

    private static void ValidarNota(int nota)
    {
        if (nota < 1 || nota > 5)
        {
            throw new BusinessException("A nota da avaliação deve estar entre 1 e 5.");
        }
    }

    private static void ValidarComentario(string comentario)
    {
        if (string.IsNullOrWhiteSpace(comentario))
        {
            throw new BusinessException("O comentário da avaliação é obrigatório.");
        }

        if (comentario.Length > 1000)
        {
            throw new BusinessException("O comentário deve possuir no máximo 1000 caracteres.");
        }
    }
}
