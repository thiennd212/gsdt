
namespace GSDT.SharedKernel.Domain;

/// <summary>Marker interface for DDD aggregate roots with domain event collection.</summary>
public interface IAggregateRoot
{
    void ClearDomainEvents();
}
