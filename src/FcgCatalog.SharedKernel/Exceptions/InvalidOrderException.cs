namespace FcgCatalog.SharedKernel.Exceptions;

public sealed class InvalidOrderException(string message): BusinessException(message);
