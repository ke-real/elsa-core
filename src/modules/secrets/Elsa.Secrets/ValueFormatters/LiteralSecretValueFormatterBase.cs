using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elsa.Secrets.Models;

namespace Elsa.Secrets.ValueFormatters;

public abstract class LiteralSecretValueFormatterBase : ISecretValueFormatter {
    public abstract string Type { get; }
    public virtual Task<string> FormatSecretValue(Secret secret) => Task.FromResult(ConvertPropertiesToString(secret.Properties));

    private static string ConvertPropertiesToString(ICollection<SecretProperty> properties)
    {
        var sb = new StringBuilder();

        foreach (var property in properties.Where(x => x.Expressions.Count > 0))
        {
            sb.Append($"{property.Expressions.First().Value}");
        }

        return sb.ToString();
    }
}