// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Validates that a parameter is either <see langword="null"/> or a positive
///   <see langword="ushort"/> value.
/// </summary>
/// <remarks>
///   Valid values range from <c>1</c> to <c>65535</c>.
/// </remarks>
public class ValidateNullOrPositiveUInt16Attribute : ValidateArgumentsAttribute
{
    /// <inheritdoc/>
    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
        // NOTE: 'arguments' is a single argument, despite the plural name.

        if (arguments is null)
            return;

        arguments = arguments.UnwrapPSObject();

        var value = LanguagePrimitives.ConvertTo<ushort>(arguments);

        if (value < 1)
            throw new ValidationMetadataException(string.Format(
                @"The value ""{0}"" is not a number between 1 and 65535, inclusive.", arguments
            ));
    }
}
