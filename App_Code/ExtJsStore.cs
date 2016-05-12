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
using ClockWork.ScriptBuilder.JavaScript.ExtJs;
using ClockWork.ScriptBuilder.JavaScript;

/// <summary>
/// Renders javascript to create an Ext.data.Store class based on the provided DataTable
/// </summary>
public class ExtJsStore : ScriptItem
{
    /// <summary>
    /// Renders javascript to create an Ext.data.Store class based on the provided DataTable
    /// </summary>
    /// <param name="table">Table used as the source for data definitions</param>
	public ExtJsStore(DataTable table)
	{
		Table = table;
	}

	private DataTable _Table;
    /// <summary>
    /// Table used as the source for data definitions
    /// </summary>
	public DataTable Table
	{
		get { return _Table; }
		set { _Table = value; }
	}

    private static ScriptItem _Namespace = null;
    public static ScriptItem Namespace
    {
        get
        {
            if (_Namespace == null)
            {
                _Namespace = Sb.Compressible("ClockWork.Store", "Cw.St");
            }
            return _Namespace;
        }
    }

    public static ScriptItem ClassName(string storeName)
    {
        return Sb.Line(Namespace, ".", storeName.Replace(" ", "_"));
    }
	
    /// <summary>
    /// writes an Ext.data.Store definition
    /// </summary>
    /// <param name="e"></param>
	protected override void OnRender(RenderingEventArgs e)
	{

        ScriptItem configParam = Sb.Compressible("config", "c");

        ScriptItem className = ExtJsStore.ClassName(this.Table.TableName);

        // create all the field definitions
        JsArray fields = Js.Array(ScriptLayout.Block);

        foreach (DataColumn dataColumn in Table.Columns)
        {
            JsObject field = Js.Object();

            field.Properties.Add("name", Js.Q(dataColumn.ColumnName));
            field.Properties.Add("mapping", Js.Q(dataColumn.ColumnName));

            string type = ExtJs.ExtJsType(dataColumn.DataType);

            field.Properties.Add("type", Js.Q(type));

            fields.Add(field);
        }

        // define the meta data
        JsObject metaData = Js.Object(ScriptLayout.Block);

        if (Table.PrimaryKey.Length == 1)
            metaData.Properties.Add("id", Js.Q(Table.PrimaryKey[0].ColumnName));  // identifier for the data

        metaData.Properties.Add("totalProperty", Js.Q("results")); // property that contains the total number of rows
        metaData.Properties.Add("root", Js.Q("rows")); // property that contains the rows

        // put it all together
		e.Writer.Write(
			ExtJs.Class(
				className, "Ext.data.Store",
                Js.Parameters(configParam),
				Js.Block(
                    Js.Statement("var ", configParam, " = ", configParam, " || {}"),
                    ExtJs.ApplyIf(configParam,
						Js.Object(ScriptLayout.InlineBlock,
                            Js.Property("reader",
                                Js.New(ScriptLayout.InlineBlock, "Ext.data.JsonReader",
                                    metaData,
                                    fields
                                )
                            ),
							Js.Property("remoteSort", true),
							Js.Property("url", Js.Q("Server.aspx")), // gets data from our Server
							Js.Property("baseParams",
								Js.Object(ScriptLayout.InlineBlock,
									Js.Property("Task", Js.Q("GetData")), // request the server to send data
                                    Js.Property("table", Js.Q(this.Table.TableName)) // from this table 
								)
							)
						)
					),
                    ExtJs.BaseCall(className, "constructor", configParam)
				)
			)
		);
	}
}
