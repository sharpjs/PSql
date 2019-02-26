namespace PSql
{
    /// <summary>
    ///   Database products supported by PSql.
    /// </summary>
    public enum SqlProduct
    {
        /// <summary>
        ///   SQL Server Database Engine
        /// </summary>
        SqlServer,

        /// <summary>
        ///   Azure SQL Database
        /// </summary>
        AzureSqlDatabase

        // Possible additions:
        // - SqlServerAnalysisServices
        // - AuzreAnalysisServices
        // - AzureSqlDataWarehouse
        // - ParallelDataWarehouse
    }
}
