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
using System.Collections.Generic;

/// <summary>
/// The application is completely created via javascript includes
/// ExtJs includes
/// Javascript.aspx which creates all the application javascript
/// 
/// </summary>
public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // ExtJs styles
        this.RegisterCssInclude("ext-all", "~/ExtJs/resources/css/ext-all.css");

        // ExtJs 
        this.RegisterJavascriptScriptInclude("ext-base", "~/ExtJs/adapter/ext/ext-base.js");

        if (Compress)
            this.RegisterJavascriptScriptInclude("ext-all", "~/ExtJs/ext-all.js");
        else
		    this.RegisterJavascriptScriptInclude("ext-all", "~/ExtJs/ext-all-debug.js");

        // The Application 
        this.RegisterJavascriptScriptInclude("application", "~/Javascript.aspx?Compress=" + Compress);

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

    #region Registering Includes
    private Dictionary<String, String> JavascriptScriptIncludes = new Dictionary<string, string>();
    /// <summary>
    /// Add a javascript include file to the page.
    /// key is used to stop the same key getting registerred twice
    /// </summary>
    /// <param name="key">unique key for the included file</param>
    /// <param name="url">location of the include. Can use ~/ to indicate root folder</param>
    protected void RegisterJavascriptScriptInclude(string key, string url)
    {
        if (!JavascriptScriptIncludes.ContainsKey(key))
        {
            JavascriptScriptIncludes.Add(key, url);

            HtmlGenericControl tag = new HtmlGenericControl("script");
            tag.Attributes.Add("type", "text/javascript");
            tag.Attributes.Add("src", this.ResolveUrl(url));
            this.Page.Header.Controls.Add(tag);

            if (!Compress)
                this.Page.Header.Controls.Add(new LiteralControl(Environment.NewLine));
        }
    }

    private Dictionary<String, String> CssIncludeIncludes = new Dictionary<string, string>();
    /// <summary>
    /// Add a css include file to the page.
    /// key is used to stop the same key getting registerred twice
    /// </summary>
    /// <param name="key">unique key for the css file</param>
    /// <param name="url">location of the css file. Can use ~/ to indicate root folder</param>
    protected void RegisterCssInclude(string key, string url)
    {
        if (!CssIncludeIncludes.ContainsKey(key))
        {
            CssIncludeIncludes.Add(key, url);

            HtmlGenericControl tag = new HtmlGenericControl("link");
            tag.Attributes.Add("type", "text/css");
            tag.Attributes.Add("rel", "stylesheet");
            tag.Attributes.Add("href", this.ResolveUrl(url));
            this.Page.Header.Controls.Add(tag);


            if (!Compress)
                this.Page.Header.Controls.Add(new LiteralControl(Environment.NewLine));

        }
    }
    #endregion
}
