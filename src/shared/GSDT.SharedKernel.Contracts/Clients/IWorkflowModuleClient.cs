namespace GSDT.SharedKernel.Contracts.Clients;

/// <summary>
/// Cross-module interface for workflow operations.
/// Monolith: InProcessWorkflowModuleClient (repository call).
/// Microservice: gRPC client when Workflow module extracted.
/// </summary>
public interface IWorkflowModuleClient
{
    /// <summary>Create a new workflow instance for a business entity.</summary>
    Task<WorkflowInstanceResult> CreateInstanceAsync(
        CreateWorkflowInstanceRequest request,
        CancellationToken ct = default);
}

public record CreateWorkflowInstanceRequest(
    Guid DefinitionId,
    string EntityType,
    Guid EntityId,
    Guid TenantId,
    Guid CreatedBy);

public record WorkflowInstanceResult(
    Guid InstanceId,
    Guid InitialStateId);
