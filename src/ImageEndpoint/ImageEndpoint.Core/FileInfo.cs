namespace ImageEndpoint.Core;

public record FileInfo(
    string FileName,
    string Format,
    long SizeBytes,
    DateTimeOffset LastModified
);