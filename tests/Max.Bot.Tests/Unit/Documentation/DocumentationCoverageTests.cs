using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Max.Bot;
using Max.Bot.Api;

namespace Max.Bot.Tests.Unit.Documentation;

/// <summary>
/// Provides smoke tests that ensure XML documentation is generated for the main library.
/// </summary>
public class DocumentationCoverageTests
{
    private static readonly string DocumentationPath = Path.ChangeExtension(typeof(MaxClient).Assembly.Location, ".xml")!;
    private static readonly Lazy<XDocument> Documentation = new(() => XDocument.Load(DocumentationPath));

    /// <summary>
    /// Ensures that the XML documentation file is generated next to the compiled assembly.
    /// </summary>
    [Fact]
    public void XmlDocumentation_ShouldExist()
    {
        System.IO.File.Exists(DocumentationPath).Should().BeTrue($"XML documentation should be generated at {DocumentationPath}");
    }

    /// <summary>
    /// Ensures that critical public members contain non-empty summaries aligned with API docs.
    /// </summary>
    /// <param name="memberName">The fully-qualified XML documentation member name.</param>
    [Theory]
    [InlineData("T:Max.Bot.MaxClient")]
    [InlineData("T:Max.Bot.Api.IMaxBotApi")]
    [InlineData("T:Max.Bot.Api.IMessagesApi")]
    [InlineData("T:Max.Bot.Polling.IUpdateHandler")]
    [InlineData("T:Max.Bot.Types.Message")]
    public void XmlDocumentation_ShouldContainSummary_ForCriticalMembers(string memberName)
    {
        var summary = GetSummary(memberName);

        summary.Should().NotBeNullOrWhiteSpace($"member {memberName} is part of the public API surface");
    }

    private static string? GetSummary(string memberName)
    {
        var document = Documentation.Value;
        var member = document.Descendants("member")
            .FirstOrDefault(node => string.Equals(node.Attribute("name")?.Value, memberName, StringComparison.Ordinal));

        return member?.Element("summary")?.Value.Trim();
    }
}


