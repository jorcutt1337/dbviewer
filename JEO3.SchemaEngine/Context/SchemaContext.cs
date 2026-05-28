using System.Diagnostics;
using JEO3.Data;
using JEO3.SchemaEngine.Extensions;
using JEO3.SchemaEngine.Models;

namespace JEO3.SchemaEngine
{
    public sealed class SchemaContext
    {
        #region Properties

        public SchemaModel Schema { get; private set; }

        private string _ConnectionString { get; init; }

        private bool isLoaded => Schema != null && Schema.Columns != null && Schema.Tables != null;

        #endregion

        #region Initialization

        public SchemaContext(string connectionString)
        {
            if (connectionString == null || connectionString == string.Empty) { throw new Exception($"Invalid Connection String: {connectionString}"); }
            _ConnectionString = connectionString;
        }

        public SchemaContext(string connectionString, SchemaModel schema)
            : this(connectionString)
        {
            if (schema == null || schema.Columns == null || schema.Tables == null) { throw new Exception($"Invalid Schema Model For Connnection: {connectionString}"); }
            Schema = schema;
        }

        public async Task<bool> LoadSchema()
        {
            try
            {
                // Test Connection
                var testConnection = DataUtility.TestConnection(_ConnectionString);
                if (testConnection == false) { throw new Exception($"Could Not Connect To Database: {_ConnectionString}"); }

                // Get Columns
                var columns = await DataUtility.GetInstances<SchemaColumn>(SqlQueries.SchemaQuery, _ConnectionString);

                // Get Table Relations
                var relations = await DataUtility.GetInstances<SchemaRelation>(SqlQueries.TableRelationsQuery, _ConnectionString);

                // Populate Schema Model
                this.Schema = new SchemaModel(columns, relations);

                return true;
            }
            catch (Exception ex)
            {
                // TODO: Logging
                throw;
            }
        }

        #endregion

        #region Query

