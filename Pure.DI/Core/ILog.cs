﻿// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Global
namespace Pure.DI.Core;

internal interface ILog<T>
{
    void Trace(Func<string[]> messageFactory);

    void Info(Func<string[]> messageFactory);

    void Warning(params string[] warning);

    void Error(params string[] error);
}