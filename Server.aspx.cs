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
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Net;
using ClockWork.ScriptBuilder.JavaScript;
using System.Data.Common;

/// <summary>
/// This page serves data to the grids
/// </summary>
public partial class Server : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Response.ContentType = "text/xml";
		Response.ContentEncoding = Encoding.UTF8;

		try
		{

			string task = this.Request.Params["Task"];

			if (String.IsNullOrEmpty(task))
			{
				throw new Exception("Missing Task");
			}
			else
			{
				switch (task)
				{
					case "GetData":

                        // ExtGrid query string parameters
						string table = this.Request.Params["table"];
						string limit = this.Request.Params["limit"]; // page size
						string start = this.Request.Params["start"]; // start of page
						string sort = this.Request.Params["sort"]; // sort field
						string dir = this.Request.Params["dir"]; // sort direction ASC/DESC
						// string query = this.Request.Params["query"]; // search string

                        // convert data for db queries
                        int first = String.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start);
                        int amount = String.IsNullOrEmpty(limit) ? 0 : Convert.ToInt32(limit);

                        string orderBy = String.IsNullOrEmpty(sort) ? null : (sort + " " + dir).Trim();

                        // get data from db using my simple generic provider
                        ClockWorkDataProvider dataProvider = new ClockWorkDataProvider("Northwind", "SELECT * FROM [" + table + "]", "SELECT count(*) FROM [" + table + "]");

                        DataTable dataTable = dataProvider.GetTable(first, amount, orderBy);

                        // convert DataTable into json using a custom ScriptItem
                        ExtJsDataTable jsDataTable = new ExtJsDataTable(dataTable, dataProvider.GetCount());

                        // write the response
                        jsDataTable.Render(this.Response.OutputStream, JsFormatProvider.Instance);
                        return;

					default:
						throw new Exception("Unknown task: " + task);

				}
			}
		}
		catch (Exception ex)
		{
            // reply with an error status code and ExtJs failure properties
            
			Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            JsObject responseObject = Js.Object();

            responseObject.Properties.Add("error", Js.Q(ex.Message));
            responseObject.Properties.Add("success", false);

            responseObject.Render(this.Response.OutputStream, JsFormatProvider.Instance);
		}


	}
}
