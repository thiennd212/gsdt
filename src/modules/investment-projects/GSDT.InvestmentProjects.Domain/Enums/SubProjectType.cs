namespace GSDT.InvestmentProjects.Domain.Enums;

/// <summary>Whether a domestic project is a standalone, sub-project, or inter-provincial.</summary>
public enum SubProjectType
{
    NotSubProject = 0,
    IsSubProject = 1,
    InterProvincial = 2
}
