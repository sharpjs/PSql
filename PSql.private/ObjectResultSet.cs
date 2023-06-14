// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;

namespace PSql;

using static FormattableString;

internal sealed class ObjectResultSet : IEnumerator<PSObject>
{
    private readonly SqlDataReader _reader;
    private readonly bool          _useSqlTypes;

    private PSObject? _current;
    private string[]? _columnNames;

    public ObjectResultSet(SqlDataReader reader, bool useSqlTypes)
    {
        if (reader is null)
            throw new ArgumentNullException(nameof(reader));

        _reader      = reader;
        _useSqlTypes = useSqlTypes;
    }

    object? IEnumerator.Current
        => _current;

    public PSObject Current
        => _current ?? throw OnNoCurrentItem();

    public bool MoveNext()
        => MoveNextCore() ? SetCurrent() : SetNoCurrent();

    public void Reset()
        => throw new NotSupportedException();

    public void Dispose()
        => _reader.Dispose();

    private bool MoveNextCore()
    {
        while (!_reader.Read())
        {
            if (!_reader.NextResult())
                return false;

            _columnNames = null;
        }

        return true;
    }

    private bool SetCurrent()
    {
        _columnNames ??= GetColumnNames(_reader);
        _current       = ProjectToObject();
        return true;
    }

    private bool SetNoCurrent()
    {
        _current = null;
        return false;
    }

    private static string[] GetColumnNames(SqlDataReader reader)
    {
        var names = new string[reader.FieldCount];

        for (var i = 0; i < names.Length; i++)
            names[i] = reader.GetName(i).NullIfEmpty() ?? Invariant($"Col{i}");

        return names;
    }

    private PSObject ProjectToObject()
    {
        var obj = new PSObject();

        for (var i = 0; i < _columnNames!.Length; i++)
        {
            var name     = _columnNames[i];
            var value    = GetValue(_reader, i, _useSqlTypes);
            var property = new PSNoteProperty(name, value);

            obj.Properties.Add(property);
        }

        return obj;
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

    private static Exception OnNoCurrentItem()
    {
        return new InvalidOperationException(
            "The " + nameof(ObjectResultSet) + " does not have a current item."
        );
    }
}
