
namespace GSDT.Identity.Application.Queries.GetJitProviderConfigByScheme;

public sealed record GetJitProviderConfigBySchemeQuery(string Scheme) : IQuery<JitProviderConfigDto>;
