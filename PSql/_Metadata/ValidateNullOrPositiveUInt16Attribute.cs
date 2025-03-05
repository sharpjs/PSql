// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

// TODO: Document
#pragma warning disable CS1591

namespace PSql;

public class ValidateNullOrPositiveUInt16Attribute : ValidateArgumentsAttribute
{
    protected override void Validate(object arg, EngineIntrinsics engine)
    {
        if (arg is null)
            return;

        if (arg is PSObject psObject)
            arg = psObject.BaseObject;

        var value = LanguagePrimitives.ConvertTo<ushort>(arg);

        if (value < 1)
            throw new ValidationMetadataException(string.Format(
                @"The value ""{0}"" is not a positive number.", arg
            ));
    }
}
