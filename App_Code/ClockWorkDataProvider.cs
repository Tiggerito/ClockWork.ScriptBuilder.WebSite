/*
 * Copyright (c) 2008, Anthony James McCreath
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     1 Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     2 Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     3 Neither the name of the project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY Anthony James McCreath "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Anthony James McCreath BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.Common;

/// <summary>
/// A simple generic class to get data from the database
/// </summary>
public class ClockWorkDataProvider
{
    /// <summary>
    /// A simple generic class to get data from the database
    /// </summary>
    /// <param name="name">the name of a connectionString definition in the Web.Config file. used for this connection string and providerName</param>
    /// <param name="selectCommand">the sql command to select data. does not need to limit for paging, this is done internally</param>
    /// <param name="countCommand">an sql command that will return a single result that is the total size of the data (rows)</param>
    public ClockWorkDataProvider(string name, string selectCommand, string countCommand)
    {
        _Name = name;
        _SelectCommand = selectCommand;
        _CountCommand = countCommand;
    }

    private string _Name;
    /// <summary>
    /// the name of a connectionString definition in the Web.Config file. used for this connection string and providerName
    /// </summary>
    public string Name
    {
        get { return _Name; }
        set { _Name = value; }
    }

    private string _SelectCommand;
    /// <summary>
    /// the sql command to select data. does not need to limit for paging, this is done internally
    /// </summary>
    public string SelectCommand
    {
        get { return _SelectCommand; }
        set { _SelectCommand = value; }
    }

    private string _CountCommand;
    /// <summary>
    /// an sql command that will return a single result that is the total size of the data (rows)
    /// </summary>
    public string CountCommand
    {
        get { return _CountCommand; }
        set { _CountCommand = value; }
    }

    /// <summary>
    /// Returns a fully populated DataTable
    /// </summary>
    /// <returns></returns>
    public DataTable GetTable()
    {
        return GetTable(0,0, null);
    }
    /// <summary>
    /// Returns an ordered DataTable populated by the paging info
    /// </summary>
    /// <param name="first">the index of the first record to return</param>
    /// <param name="amount">how many records to return (may return less)</param>
    /// <param name="orderBy">How to order the results (sql style)</param>
    /// <returns></returns>
    public DataTable GetTable(int first, int amount, string orderBy)
    {
        DataTable table = new DataTable();
        Fill(table, first, amount, orderBy);
        return table;
    }
    /// <summary>
    /// fully populate the DataTable
    /// </summary>
    /// <param name="table">table to populate</param>
    public void Fill(DataTable table)
    {
        Fill(table, 0, 0, null);
    }

    /// <summary>
    /// Populates an ordered DataTable populated by the paging info
    /// </summary>
    /// <param name="table">table to populate</param>
    /// <param name="first">the index of the first record to return</param>
    /// <param name="amount">how many records to return (may return less)</param>
    /// <param name="orderBy">How to order the results (sql style)</param>
    public void Fill(DataTable table, int first, int amount, string orderBy)
    {
        // get data from the web.config file
        string providerName = System.Configuration.ConfigurationManager.ConnectionStrings[this.Name].ProviderName;
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[this.Name].ConnectionString;


        DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);

        using (DbConnection connection = factory.CreateConnection())
        {
            // set the connection string with the ordering
            connection.ConnectionString = connectionString;

            using (DbDataAdapter adapter = factory.CreateDataAdapter())
            {
                adapter.SelectCommand = connection.CreateCommand();

                adapter.SelectCommand.CommandText = String.IsNullOrEmpty(orderBy) ? SelectCommand : SelectCommand + " ORDER BY " + orderBy;

                if (amount<=0)
                    adapter.Fill(table);
                else
                    adapter.Fill(first, amount, table); // a realy crap way to page, but then its access!
            }
        }
    }

    /// <summary>
    /// Users the CountCommand to get the full count of the data source
    /// </summary>
    /// <returns></returns>
    public int GetCount()
    {
        // get data from the web.config file
        string providerName = System.Configuration.ConfigurationManager.ConnectionStrings[this.Name].ProviderName;
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[this.Name].ConnectionString;

        DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);

        using (DbConnection connection = factory.CreateConnection())
        {
            connection.ConnectionString = connectionString;

            using (DbCommand command = connection.CreateCommand())
            {
                try
                {
                    connection.Open();

                    command.CommandText = CountCommand;
                    return (int)command.ExecuteScalar();
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }

    /// <summary>
    /// Gets schema information from the datasource based on the SelectCommand
    /// </summary>
    /// <returns></returns>
    public DataTable GetSchema()
    {
        DataTable table = new DataTable();
        FillSchema(table);
        return table;
    }

    /// <summary>
    /// Populates the DataTable with schema information from the datasource based on the SelectCommand
    /// </summary>
    /// <param name="table"></param>
    public void FillSchema(DataTable table)
    {
        string providerName = System.Configuration.ConfigurationManager.ConnectionStrings[this.Name].ProviderName;
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[this.Name].ConnectionString;

        DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);

        using (DbConnection connection = factory.CreateConnection())
        {
            connection.ConnectionString = connectionString;

            using (DbDataAdapter adapter = factory.CreateDataAdapter())
            {
                adapter.SelectCommand = connection.CreateCommand();
                adapter.SelectCommand.CommandText = SelectCommand;

                adapter.FillSchema(table,SchemaType.Source);
            }
        }
    }
}
