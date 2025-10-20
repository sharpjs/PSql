// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   A strategy to build objects dynamically.
/// </summary>
/// <typeparam name="T">
///   The type of objects that the builder can build.
/// </typeparam>
public interface IObjectBuilder<T>
{
    /// <summary>
    ///   Creates a new object.
    /// </summary>
    /// <returns>
    ///   A new instance of type <typeparamref name="T"/>.
    /// </returns>
    T NewObject();

    /// <summary>
    ///   Adds a property to the specified object.
    /// </summary>
    /// <param name="obj">
    ///   The object to which to add the property.
    /// </param>
    /// <param name="name">
    ///   The name of the property to add.
    /// </param>
    /// <param name="value">
    ///   The value of the property to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="obj"/> or
    ///   <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    void AddProperty(T obj, string name, object? value);
}
