// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Globalization;

namespace PSql;

internal sealed class ObjectResultSet<T> : IEnumerator<T>
{
    private readonly SqlConnection     _connection;
    private readonly SqlDataReader     _reader;
    private readonly IObjectBuilder<T> _builder;
    private readonly bool              _useSqlTypes;

    private T?        _current;
    private string[]? _columnNames;

    public ObjectResultSet(
        SqlConnection     connection,
        SqlDataReader     reader,
        IObjectBuilder<T> builder,
        bool              useSqlTypes)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(builder);

        _connection  = connection;
        _reader      = reader;
        _builder     = builder;
        _useSqlTypes = useSqlTypes;
    }

    public T Current
        => _current ?? throw OnNoCurrentItem();

    object? IEnumerator.Current
        => Current;

    public bool MoveNext()
    {
        while (!_reader.Read())
        {
            if (!_reader.NextResult())
                return SetNoCurrent();

            _columnNames = null;
        }

        return SetCurrent();
    }

    private bool SetCurrent()
    {
        _current = ProjectToObject();
        return true;
    }

    private bool SetNoCurrent()
    {
        _current = default;
        _connection.ThrowIfHasErrors();
        return false;
    }

    private T ProjectToObject()
    {
        _columnNames ??= GetColumnNames(_reader);

        var obj = _builder.NewObject();

        for (var i = 0; i < _columnNames.Length; i++)
        {
            var name  = _columnNames[i];
            var value = GetValue(_reader, i, _useSqlTypes);

            _builder.AddProperty(obj, name, value);
        }

        return obj;
    }

    private static string[] GetColumnNames(SqlDataReader reader)
    {
        var names = new string[reader.FieldCount];

        for (var i = 0; i < names.Length; i++)
            names[i] = reader.GetName(i).NullIfEmpty() ?? GetDefaultColumnName(i);

        return names;
    }

    private static string GetDefaultColumnName(int i)
    {
        return string.Create(CultureInfo.InvariantCulture, $"Col{i}");
    }

    private static object? GetValue(SqlDataReader reader, int ordinal, bool useSqlTypes)
    {
        var value = useSqlTypes
            ? reader.GetSqlValue (ordinal)
            : reader.GetValue    (ordinal);

        return value is DBNull
            ? null
            : value;
    }

    void IEnumerator.Reset()
        => throw new NotSupportedException();

    public void Dispose()
    {
        _reader.Dispose();
    }

    private static Exception OnNoCurrentItem()
    {
        return new InvalidOperationException(
            "The result set does not have a current item."
        );
    }
}
