namespace FcgCatalog.SharedKernel.Exceptions;

public sealed class NotFoundException(string message) : BusinessException(message);