        public async Task<string> GenerateQuery(string tableName, QueryOptions options)
        {
            // Check Schema Loaded
            if (isLoaded == false)
            {
                await this.LoadSchema();
            }

            // Get Table Model To Get Started
            var table = this.Schema.Tables.FirstOrDefault(t => t.TableName == tableName);
            if (table == null) { throw new InvalidOperationException($"Could Not Find Table: {tableName}"); }

            // Track Time Elapsed To Build
            var startTime = DateTime.Now;

            // Sanitize
            var alias = options.DefaultTableAlias.Length > 0 ? options.DefaultTableAlias : "X";
            alias = string.Join(string.Empty, alias.Except("!@#$%^&*()-=+/[]\\|`~,./?:;'{}".Select(v => v).AsEnumerable()));
            alias = alias.TrimStart("0123456789".Select(v => v).ToArray());

            // Constants To Avoid Hardcoding
            const string TB = "\t";
            const string RN = "\r\n";
            const string RNT = RN + TB;
            const string FROM = "FROM ";
            var isSingleLine = options.PutTableSelectsSingleLinePerTable;
            var SLTT = (isSingleLine == false ? string.Empty : TB + TB);

            // Query We Are Going To Build Eventually
            var query = "USE " + table.DatabaseName + RN + RNT + SqlQueries.SelectTop1000 + RN;

            // Keep Track Of Related Tables Were Gonna Include. Base Table Alias = 'X'. So Start Next Table As 'X2' Then 'X3' etc.
            var relatedTableColumnsSelect = string.Empty;
            var relatedTableJoins = new List<string>();
            var relatedTableIndex = 2;

            var querySegments = new List<QuerySegment>();

            if (options.LevelsUp >= 1)
            {
                // Loop Parent Relationships With Inner Joins
                foreach (var key in table.RelationsUp)
                {
                    var relatedTableColumns = new List<SchemaColumn>() { key.PrimaryColumn }.Concat(key.PrimaryColumn.OtherTableColumns())
                        // .Where(v => !SqlConstants.SqlNonPrimitiveDataTypes.Contains(v.DataType.ToUpper()))
                        .Distinct().ToList();

                    // Add Related Parent Table Join
                    var upJoin = TB + "INNER JOIN " + key.PrimaryTableName + " " + alias + relatedTableIndex + " ON " + alias + relatedTableIndex + "." + key.PrimaryColumnName + " = " + alias + "." + key.ForeignColumnName;
                    relatedTableJoins.Add(upJoin);

                    // Append Related Table Columns Select Clause (Columns Separated By Comma Or Line Break Depending On Checkbox Option '1 Line Per Table Selects')
                    relatedTableColumnsSelect += RN + SLTT + string.Join("," + (isSingleLine == false ? RN + SLTT : " "), relatedTableColumns.OrderBy(x => x.OrdinalPosition).Select(x => (isSingleLine == false ? TB + TB : "") + alias + relatedTableIndex + "." + x.ColumnName)) + ",";
                    relatedTableIndex++;


                    var upSegment = new QuerySegment(key.PrimaryTableName, key.ForeignTableName, relatedTableColumnsSelect, upJoin); //, key.PrimaryTable, options.IncludeJoinTableSelects == false ? new List<SchemaColumn>() : relatedTableColumns.OrderBy(x => x.OrdinalPosition).ToList(), key.ForeignTable, new List<SchemaRelation>() { key });
                    querySegments.Add(upSegment);

                    Debug.WriteLine($"Primary: {key.ForeignTableName}.{key.ForeignColumnName} -> Foreign: {key.PrimaryTableName}.{key.PrimaryColumnName}");
                }
            }

            // TODO: This Actually Works... Need To Do More Testing With Deeper Relationships On Different Databases

            if (options.LevelsDown >= 1)
            {
                // Recursively Get Child Relationship Segments (Joins And Selects) To Specified Level Down
                var downSegments = GetQueryRelationsDown(table, 0, options.LevelsDown, options);
                querySegments.AddRange(downSegments);



                // HERE



                // Keep Track Of Tables We've Already Added To The Query So We Can Alias Them Correctly When They Come Up Again In Deeper Relationships (e.g. Table A Joins To B Which Joins Back To A - We Don't Want To Alias The First A As X And The Second A As X2, We Want Both To Be X With The Correct Joins/Selects)
                var newTables = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(table.TableName, alias) };

                // Loop Child Relationships With Left Joins
                foreach (var segment in downSegments)
                {
                    // Validation - If Table Already Been Added As Join Then Don't Add Again, Just Alias In Joins/Selects (Prevents Circular Relationship Issues)
                    if (segment.PrimaryTable != table.TableName && !newTables.Any(v => v.Key == segment.PrimaryTable))
                    {
                        // Add Related Child Table Join
                        newTables.Add(new KeyValuePair<string, string>(segment.PrimaryTable, alias + (relatedTableIndex - 1)));
                    }

                    // Replace Foreign Table Name Placeholder With Proper Foreign Table Alias In SELECT
                    var updatedSelect = segment.Selects.Replace("[" + segment.ForeignTable + "]", alias + relatedTableIndex);

                    // Replace Primary Table Name Placeholder With Proper Primary Table Alias In SELECT
                    updatedSelect = updatedSelect.Replace("[" + table.TableName + "]", alias);

                    // Replace Foreign Table Name Placeholder With Proper Foreign Table Alias In JOIN
                    var updatedJoin = segment.Joins.Replace("[" + segment.ForeignTable + "]", alias + relatedTableIndex);

                    // Check For Match In The List Were Tracking
                    var match = newTables.FirstOrDefault(v => v.Key == segment.PrimaryTable);

                    // Validation - If Primary Table Placeholder Exists
                    if (match.Key == segment.PrimaryTable && segment.Joins.Contains("[" + segment.PrimaryTable + "]"))
                    {
                        // Replace Primary Table Placeholder With Proper Primary Table Alias In JOIN
                        updatedJoin = updatedJoin.Replace("[" + segment.PrimaryTable + "]", match.Value);
                    }

                    relatedTableColumnsSelect += updatedSelect;
                    relatedTableJoins.Add(updatedJoin);
                    relatedTableIndex++;
                }
            }

            // Kill Any Trailing Comma Before "FROM" Clause
            relatedTableColumnsSelect = relatedTableColumnsSelect.TrimEnd(',');

