/*
    Copyright 2021 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Management.Automation;

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
