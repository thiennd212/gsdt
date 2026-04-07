using System.Security.Cryptography;
using System.Text;

namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>
/// Generates deterministic Guids for seed catalog data using MD5 hash of "{tableName}_{index}".
/// Ensures migration reproducibility — same input always yields same Guid.
/// </summary>
internal static class SeedCatalogHelper
{
    public static Guid SeedId(string tableName, int index)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"{tableName}_{index}"));
        return new Guid(bytes);
    }
}
