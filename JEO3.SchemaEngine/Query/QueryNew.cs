//using System;
//using System.Collections.Generic;
//using System.Text;
//using JEO3.SchemaEngine.Models;

//namespace JEO3.SchemaEngine.Query
//{
//    public interface IMapping
//    {
//        Table Parent { get; set; }
//        string Alias { get; set; }
//        string Table { get; init; }
//        string Column { get; init; }
//        string DataType { get; init; }
//        bool Exists { get; }
//        bool IsIdentity { get; init; }
//        bool IsNullable { get; init; }
//    }

//    public class Mapping : IMapping
//    {
//        public Table Parent { get; set; } = null;
//        public string Alias { get; set; }
//        public string Table { get; init; }
//        public string Column { get; init; }
//        public string DataType { get; init; }
//        public bool Exists => Table?.Length > 0 && Column?.Length > 0 && DataType?.Length > 0;
//        public bool IsIdentity { get; init; }
//        public bool IsNullable { get; init; }

//        public Mapping(string table, string column, string dataType, bool isIdentity, bool isNullable, string defaultAlias = "")
//        {
//            this.Table = table;
//            this.Column = column;
//            this.DataType = dataType;
//            this.IsIdentity = isIdentity;
//            this.IsNullable = isNullable;
//            this.Alias = defaultAlias?.Length > 0 ? defaultAlias : string.Empty;
//        }

//        public void SetParent(Table parent)
//        {
//            Parent = parent;
//        }
//    }

//    public interface IRelation
//    {
//        Mapping Source { get; init; }
//        Mapping? Destination { get; init; }
//    }

//    public class Relation : IRelation
//    {
//        public Mapping Source { get; init; }
//        public Mapping? Destination { get; init; }
//        public Relation(Mapping source, Mapping? destination = null)
//        {
//            Source = source;
//            Destination = destination;
//        }
//    }

//    public interface ITable
//    {
//        string Name { get; init; }
//        IReadOnlyList<Mapping> SelectColumns { get; init; }
//        IReadOnlyList<Relation> Relations { get; init; }
//        bool CompositeKey { get; }
//    }

//    public class Table : ITable
//    {
//        public string Name { get; init; }
//        public IReadOnlyList<Mapping> SelectColumns { get; init; }
//        public IReadOnlyList<Relation> Relations { get; init; }
//        public bool CompositeKey => Relations?.Count > 1 && Relations.GroupBy(v => new { SourceTable = v.Source?.Table, DestinationTable = v.Destination?.Table }).Distinct().Count() == 1;

//        public Table(string name, IReadOnlyList<Mapping> selectColumns, IReadOnlyList<Relation> relations)
//        {
//            Name = name;
//            SelectColumns = selectColumns;
//            Relations = relations;
//        }
//    }

//    public static class QueryExtensions
//    {
//        //public static Mapping ToMapping(this SchemaColumn column)
//        //{
//        //    return new Mapping(column.TableName, column.ColumnName, column.DataType, column.IsIdentity, column.IsNullable);
//        //}
//        //public static Relation ToRelation(this SchemaRelation relation)
//        //{
//        //    var src = new Mapping(relation.PrimaryTableName, relation.PrimaryColumnName, relation.PrimaryColumnDataType, relation.PrimaryColumn.IsIdentity, relation.PrimaryColumn.IsNullable);
//        //    var dest = new Mapping(relation.ForeignTableName, relation.ForeignColumnName, relation.ForeignColumnDataType, relation.ForeignColumn.IsIdentity, relation.ForeignColumn.IsNullable);
//        //    return new Relation(src, dest);
//        //}

//        //public static Table ToTable(this SchemaTable table, bool includeColumns)
//        //{
//        //    var mappings = includeColumns == false ? new List<Mapping>(), table.Columns.Select(v => v.ToMapping()).ToList();
//        //    var relations = table.RelationsDown.Select(v => v.ToRelation()).ToList();

//        //    var x new Table(table.TableName, )
//        //    {

//        //    };

//        //    return x;
//        //}
//    }
//}
