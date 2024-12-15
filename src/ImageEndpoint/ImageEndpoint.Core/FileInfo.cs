namespace ImageEndpoint.Core;

public record FileInfo(
    string FileName,
    ImageFileFormat Format,
    long SizeBytes,
    DateTimeOffset LastModified
);