// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

public static class SqlErrorHandling
{
    public static string Apply(IEnumerable<string> batches)
    {
        var builder = new SqlErrorHandlingBuilder();

        foreach (var batch in batches)
        {
            builder.StartNewBatch();
            builder.Append(batch);
        }

        return builder.Complete();
    }
}
