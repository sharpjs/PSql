// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Validates that a parameter is either <see langword="null"/> or a valid
///   timeout value.
/// </summary>
/// <remarks>
///   Valid timeouts range from <c>00:00:00</c> to <c>24855.03:14:07</c>.
/// </remarks>
public class ValidateNullOrTimeoutAttribute : ValidateArgumentsAttribute
{
    private const long
        MinValueTicks =            0 * TimeSpan.TicksPerSecond, //       00:00:00
        MaxValueTicks = int.MaxValue * TimeSpan.TicksPerSecond; // 24855.03:14:07

    /// <inheritdoc/>
    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
        // NOTE: 'arguments' is a single argument, despite the plural name.

        if (arguments is null)
            return;

        if (arguments is PSObject psObject)
            arguments = psObject.BaseObject;

        var value = LanguagePrimitives.ConvertTo<TimeSpan>(arguments);
        var ticks = value.Ticks;

        if (ticks < MinValueTicks)
            throw new ValidationMetadataException(string.Format(
                @"The value ""{0}"" is negative. Negative timeouts are not supported.", arguments
            ));

        if (ticks > MaxValueTicks)
            throw new ValidationMetadataException(string.Format(
                @"The value ""{0}"" exceeds the maximum supported timeout, 24855.03:14:07.", arguments
            ));
    }
}
