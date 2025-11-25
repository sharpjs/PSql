// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;

namespace PSql;

[TestFixture]
public class SqlCmdPreprocessorTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    public void EnableVariableReplacementInComments_Get()
    {
        new SqlCmdPreprocessor()
            .EnableVariableReplacementInComments.ShouldBeFalse();
    }

    [Test]
    public void EnableVariableReplacementInComments_Set()
    {
        new SqlCmdPreprocessor { EnableVariableReplacementInComments = true }
            .EnableVariableReplacementInComments.ShouldBeTrue();
    }

    [Test]
    public void SetVariables_NullEntries()
    {
        var preprocessor = new SqlCmdPreprocessor();

        preprocessor.SetVariables(null);

        preprocessor.Process("a").ShouldBe(["a"]);
    }

    [Test]
    public void SetVariables_DictionaryEnumeratesUnexpectedType()
    {
        TestSetVariablesWith(entry: new object()); // Not a DictionaryEntry
    }

    [Test]
    public void SetVariables_EntryHasNullKey()
    {
        TestSetVariablesWith(entry: new DictionaryEntry(null!, "value"));
    }

    [Test]
    public void SetVariables_EntryHasEmptyKey()
    {
        TestSetVariablesWith(entry: new DictionaryEntry("", "value"));
    }

    [Test]
    public void SetVariables_EntryHasNullValue()
    {
        TestSetVariablesWith(entry: new DictionaryEntry("a", null));
    }

    [Test]
    public void SetVariables_EntryToStringIsNull()
    {
        TestSetVariablesWith(entry: new DictionaryEntry("a", new ThingWhoseToStringIsNull()));
    }

    private static void TestSetVariablesWith(object entry)
    {
        var preprocessor = new SqlCmdPreprocessor();

        var dictionary = new Mock<IDictionary>(MockBehavior.Strict);
        var enumerator = new Mock<IDictionaryEnumerator>(MockBehavior.Strict);
        var sequence   = new MockSequence();

        dictionary.InSequence(sequence).Setup(x => x.GetEnumerator()).Returns(enumerator.Object);
        enumerator.InSequence(sequence).Setup(x => x.MoveNext()).Returns(true);
        enumerator.InSequence(sequence).Setup(x => x.Current).Returns(entry);
        enumerator.InSequence(sequence).Setup(x => x.MoveNext()).Returns(false);

        preprocessor.SetVariables(dictionary.Object);

        preprocessor.Process("a").ShouldBe(["a"]);
    }

    private sealed class ThingWhoseToStringIsNull
    {
        public override string? ToString() => null;
    }
}
