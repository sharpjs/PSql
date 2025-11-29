// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

[TestFixture]
public class ObjectExtensionsTests
{
    [Test]
    public void UnwrapPSObject_Null()
    {
        (null as object).UnwrapPSObject().ShouldBeNull();
    }

    [Test]
    public void UnwrapPSObject_NonPSObject()
    {
        var obj = new object();

        obj.UnwrapPSObject().ShouldBeSameAs(obj);
    }

    [Test]
    public void UnwrapPSObject_PSObject()
    {
        var baseObject = new object();
        var psObject   = new PSObject(baseObject);

        psObject.UnwrapPSObject().ShouldBeSameAs(baseObject);
    }
}
