﻿// ReSharper disable ClassNeverInstantiated.Global
namespace Pure.DI.Core;

internal class Generator : IGenerator
{
    private readonly IMetadataBuilder _metadataBuilder;
    private readonly ISourceBuilder _sourceBuilder;
    private readonly ISettings _settings;
    private readonly IDiagnostic _diagnostic;

    public Generator(
        IMetadataBuilder metadataBuilder,
        ISourceBuilder sourceBuilder,
        ISettings settings,
        IDiagnostic diagnostic)
    {
        _metadataBuilder = metadataBuilder;
        _sourceBuilder = sourceBuilder;
        _settings = settings;
        _diagnostic = diagnostic;
    }

    public void Generate(IExecutionContext context)
    {
        var workingThread = new Thread(() => GenerateInternal(context), 0xff_ffff)
        {
            Name = "Pure.DI",
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };

        workingThread.Start();
        while (!workingThread.Join(10) && !context.CancellationToken.IsCancellationRequested)
        {
        }

        if (context.CancellationToken.IsCancellationRequested && !workingThread.Join(1))
        {
            workingThread.Abort();
        }
    }

    private void GenerateInternal(IExecutionContext context)
    {
        if (_diagnostic is CompilationDiagnostic compilationDiagnostic)
        {
            compilationDiagnostic.Context = context;
        }

        try
        {
            var metadata = _metadataBuilder.Build(context.Compilation, context.CancellationToken);
            foreach (var source in _sourceBuilder.Build(metadata))
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                context.AddSource(source.HintName, source.Code);
            }

            if (_settings.Api)
            {
                foreach (var source in metadata.Api)
                {
                    context.AddSource(source.HintName, source.Code);
                }
            }
        }
        catch (BuildException buildException)
        {
            _diagnostic.Error(buildException.Id, buildException.Message, buildException.Location);
        }
        catch (HandledException)
        {
        }
        catch (Exception ex)
        {
            _diagnostic.Error(Diagnostics.Error.Unhandled, $"{ex}: {ex.StackTrace}");
        }
    }
}