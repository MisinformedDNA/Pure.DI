﻿namespace Pure.DI.Tests;

using NS35EBD81B;

public class TableTests
{
    [Fact]
    public void ShouldProvideTryGet()
    {
        // Given
        const int count = 10000;
        var pairs = (
            from index in Enumerable.Range(-count, count * 2)
            select new Pair<string, long>(index.ToString(), index)).ToArray();

        // When
        var table = new Table<string, long>(pairs);

        // Then
        for (var index = -count; index < count; index++)
        {
            table.Get(index.ToString()).ShouldBe(index);
        }

        table.Get(count.ToString()).ShouldBe(0);
    }

    [Fact]
    public void ShouldProvideTryGetWhenEmpty()
    {
        // Given

        // When
        var table = new Table<string, string>(Array.Empty<Pair<string, string>>());

        // Then
        table.Get("aa").ShouldBeNull();
    }
}