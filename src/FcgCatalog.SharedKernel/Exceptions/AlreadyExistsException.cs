namespace FcgCatalog.SharedKernel.Exceptions;

public sealed class AlreadyExistsException(string message) : BusinessException(message);
