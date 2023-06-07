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

namespace PSql;

using static FormattableString;

internal sealed class ObjectResultSet : IEnumerator<object>
{
    private readonly SqlDataReader                    _reader;
    private readonly Func   <object>                  _createObject;
    private readonly Action <object, string, object?> _setProperty;
    private readonly bool                             _useSqlTypes;

    private object?   _current;
    private string[]? _columnNames;

    public ObjectResultSet(
        SqlDataReader                    reader,
        Func   <object>                  createObject,
        Action <object, string, object?> setProperty,
        bool                             useSqlTypes)
    {
        _reader       = reader       ?? throw new ArgumentNullException(nameof(reader));
        _createObject = createObject ?? throw new ArgumentNullException(nameof(createObject));
        _setProperty  = setProperty  ?? throw new ArgumentNullException(nameof(setProperty));
        _useSqlTypes  = useSqlTypes;
    }

    public object Current
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

    private object ProjectToObject()
    {
        var obj = _createObject();

        for (var i = 0; i < _columnNames!.Length; i++)
        {
            var name  = _columnNames[i];
            var value = GetValue(_reader, i, _useSqlTypes);
            _setProperty(obj, name, value);
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

