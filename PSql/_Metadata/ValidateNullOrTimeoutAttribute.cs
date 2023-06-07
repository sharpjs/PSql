// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

public class ValidateNullOrTimeoutAttribute : ValidateArgumentsAttribute
{
    private const long
        MinValueTicks =                  0L,
        MaxValueTicks = 2147483647_0000000L;

    protected override void Validate(object arg, EngineIntrinsics engine)
    {
        if (arg is null)
            return;

        if (arg is PSObject psObject)
            arg = psObject.BaseObject;

        var value = LanguagePrimitives.ConvertTo<TimeSpan>(arg);
        var ticks = value.Ticks;

        if (ticks < MinValueTicks)
            throw new ValidationMetadataException(string.Format(
                @"The value ""{0}"" is negative. Negative timeouts are not supported.", arg
            ));

        if (ticks > MaxValueTicks)
            throw new ValidationMetadataException(string.Format(
                @"The value ""{0}"" exceeds the maximum supported timeout, 24855.03:14:07.", arg
            ));
    }
}
