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
using ClockWork.ScriptBuilder;
using ClockWork.ScriptBuilder.JavaScript;
using ClockWork.ScriptBuilder.JavaScript.ExtJs;

/// <summary>
/// Generates Json based on a DataTable
/// </summary>
public class ExtJsDataTable : ScriptItem
{
    /// <summary>
    /// Generates JSon based on a DataTable
    /// </summary>
    /// <param name="table">a populated table. if paging it should only contain the current pages data</param>
    /// <param name="fullCount">for paging, inform the recipient how many records there realy are</param>
	public ExtJsDataTable(DataTable table, int fullCount)
	{
		_Table = table;
        _FullCount = fullCount;
	}

	private DataTable _Table;
    /// <summary>
    /// a populated table. if paging it should only contain the current pages data
    /// </summary>
	public DataTable Table
	{
		get { return _Table; }
		set { _Table = value; }
	}

    private int _FullCount;
    /// <summary>
    /// for paging, inform the recipient how many records there realy are
    /// </summary>
    public int FullCount
    {
        get { return _FullCount; }
        set { _FullCount = value; }
    }

    /// <summary>
    /// creates a javascript/json object for a single row of data
    /// </summary>
    /// <param name="dataRow"></param>
    /// <returns></returns>
	protected JsObject ExtJsDataRow(DataRow dataRow)
	{
        JsObject row = Js.Object(ScriptLayout.Block);

		foreach (DataColumn dataColumn in Table.Columns)
		{
			object value = dataRow[dataColumn];

			switch (dataColumn.DataType.ToString())
			{
				case "Int16":
				case "Int32":
				case "Int64":
				case "Double":
				case "Float":
				case "Decimal":
				case "Boolean":
					// add as is
					break;
				default:
					// quote anything else
					value = Js.Q(value);
					break;
			}

			row.Properties.Add(dataColumn.ColumnName, value);	
		}

		return row;
	}

    /// <summary>
    /// creates a javascript/json Array for the supplied Table
    /// </summary>
    /// <returns></returns>
	protected JsArray DataTableJsArray()
	{
        JsArray array = Js.Array(ScriptLayout.Block);

		foreach (DataRow dataRow in this.Table.Rows)
		{
			array.Add(ExtJsDataRow(dataRow));
		}

		return array;
	}

    /// <summary>
    /// render the supplied DataTable as a javascript/json array inside an object ExtJs understands
    /// </summary>
    /// <param name="e"></param>
	protected override void OnRender(RenderingEventArgs e)
	{
        JsObject data = Js.Object(ScriptLayout.Block);

        data.Properties.Add("results", this.FullCount);
		data.Properties.Add("rows", this.DataTableJsArray());

		e.Writer.Write(data);
	}

}