            // Get Primary Table Columns Separated By Comma Or Line Break Depending On Checkbox Option '1 Line Per Table Selects'
            var primaryColumns = table.Columns
                // .Where(v => !SqlConstants.SqlNonPrimitiveDataTypes.Contains(v.DataType.ToUpper()))
                .OrderBy(v => v.OrdinalPosition).Select(v => (isSingleLine == false ? TB + TB : " ") + alias + "." + v.ColumnName).Distinct().ToList();


            var baseSelect = table.Columns.Query(isSingleLine, alias);
            //var baseSelect = SLTT + String.Join("," + (isSingleLine == false ? RN : ""), primaryColumns);
            var baseJoin = (options.GenerateTableJoins == true) ? RNT + FROM + table.TableName + " " + alias : RNT + FROM + table.TableName + " " + alias;
            var baseSegment = new QuerySegment(table.TableName, string.Empty, baseSelect, baseJoin); //, table, table.Columns.OrderBy(v => v.OrdinalPosition).ToList(), null, null);
            querySegments.Insert(0, baseSegment);

            //var newQuery = GetQuery(querySegments, table, options);

            // If Auto-Generate Query Joins
            if (options.GenerateTableJoins == true)
            {
                query += baseSelect +
                (options.IncludeJoinTableSelects == false ? "" : (relatedTableColumnsSelect.Replace(RN, "").Length > 0 ? "," + relatedTableColumnsSelect.TrimEnd('\r').TrimEnd('\n').TrimEnd(',') : "")) +
                baseJoin +
                RNT + string.Join(RNT, relatedTableJoins);
            }
            else
            {
                query +=
                SLTT + baseSelect +
                baseJoin;
            }

            query = query + RNT + RNT;

            // DEBUG - Log Time Elapsed
            Debug.WriteLine("Time To Generate Query - " + (decimal)Math.Round((DateTime.Now - startTime).TotalSeconds, 3) + " seconds");

            return query;
        }

