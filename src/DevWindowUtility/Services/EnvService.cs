using System.Collections;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class EnvService
{
    public IReadOnlyList<EnvVariable> Get(EnvironmentVariableTarget target)
    {
        var scope = target.ToString();
        return Environment.GetEnvironmentVariables(target)
            .Cast<DictionaryEntry>()
            .Select(entry => new EnvVariable(
                entry.Key?.ToString() ?? "",
                entry.Value?.ToString() ?? "",
                scope))
            .OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
