﻿// ReSharper disable ClassNeverInstantiated.Global
namespace Pure.DI.Core;

using System.IO;

internal class Settings : ISettings
{
    private readonly IBuildContext _buildContext;
    private readonly IFileSystem _fileSystem;

    public Settings(
        IBuildContext buildContext,
        IFileSystem fileSystem)
    {
        _buildContext = buildContext;
        _fileSystem = fileSystem;
    }

    public bool Debug => GetBool(Setting.Debug);

    public bool Trace => GetBool(Setting.Trace);

    public bool TryGetOutputPath(out string outputPath)
    {
        if (!TryGet(Setting.Out, out outputPath))
        {
            outputPath = string.Empty;
            return false;
        }

        outputPath = EnsureExists(Path.Combine(outputPath, _buildContext.Metadata.ComposerTypeName));
        return true;
    }

    public Verbosity Verbosity =>
        TryGet(Setting.Verbosity, out var verbosityValue)
        && Enum.TryParse(verbosityValue, true, out Verbosity verbosity)
            ? verbosity
            : Verbosity.Quiet;

    public bool TryGetLogFile(out string logFilePath)
    {
        if (Verbosity > Verbosity.Quiet)
        {
            logFilePath = GetFullPath($"{_buildContext.Metadata.ComposerTypeName}.log");
            return true;
        }

        logFilePath = string.Empty;
        return false;
    }

    private bool TryGet(Setting setting, out string value)
    {
        var settings = _buildContext.Metadata.Settings;
        return settings.TryGetValue(setting, out value);
    }

    private bool GetBool(Setting setting, bool defaultValue = false) =>
        TryGet(setting, out var valueStr) && bool.TryParse(valueStr, out var value)
            ? value
            : defaultValue;

    private string GetFullPath(string path)
    {
        if (TryGetOutputPath(out var outputPath))
        {
            path = Path.Combine(outputPath, path);
        }

        return path;
    }

    private string EnsureExists(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), path);
        }

        _fileSystem.CreateDirectory(path);
        return path;
    }
}