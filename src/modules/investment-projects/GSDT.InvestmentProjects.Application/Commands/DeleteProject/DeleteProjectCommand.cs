namespace GSDT.InvestmentProjects.Application.Commands.DeleteProject;

/// <summary>
/// Soft-deletes a project (domestic or ODA) and all child entities via cascade.
/// Works for both DomesticProject and OdaProject — discriminated by domain type check.
/// </summary>
public sealed record DeleteProjectCommand(Guid Id) : ICommand;
