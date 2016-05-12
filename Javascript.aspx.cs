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
using ClockWork.ScriptBuilder;
using ClockWork.ScriptBuilder.JavaScript;
using System.Text;

/// <summary>
/// Provides all the javascript to run the application using the ExtJsDemoApplication class
/// </summary>
public partial class JavaScriptPage : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Response.ContentType = "text/xml";
		Response.ContentEncoding = Encoding.UTF8;

		Script script = Sb.Script();

        script.Add(new ExtJsDemoApplication());  // write the javascript application

        ScriptWriter scriptWriter = new ScriptWriter(this.Response.OutputStream, JsFormatProvider.Instance);

        if (this.Compress)
        {
            scriptWriter.NewLine = String.Empty;
            scriptWriter.IndentText = String.Empty;
            scriptWriter.Compress = true;
        }

        scriptWriter.Write(script);

    }

    #region Compress
    private bool? _Compress = null;
    /// <summary>
    /// Should the content be rendered in a compressed way
    /// By default this is based on the comile mode where DEBUG is not compressed
    /// Passing a "Compressed" query string boolean can dynamically change this setting 
    /// </summary>
    public bool Compress
    {
        get
        {
            if (_Compress == null)
            {
                // check if status is set in the querystring
                string c = this.Request.QueryString["Compress"];

                if (!String.IsNullOrEmpty(c))
                {
                    bool result;
                    if (Boolean.TryParse(c, out result))
                        _Compress = result;
                }

                // if was not specifically set then set based on compile mode
                if (_Compress == null)
                {
#if DEBUG
		            _Compress = false;
#else
                    _Compress = true;
#endif
                }
            }

            return (bool)_Compress;
        }
        set { _Compress = value; }
    }
    #endregion
}
