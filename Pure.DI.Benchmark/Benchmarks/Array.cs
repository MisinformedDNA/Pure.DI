﻿// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMember.Local
#pragma warning disable CA1822
namespace Pure.DI.Benchmark.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Model;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Array : BenchmarkBase
{
    private static void SetupDI() => DI.Setup()
        .Bind<ICompositionRoot>().To<CompositionRoot>()
        .Bind<IService1>().To<Service1>()
        .Bind<IService2>().To<Service2Array>()
        .Bind<IService3>().To<Service3>()
        .Bind<IService3>().Tags(2).To<Service3v2>()
        .Bind<IService3>().Tags(3).To<Service3v3>()
        .Bind<IService3>().Tags(4).To<Service3v4>();

    protected override TActualContainer CreateContainer<TActualContainer, TAbstractContainer>()
    {
        var abstractContainer = new TAbstractContainer();
        abstractContainer.Register(typeof(ICompositionRoot), typeof(CompositionRoot));
        abstractContainer.Register(typeof(IService1), typeof(Service1));
        abstractContainer.Register(typeof(IService2), typeof(Service2Array));
        abstractContainer.Register(typeof(IService3), typeof(Service3));
        abstractContainer.Register(typeof(IService3), typeof(Service3v2), AbstractLifetime.Transient, "2");
        abstractContainer.Register(typeof(IService3), typeof(Service3v3), AbstractLifetime.Transient, "3");
        abstractContainer.Register(typeof(IService3), typeof(Service3v4), AbstractLifetime.Transient, "4");
        return abstractContainer.TryCreate();
    }

    [Benchmark(Description = "Pure.DI")]
    public void PureDI() => ArrayDI.Resolve<ICompositionRoot>();

    [Benchmark(Description = "Pure.DI composition root")]
    public void PureDIByCR() => ArrayDI.ResolveICompositionRoot();

    [Benchmark(Description = "Hand Coded", Baseline = true)]
    public void HandCoded() => NewInstance();

    private static readonly Func<IService3> Service3Factory = () => new Service3();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static ICompositionRoot NewInstance() =>
        new CompositionRoot(
            new Service1(
                new Service2Array(new IService3[]
                {
                    new Service3(), new Service3v2(), new Service3v3(), new Service3v4()
                })),
            new Service2Array(new IService3[]
            {
                new Service3(), new Service3v2(), new Service3v3(), new Service3v4()
            }),
            new Service2Array(new IService3[]
            {
                new Service3(), new Service3v2(), new Service3v3(), new Service3v4()
            }),
            new Service2Array(new IService3[]
            {
                new Service3(), new Service3v2(), new Service3v3(), new Service3v4()
            }),
            new Service3());
}
#pragma warning restore CA1822