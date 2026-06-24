namespace FcgCatalog.SharedKernel.Exceptions;

public sealed class InvalidAddressException(string message): BusinessException(message);
