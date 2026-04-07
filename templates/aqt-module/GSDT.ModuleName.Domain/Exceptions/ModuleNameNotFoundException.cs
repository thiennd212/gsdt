using GSDT.SharedKernel.Errors;

namespace GSDT.ModuleName.Domain.Exceptions;

public sealed class ModuleNameNotFoundException(Guid id)
    : NotFoundException($"ModuleName entity with ID '{id}' was not found.");
