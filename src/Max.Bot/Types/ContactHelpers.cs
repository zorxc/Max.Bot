using System;
using System.Text.RegularExpressions;

namespace Max.Bot.Types;

/// <summary>
/// Helper methods for working with contact information.
/// </summary>
public static class ContactHelpers
{
    // Pre-compiled regex patterns for performance
    private static readonly Regex TelRegex = new(@"TEL(?:;[^:\r\n]*)?:(.+?)(?:\r?\n|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex FnRegex = new(@"FN(?:;[^:\r\n]*)?:(.+?)(?:\r?\n|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex NRegex = new(@"N:(.+?)(?:\r?\n|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Parses vCard (VCF) data and extracts contact information.
    /// </summary>
    /// <param name="vcfInfo">The vCard data in VCF format.</param>
    /// <returns>Parsed contact information, or null if parsing fails.</returns>
    public static ContactInfo? ParseVcf(string? vcfInfo)
    {
        if (string.IsNullOrWhiteSpace(vcfInfo))
        {
            return null;
        }

        // Normalize line endings and unfold folded lines (vCard spec: lines starting with whitespace are continuations)
        var normalizedVcf = NormalizeVcf(vcfInfo);

        var contactInfo = new ContactInfo();

        // Extract phone number from TEL: field
        var telMatch = TelRegex.Match(normalizedVcf);
        if (telMatch.Success)
        {
            contactInfo.PhoneNumber = UnescapeVcfText(telMatch.Groups[1].Value.Trim());
        }

        // Extract full name from FN: field
        var fnMatch = FnRegex.Match(normalizedVcf);
        if (fnMatch.Success)
        {
            contactInfo.FullName = UnescapeVcfText(fnMatch.Groups[1].Value.Trim());
        }

        // Extract first and last name from N: field (N:Last;First;Middle;Prefix;Suffix)
        var nMatch = NRegex.Match(normalizedVcf);
        if (nMatch.Success)
        {
            var parts = nMatch.Groups[1].Value.Split(';');
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                contactInfo.LastName = UnescapeVcfText(parts[0].Trim());
            }
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                contactInfo.FirstName = UnescapeVcfText(parts[1].Trim());
            }
        }

        // If FN was not found but we have first and last name, construct full name
        if (string.IsNullOrEmpty(contactInfo.FullName))
        {
            var firstName = contactInfo.FirstName ?? string.Empty;
            var lastName = contactInfo.LastName ?? string.Empty;

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                contactInfo.FullName = $"{firstName} {lastName}";
            }
            else if (!string.IsNullOrEmpty(firstName))
            {
                contactInfo.FullName = firstName;
            }
            else if (!string.IsNullOrEmpty(lastName))
            {
                contactInfo.FullName = lastName;
            }
            // else: leave FullName as null (don't set empty string)
        }

        return contactInfo;
    }

    /// <summary>
    /// Normalizes vCard text by unfolding folded lines.
    /// vCard spec: lines starting with whitespace are continuations of the previous line.
    /// </summary>
    private static string NormalizeVcf(string vcf)
    {
        var lines = vcf.Replace("\r\n", "\n").Split('\n');
        var result = new System.Text.StringBuilder(vcf.Length);

        foreach (var line in lines)
        {
            if (line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
            {
                // Folded line - append to previous content (remove leading whitespace)
                result.Append(line.TrimStart());
            }
            else
            {
                if (result.Length > 0)
                {
                    result.Append('\n');
                }
                result.Append(line);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Unescapes vCard text characters.
    /// vCard spec: \n = newline, \; = semicolon, \, = comma, \\ = backslash
    /// </summary>
    private static string UnescapeVcfText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text
            .Replace("\\n", "\n")
            .Replace("\\N", "\n")
            .Replace("\\;", ";")
            .Replace("\\,", ",")
            .Replace("\\\\", "\\");
    }

    /// <summary>
    /// Gets the phone number from a contact attachment.
    /// First tries to get it from MaxInfo.PhoneNumber, then parses from VcfInfo.
    /// </summary>
    /// <param name="contact">The contact object.</param>
    /// <returns>The phone number, or null if not available.</returns>
    public static string? GetPhoneNumber(Contact? contact)
    {
        if (contact == null)
        {
            return null;
        }

        // Try to get from MaxInfo first
        if (!string.IsNullOrEmpty(contact.MaxInfo?.PhoneNumber))
        {
            return contact.MaxInfo.PhoneNumber;
        }

        // Parse from VcfInfo
        if (!string.IsNullOrEmpty(contact.VcfInfo))
        {
            var parsed = ParseVcf(contact.VcfInfo);
            return parsed?.PhoneNumber;
        }

        return null;
    }

    /// <summary>
    /// Gets the full name from a contact attachment.
    /// First tries to get it from MaxInfo.FullName, then parses from VcfInfo.
    /// </summary>
    /// <param name="contact">The contact object.</param>
    /// <returns>The full name, or null if not available.</returns>
    public static string? GetFullName(Contact? contact)
    {
        if (contact == null)
        {
            return null;
        }

        // Try to get from MaxInfo first
        if (!string.IsNullOrEmpty(contact.MaxInfo?.FullName))
        {
            return contact.MaxInfo.FullName;
        }

        // Parse from VcfInfo
        if (!string.IsNullOrEmpty(contact.VcfInfo))
        {
            var parsed = ParseVcf(contact.VcfInfo);
            return parsed?.FullName;
        }

        return null;
    }
}
