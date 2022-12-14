using System.Collections.Generic;
using Nevermore.Querying;
using Nevermore.Querying.AST;

namespace Nevermore
{
    public interface ISelectBuilder
    {
        void AddTop(int top);

        void AddDistinct();
        
        void AddGroupBy(string fieldName);
        void AddGroupBy(string fieldName, string tableAlias);

        void AddOrder(OrderByField orderBy);
        void AddOrder(string fieldName, bool descending);
        void AddOrder(string fieldName, string tableAlias, bool descending);
        void AddWhere(UnaryWhereParameter whereParams);
        void AddWhere(BinaryWhereParameter whereParams);
        void AddWhere(ArrayWhereParameter whereParams);
        void AddWhere(IsNullWhereParameter isNull);
        void AddWhere(string whereClause);
        void AddColumn(string columnName);
        void AddColumn(string columnName, string columnAlias);
        void AddColumnSelection(ISelectColumns columnSelection);
        void AddRowNumberColumn(string alias, IReadOnlyList<Column> partitionBys);
        void AddRowNumberColumn(string alias, IReadOnlyList<TableColumn> partitionBys);
        void AddDefaultColumnSelection();
        void AddOption(string queryHint);
        void AddOptions(IReadOnlyList<string> queryHints);
        OrderByField[] RemoveOrderBys();
        ISelect GenerateSelect();
        ISelect GenerateSelectWithoutDefaultOrderBy();

        ISelectBuilder Clone();
        bool HasCustomColumnSelection { get; }
    }
}