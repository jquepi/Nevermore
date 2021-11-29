﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Nevermore.Querying.AST
{
    public interface ISelectColumns
    {
        bool AggregatesRows { get; }
        string GenerateSql();
    }

    public class AggregateSelectColumns : ISelectColumns
    {
        readonly IReadOnlyList<ISelectColumns> columns;

        public AggregateSelectColumns(IReadOnlyList<ISelectColumns> columns)
        {
            this.columns = columns;
        }

        public bool AggregatesRows => columns.Any(c => c.AggregatesRows);

        public string GenerateSql() => string.Join(@",
", columns.Select(c => c.GenerateSql()));
        public override string ToString() => GenerateSql();
    }

    public class AliasedColumn : ISelectColumns
    {
        readonly IColumn column;
        readonly string columnAlias;

        public AliasedColumn(IColumn column, string columnAlias)
        {
            this.column = column;
            this.columnAlias = columnAlias;
        }

        public bool AggregatesRows => false;
        public string GenerateSql() => $"{column.GenerateSql()} AS [{columnAlias}]";
        public override string ToString() => GenerateSql();
    }

    public class SelectAllFromJsonColumnLast : ISelectColumns
    {
        readonly string tableAlias;
        readonly IReadOnlyList<string> columnNames;

        public SelectAllFromJsonColumnLast(string tableAlias, IReadOnlyList<string> columnNames)
        {
            this.tableAlias = tableAlias;
            this.columnNames = columnNames.OrderBy(x => x == "JSON").ToList();
        }

        public bool AggregatesRows => false;

        public string GenerateSql() => string.Join(',', columnNames.Select(x => $"{tableAlias}.{x}").ToArray());
        
        public override string ToString() => GenerateSql();
    }

    public class SelectAllSource : ISelectColumns
    {
        public bool AggregatesRows => false;
        public string GenerateSql() => "*";
        public override string ToString() => GenerateSql();
    }

    public class SelectAllJsonColumnLast : ISelectColumns
    {
        readonly IReadOnlyList<string> columnNames;

        public SelectAllJsonColumnLast(IReadOnlyList<string> columnNames)
        {
            this.columnNames = columnNames.OrderBy(x => x == "JSON").ToList();
        }
        
        public bool AggregatesRows => false;
        public string GenerateSql() => string.Join(',', columnNames);
    }

    public class SelectCountSource : ISelectColumns
    {
        public bool AggregatesRows => true;
        public string GenerateSql() => "COUNT(*)";
        public override string ToString() => GenerateSql();
    }

    public class SelectRowNumber : ISelectColumns
    {
        readonly Over over;
        readonly string alias;

        public SelectRowNumber(Over over, string alias)
        {
            this.over = over;
            this.alias = alias;
        }

        public bool AggregatesRows => false;

        public string GenerateSql() => $"ROW_NUMBER() {over.GenerateSql()} AS {alias}";
        public override string ToString() => GenerateSql();
    }
}