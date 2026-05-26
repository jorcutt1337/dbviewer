using System;
using System.Collections.Generic;
using System.Text;

namespace JEO3.SchemaEngine
{
    public sealed class QueryOptions
    {
        #region Properties

        public string DefaultTableAlias { get; private set; } = "X";
        public List<string> ColumnsToIgnore { get; init; }

        public bool GenerateTableJoins { get; init; }
        public bool IncludeJoinTableSelects { get; init; }
        public bool PutTableSelectsSingleLinePerTable { get; init; }

        /// <summary>
        /// # of Recursive Levels Down For Generating Joins And Selects Via Child Relationships (FKs To This Column -> Downward).
        /// It controls The Depth Of The Join Tree When Generating Queries.
        /// </summary>
        public int LevelsDown { get; init; }

        /// <summary>
        /// # of Recursive Levels Up For Generating Joins And Selects Via Parent Relationships (This Column's FKs To Others - Upward)
        /// It controls The Depth Of The Join Tree When Generating Queries.
        /// </summary>
        public int LevelsUp { get; init; }

        /// <summary>
        /// Hard Limit For Recursion Depth. 15 Works Fine
        /// </summary>
        private const int MAXHEADROOM = 15;

        #endregion

        #region Initialization

        public QueryOptions(bool generateTableJoins, bool includeJoinTableSelects, bool joinTableSelectsOnSingleLinePerTable, 
            int recursiveLevelsDown, int recursiveLevelsUp, string defaultAlias = "X", params string[] columnsToIgnore)
        {
            GenerateTableJoins = generateTableJoins;
            IncludeJoinTableSelects = generateTableJoins && includeJoinTableSelects;
            PutTableSelectsSingleLinePerTable = joinTableSelectsOnSingleLinePerTable;
            LevelsDown = generateTableJoins ? recursiveLevelsDown > MAXHEADROOM ? MAXHEADROOM : recursiveLevelsDown < 0 ? 0 : recursiveLevelsDown : 0;
            LevelsUp = generateTableJoins ? recursiveLevelsUp > MAXHEADROOM ? MAXHEADROOM : recursiveLevelsUp < 0 ? 0 : recursiveLevelsUp : 0;
            ColumnsToIgnore = new List<string>(columnsToIgnore);
            DefaultTableAlias = defaultAlias;
        }

        #endregion
    }
}
