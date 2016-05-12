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
/// Effectively the main application
/// Creates a few tabs with grids
/// </summary>
public class ExtJsDemoApplication : ScriptItem
{
    public ExtJsDemoApplication()
    {
    }

    private static ScriptItem _Namespace = null;
    public static ScriptItem Namespace
    {
        get
        {
            if (_Namespace == null)
            {
                _Namespace = Sb.Compressible("ClockWork", "Cw");
            }
            return _Namespace;
        }
    }


    protected override void OnRender(RenderingEventArgs e)
    {
        base.OnRender(e);

        ScriptItem viewportName = Sb.Line(Namespace, ".", Sb.Compressible("Viewport", "Vp"));

		IScriptWriter writer = e.Writer;

        // add the Stores and Grids
        JsObject viewPortClass = new JsObject(); // the display area for the application that contains everything

        JsArray tabs = new JsArray(); // will contain all the tabs in the application

        Script initComponent = new Script(); // the script withing the initComponent function of the viewport

        // the tables to include in the application
        string[] tables = new string[] {"Categories", "Employees", "Customers", "Shippers", "Suppliers", "Products", "Orders", "Order Details" };

        // add all the stuff needed for each table
        foreach (string table in tables)
        {
            string gridName = table.Replace(" ", "_");

            // get the tables schema as a DataTable
            ClockWorkDataProvider dataProvider = new ClockWorkDataProvider("Northwind", "SELECT * FROM [" + table + "]", "SELECT count(*) FROM [" + table + "]");
            DataTable dataTable = dataProvider.GetSchema();

            // write the ExtJsStore class definition that will be used by the grid
            writer.Write(new ExtJsStore(dataTable));

            // write the ExtJsGrid class definition that will be used next
            writer.Write(new ExtJsGrid(dataTable));

            // add a property to the viewport class for this tables grid
            viewPortClass.Properties.Add(gridName, "{}");

            // initialise the grid when the class is initialised
            initComponent.Add(Js.Statement("this." + gridName + " = ", Js.New(ExtJsGrid.ClassName(gridName))));

            // add a tab that contains the grid
            tabs.Add(
                 Js.New("Ext.Panel",
                    Js.Object(ScriptLayout.InlineBlock,
                        Js.Property("autoScroll", true),
                        Js.Property("layout", Js.Q("fit")),
                        Js.Property("title", Js.Q(table)),
                        Js.Property("items", "this." + gridName)
                    )
                )
            );

        }

        // create the viewport that contains the tabs
        viewPortClass.AddRange(
           Js.Property("initComponent",
                Js.Function(ScriptLayout.InlineBlock,
                    Js.Block(
                        // initialise the grids
                        initComponent,

                        ExtJs.Apply(
                            Js.Object(
                                Js.Property("layout", Js.Q("border")),

                                Js.Property("items",
                                    Js.Array(ScriptLayout.InlineBlock,
                                        Js.Object(
                                            Js.Property("region", Js.Q("north")),
                                            Js.Property("html", Js.Q("<h1 class='x-panel-header'>An ASP.Net based ExtJs Demo using the ClockWork ScriptBuilder (<a href='http://www.mccreath.org.uk/Article/ClockWork-Script-Builder-Net_43.aspx'>Related Article and Source Code</a>)</h1>")),
                                            Js.Property("autoHeight", true),
                                            Js.Property("margins", Js.Q("10 20 0 20")),
                                            Js.Property("border", false)
                                        ),
                                        // viewport consists of a TabPanel
                                        Js.New("Ext.TabPanel",
                                            Js.Object(ScriptLayout.InlineBlock,
                                                Js.Property("region", Js.Q("center")),
                                                Js.Property("margins", Js.Q("10 20 20 20")),
                                                Js.Property("activeTab", 0),
                                                Js.Property("items", tabs)
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        ExtJs.BaseApply(viewportName, "initComponent")
                    )
                )
            )
        );

        // write the viewport and start the application by creating an instance of it
		writer.Write(
			Sb.Script(
                ExtJs.Component(viewportName, "Ext.Viewport", viewPortClass),

                // starts the application by creating the viewport
                Js.Call("Ext.onReady",
                    Js.Function(
                        Js.Statement("var vp = ", Js.New(viewportName))
                    )
                )
			)
		);
    }
}
