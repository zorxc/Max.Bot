using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents a contact.
/// </summary>
public class Contact
{
    /// <summary>
    /// Gets or sets contact information in VCF format.
    /// </summary>
    /// <value>Contact information in VCF format.</value>
    [JsonPropertyName("vcf_info")]
    public string? VcfInfo { get; set; }

    /// <summary>
    /// Gets or sets contact information.
    /// </summary>
    /// <value>Contact information.</value>
    [JsonPropertyName("max_info")]
    public ContactInfo? MaxInfo { get; set; }
}
