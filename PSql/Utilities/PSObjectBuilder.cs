// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

internal class PSObjectBuilder : E.IObjectBuilder<PSObject>
{
    public PSObject NewObject()
    {
        return new PSObject();
    }

    public void AddProperty(PSObject obj, string name, object? value)
    {
        obj.Properties.Add(new PSNoteProperty(name, value));
    }
}
