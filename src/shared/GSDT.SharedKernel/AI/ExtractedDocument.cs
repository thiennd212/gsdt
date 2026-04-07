namespace GSDT.SharedKernel.AI;

/// <summary>Result of AI document extraction (OCR + field parsing).</summary>
public sealed record ExtractedDocument(
    string Title,
    string Content,
    IReadOnlyDictionary<string, string> Metadata);
