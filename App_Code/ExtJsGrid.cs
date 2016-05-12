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
/// Renders an Ext.grid.GridPanel based on the provided DataTable
/// </summary>
public class ExtJsGrid : ScriptItem
{
	public ExtJsGrid(DataTable table)
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

    private int _PageSize = 20;
    /// <summary>
    /// If not zero then the grid adds a pager and displays this number of rows at a time
    /// </summary>
    public int PageSize
    {
        get { return _PageSize; }
        set { _PageSize = value; }
    }

    private static ScriptItem _Namespace = null;
    public static ScriptItem Namespace
    {
        get
        {
            if (_Namespace == null)
            {
                _Namespace = Sb.Compressible("ClockWork.Grid", "Cw.Gr");
            }
            return _Namespace;
        }
    }

    public static ScriptItem ClassName(string storeName)
    {
        return Sb.Line(Namespace, ".", storeName.Replace(" ", "_"));
    }
	
    /// <summary>
    /// writes the Ext.grid.GridPanel
    /// </summary>
    /// <param name="e"></param>
	protected override void OnRender(RenderingEventArgs e)
	{
        ScriptItem className = ExtJsGrid.ClassName(this.Table.TableName);

        ScriptItem storeProperty = Sb.Compressible("store", "s");

        ScriptItem thisStoreProperty = Sb.Line("this.", storeProperty);

        // define each column in the grid
		JsArray columns = Js.Array(ScriptLayout.InlineBlock);

		foreach (DataColumn dataColumn in Table.Columns)
		{
			JsObject column = Js.Object(ScriptLayout.InlineBlock);

            column.Properties.Add("header", Js.Q(dataColumn.Caption)); // the displayed column heading
			column.Properties.Add("dataIndex", Js.Q(dataColumn.ColumnName)); // the data field the column relates to
            column.Properties.Add("sortable", true);
			columns.Add(column);
		}

        // write the class
		e.Writer.Write(
			ExtJs.Component(
				className, "Ext.grid.GridPanel",
				Js.Object(
                    Js.Property(storeProperty, "{}"), // the store that is the data source for the grid

					Js.Property("initComponent",
						Js.Function(ScriptLayout.InlineBlock,
							Js.Block(
                                // create the store
                                Js.Statement(thisStoreProperty, " = ", Js.New(ExtJsStore.ClassName(this.Table.TableName))),
                                Js.Statement(thisStoreProperty, ".baseParams['limit'] = " + this.PageSize), // tell it to page
								ExtJs.Apply(
									Js.Object(
										Js.Property("columns", columns),
                                        Js.Property("store", thisStoreProperty),

                                        // add the pager
                                        Js.Property("bbar",
											Js.New("Ext.PagingToolbar",
												Js.Object(ScriptLayout.InlineBlock,
													Js.Property("pageSize", this.PageSize),
                                                    Js.Property("store", thisStoreProperty),
													Js.Property("displayInfo", true),
													Js.Property("displayMsg", true),
													Js.Property("emptyMsg", Js.Q("No " + this.Table.TableName + " to display")),
													Js.Property("displayInfo", Js.Q("Displaying " + this.Table.TableName + "s {0} - {1} of {2})"))
												)
											)
										)
									)
								),
								ExtJs.BaseApply(className, "initComponent")
							)
						)
					),
					Js.Property("onRender",
						Js.Function(ScriptLayout.InlineBlock,
							Js.Block(
								Js.Statement(thisStoreProperty,".load()"), // all loads should be done in the onrender event to stop a possible race condition ... http://extjs.com/forum/showthread.php?p=140715
								ExtJs.BaseApply(className, "onRender")
							)
						)
					)
				)
			)
		);
	}
}