        private List<QuerySegment> GetQueryRelationsDown(SchemaTable model, int currentLevel, int stopAfter, QueryOptions options, List<QuerySegment> queries = null)
        {
            // Constants To Avoid Hardcoding
            const string TB = "\t";
            const string RN = "\r\n";
            var isSingleLine = options.PutTableSelectsSingleLinePerTable;
            var SLTT = (isSingleLine == false ? string.Empty : TB + TB);

            // Default Return Query Segments
            queries = queries ?? new List<QuerySegment>();

            // List Of Relations To Skip.
            // E.g. SalesOrderDetail Table Links To SpecialOfferProduct Table On BOTH ProductID AND SpecialOfferID
            // The 1st Ordinal Position Relation (ProductID) Will Process All Links Between These 2 Tables
            // SpecialOfferID Relation Will Be Added To This List In Loop So We Can Properly Skip It
            var compositeRelationsToSkip = new List<SchemaRelation>();

            // Loop Child Relationships With Left Joins
            foreach (var key in model.RelationsDown)
            {
                // Validation
                if (options.ColumnsToIgnore.Any(v => v == key.PrimaryColumnName || v == key.ForeignColumnName))
                {
                    Debug.WriteLine($"Ignoring Relation - Primary: {key.PrimaryTableName}.{key.PrimaryColumnName} -> Foreign: {key.ForeignTableName}.{key.ForeignColumnName} Because Column Is In DefaultColumnsToIgnore List");
                    continue;
                }

                // Check If This Relation Was Already Processed By A Previous Relation To Same Table In Its Own JOIN. If So Skip It
                if (compositeRelationsToSkip.Contains(key)) { continue; }

                // Get Any Other Composite Key Columns For This Foreign Table
                var compositeOtherRelations = model.RelationsDown.Where(v => v.IsCompositeKey && v.ForeignTableName == key.ForeignTableName && v.ForeignColumnName != key.ForeignColumnName).ToList();
                if (compositeOtherRelations.Count > 0)
                {
                    // Add Other Composites To List Of Relations To Skip, Because Were Gonna Process Them Right Now
                    compositeRelationsToSkip.AddRange(compositeOtherRelations);
                }

                // Get Related Table Columns
                var relatedTableColumns = new List<SchemaColumn>() { key.ForeignColumn }.Concat(key.ForeignColumn.OtherTableColumns())
                    // .Where(v => !SqlConstants.SqlNonPrimitiveDataTypes.Contains(v.DataType.ToUpper()))
                    .Distinct().ToList();

                // Add Related Child Table Join
                var joins = TB + "LEFT JOIN " + key.ForeignTableName + " [" + key.ForeignTableName + "] ON [" + key.ForeignTableName + "]." + key.ForeignColumnName + " = [" + key.PrimaryTableName + "]." + key.PrimaryColumnName
                    + (compositeOtherRelations.Count == 0 ? string.Empty : " AND " + String.Join(" AND ", compositeOtherRelations.Select(v => "[" + v.ForeignTableName + "]." + v.ForeignColumnName + " = " + "[" + v.PrimaryTableName + "]." + v.PrimaryColumnName)));

                // DEBUG - For Testing
                if (compositeOtherRelations.Count > 0)
                {
                    foreach (var rel in compositeOtherRelations)
                    {
                        Debug.WriteLine($"Added Composite Key Entry - Primary: {rel.PrimaryTableName}.{rel.PrimaryColumnName} -> Foreign: {rel.ForeignTableName}.{rel.ForeignColumnName}");
                        Debug.WriteLine($"JOIN: {joins}");
                    }
                }

                // Append Related Table Columns Select Clause (Columns Separated By Comma Or Line Break Depending On Checkbox Option '1 Line Per Table Selects')
                var selects = RN + SLTT + string.Join("," + (isSingleLine == false ? RN + SLTT : " "), relatedTableColumns.OrderBy(x => x.OrdinalPosition).Select(x => (isSingleLine == false ? TB + TB : "") + "[" + key.ForeignTableName + "]." + x.ColumnName)) + ",";


                // DEBUG
                //var selectCols = relatedTableColumns.OrderBy(x => x.OrdinalPosition).Select(v => new Mapping(v.TableName, v.ColumnName, v.DataType)).ToList();
                //var allRelations = new List<SchemaRelation>() { key }.Concat(compositeOtherRelations).ToList();


                // Add Query Segments To Return List
                queries.Add(new QuerySegment(key.PrimaryTableName, key.ForeignTableName, selects, joins)); //, key.PrimaryColumn.Table, key.ForeignColumn.Table, new List<SchemaRelation>() { key }.Concat(compositeOtherRelations).ToList()));
                currentLevel++;
                Debug.WriteLine($"Main - Primary: {key.PrimaryTableName}.{key.PrimaryColumnName} -> Foreign: {key.ForeignTableName}.{key.ForeignColumnName} - Added");

                // Check If Stop
                if (currentLevel >= stopAfter)
                {
                    // Debug.WriteLine($"Primary: {key.PrimaryTableName} -> Foreign: {key.ForeignTableName} Stopping");
                    continue;
                }

                // Find Next Level Down
                var find = this.Schema.Tables.First(v => v.TableName == key.ForeignTableName);

                // Validation - If Table Already Been Added As Join Then Don't Recurse Down That Path Again (Prevents Circular Relationship Issues)
                if (queries.Any(v => v.PrimaryTable == find.TableName)) { continue; }

                // Recursion - Get Next Level Down Relations
                var nextLevelDownQueries = GetQueryRelationsDown(find, currentLevel, stopAfter, options, queries);

                // Loop 
                foreach (var query in nextLevelDownQueries)
                {
                    // Validation?
                    if (queries.Any(v => v.PrimaryTable == query.PrimaryTable && v.ForeignTable == query.ForeignTable))
                    {
                        //  Debug.WriteLine($"Primary: {query.PrimaryTableName} -> Foreign: {query.ForeignTableName} - Skipped. Already In Table");
                        continue;
                    }
                    Debug.WriteLine($"Nested - Primary: {query.PrimaryTable} -> Foreign: {query.ForeignTable} - Adding");
                    queries.Add(query);
                }
            }

            return queries;
        }

        #endregion
    }
}
