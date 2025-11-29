// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

[TestFixture]
public class SqlErrorHandlingTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    public void Apply_Null()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            SqlErrorHandling.Apply(null!);
        });
    }
}
