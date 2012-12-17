using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CRUDTable
{
    [DefaultProperty("TableName")]
    [ToolboxData("<{0}:CRUDTable runat=server></{0}:CRUDTable>")]
    public class CRUDTable : WebControl
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue("")]
        [Localizable(false)]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue("")]
        [Localizable(false)]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="CRUDTable" /> is read only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if read only; otherwise, <c>false</c>.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue(false)]
        [Localizable(false)]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow creating records.
        /// </summary>
        /// <value>
        ///   <c>true</c> if creating records is allowed; otherwise, <c>false</c>.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue(true)]
        [Localizable(false)]
        public bool AllowCreate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow reading records.
        /// </summary>
        /// <value>
        ///   <c>true</c> if reading records is allowed; otherwise, <c>false</c>.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue(true)]
        [Localizable(false)]
        public bool AllowRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow updating records.
        /// </summary>
        /// <value>
        ///   <c>true</c> if updating records is allowed; otherwise, <c>false</c>.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue(true)]
        [Localizable(false)]
        public bool AllowUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow deleting records.
        /// </summary>
        /// <value>
        ///   <c>true</c> if deleting records is allowed; otherwise, <c>false</c>.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue(true)]
        [Localizable(false)]
        public bool AllowDelete { get; set; }

        /// <summary>
        /// Gets or sets the size of the span.
        /// </summary>
        /// <value>
        /// The size of the span.
        /// </value>
        [Bindable(true)]
        [Category("CRUDTable")]
        [DefaultValue(12)]
        [Localizable(false)]
        public int SpanSize { get; set; }

        /// <summary>
        /// The table
        /// </summary>
        Table table = null;

        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <exception cref="System.InvalidOperationException">No table name is available. Try setting `TableName'.</exception>
        protected void HandleRequest(SqlConnection conn)
        {
            if (this.TableName == null) {
                throw new InvalidOperationException("No table name is available. Try setting `TableName'.");
            }

            foreach (string key in this.table.Keys) {
                Field f = this.table.GetField(key);
                f.Value = null;

                if (HttpContext.Current.Request.QueryString[key] != null && f.Type != Field.FieldType.File) {
                    f.Value = HttpContext.Current.Request.QueryString[key];
                }

                if (HttpContext.Current.Request.Form[key] != null && f.Type != Field.FieldType.File) {
                    f.Value = HttpContext.Current.Request.Form[key];
                }

                if (HttpContext.Current.Request.Files[key] != null) {
                    string path = Path.GetTempFileName();
                    HttpContext.Current.Request.Files[key].SaveAs(path);

                    DbFile file = new DbFile();
                    file.Name = HttpContext.Current.Request.Files[key].FileName;
                    file.MimeType = HttpContext.Current.Request.Files[key].ContentType;
                    file.Data = File.ReadAllBytes(path);

                    using (MemoryStream s = new MemoryStream()) {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(s, file);
                        f.Value = s.ToArray();
                    }
                }

                if (f.Type == Field.FieldType.File) {
                    if (HttpContext.Current.Request.Form[key + "-remove"] != null && HttpContext.Current.Request.Form[key + "-remove"] == "1") {
                        f.Value = DBNull.Value;
                    }
                }
            }
            switch (HttpContext.Current.Request.QueryString["action"]) {
                case "create":
                    HttpContext.Current.Response.StatusCode = (this.table.Create(conn)) ? 200 : 500;
                    HttpContext.Current.Response.End();
                    break;
                case "update":
                    foreach (string col in this.table.Keys) {
                        if (this.table.GetField(col).Type == Field.FieldType.File && this.table.GetField(col).Value != null) {
                            string cache = "CRUDTable_" + this.TableName + "_file_";
                            foreach (string key in this.table.PrimaryKeys) {
                                cache += this.table.GetField(key).Value + "_";
                            }
                            cache += col;
                            HttpContext.Current.Cache.Remove(cache);
                        }
                    }
                    HttpContext.Current.Response.StatusCode = (this.table.Update(conn)) ? 200 : 500;
                    HttpContext.Current.Response.End();
                    break;
                case "delete":
                    foreach (string col in this.table.Keys) {
                        if (this.table.GetField(col).Type == Field.FieldType.File) {
                            string cache = "CRUDTable_" + this.TableName + "_file_";
                            foreach (string key in this.table.PrimaryKeys) {
                                cache += this.table.GetField(key).Value + "_";
                            }
                            cache += col;
                            HttpContext.Current.Cache.Remove(cache);
                        }
                    }
                    HttpContext.Current.Response.StatusCode = (this.table.Delete(conn)) ? 200 : 500;
                    HttpContext.Current.Response.End();
                    break;
                case "file":
                    if (HttpContext.Current.Request.QueryString["tablecol"] != null) {
                        string cache = "CRUDTable_" + this.TableName + "_file_";
                        foreach (string key in this.table.PrimaryKeys) {
                            cache += this.table.GetField(key).Value + "_";
                        }
                        cache += HttpContext.Current.Request.QueryString["tablecol"];

                        DbFile f = null;
                        if (f == null) {
                            this.table.ReadOne(conn);
                            if (this.table.Count > 0) {
                                try {
                                    f = this.table[HttpContext.Current.Request.QueryString["tablecol"]].GetDbFile(0);
                                    HttpContext.Current.Cache.Insert(cache, f, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
                                } catch {
                                    HttpContext.Current.Response.StatusCode = 500;
                                }
                            }
                        }
                        if (f != null) {
                            HttpContext.Current.Response.StatusCode = 200;
                            HttpContext.Current.Response.ContentType = f.MimeType;
                            if (f.MimeType.StartsWith("image/") || f.MimeType.StartsWith("text/")) HttpContext.Current.Response.AddHeader("Content-disposition", "inline; filename=" + f.Name);
                            else HttpContext.Current.Response.AddHeader("Content-disposition", "attachment; filename=" + f.Name);
                            HttpContext.Current.Response.BinaryWrite(f.Data);
                        } else {
                            HttpContext.Current.Response.StatusCode = 404;
                        }
                    } else {
                        HttpContext.Current.Response.StatusCode = 404;
                    }
                    HttpContext.Current.Response.End();
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        /// <exception cref="System.InvalidOperationException">No DB connection information is available. Try setting `Connection' or `ConnectionString'.</exception>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.EnableViewState = false;

            if (this.ConnectionString == null) {
                throw new InvalidOperationException("No DB connection information is available. Try setting `Connection' or `ConnectionString'.");
            }
            if (this.TableName == null) {
                throw new InvalidOperationException("No table name is available. Try setting `TableName'.");
            }

            using (SqlConnection conn = new SqlConnection(this.ConnectionString)) {
                conn.Open();
                if (this.table == null) {
                    // Fuck, threading...
                    // TODO: Make this shit thread safe
                    Table t = (Table)HttpContext.Current.Cache["CRUDTable_" + this.TableName];
                    if (t != null) {
                        this.table = t.Clone();
                    } else {
                        this.table = new Table(this.TableName);
                        this.table.AnalyzeTable(conn);
                        HttpContext.Current.Cache.Insert("CRUDTable_" + this.TableName, this.table, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
                    }
                }
                if (HttpContext.Current.Request.QueryString["action"] != null || HttpContext.Current.Request.HttpMethod == "POST") {
                    this.HandleRequest(conn);
                } else {
                    this.table.Read(conn);
                }
            }
        }

        /// <summary>
        /// This is not the method you're looking for.
        /// </summary>
        /// <param name="output">This is not the parameter you're looking for</param>
        public override void RenderBeginTag(HtmlTextWriter output)
        {
            /* do not remove */
        }

        /// <summary>
        /// Renders the contents.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.AddAttribute("class", "row", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("class", "span" + this.SpanSize.ToString(), false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("class", "page-header");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderBeginTag(HtmlTextWriterTag.H1);
            output.WriteEncodedText(char.ToUpper(this.TableName[0]) + this.TableName.Substring(1));
            output.RenderEndTag(); // h1
            output.RenderEndTag(); // div.page-header
            output.RenderEndTag(); // div.span12
            output.RenderEndTag(); // div.row

            output.AddAttribute("class", "row", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("class", "span" + this.SpanSize.ToString(), false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("class", "table table-bordered table-striped table-hover tablesorter", false);
            output.RenderBeginTag(HtmlTextWriterTag.Table);

            output.RenderBeginTag(HtmlTextWriterTag.Thead);
            output.RenderBeginTag(HtmlTextWriterTag.Tr);

            foreach (string col in this.table.Keys) {
                this.table[col].RenderHeaderCell(output);
            }

            output.RenderBeginTag(HtmlTextWriterTag.Th);
            output.RenderEndTag(); // th

            output.RenderEndTag(); // tr
            output.RenderEndTag(); // thead

            for (int i = 0; i < this.table.Count; ++i) {
                output.RenderBeginTag(HtmlTextWriterTag.Tr);

                foreach (string col in this.table.Keys) {
                    this.table[col].RenderCell(output, i);
                }

                output.RenderBeginTag(HtmlTextWriterTag.Td);

                output.AddAttribute("type", "button", false);
                output.AddAttribute("class", "update-btn btn btn-mini", false);
                output.RenderBeginTag(HtmlTextWriterTag.Button);
                output.AddAttribute("class", "icon-edit", false);
                output.RenderBeginTag(HtmlTextWriterTag.I);
                output.RenderEndTag(); // i
                output.RenderEndTag(); // button
                output.WriteEncodedText(" ");
                output.AddAttribute("type", "button", false);
                output.AddAttribute("class", "delete-btn btn btn-mini btn-danger", false);
                foreach (string key in this.table.PrimaryKeys) {
                    output.AddAttribute("data-" + key, this.table[key].ToString(i), true);
                }
                output.RenderBeginTag(HtmlTextWriterTag.Button);
                output.AddAttribute("class", "icon-remove icon-white", false);
                output.RenderBeginTag(HtmlTextWriterTag.I);
                output.RenderEndTag(); // i
                output.RenderEndTag(); // button

                output.RenderEndTag(); // td
                output.RenderEndTag(); // tr
            }

            output.RenderEndTag(); // table

            output.RenderEndTag(); // div.span12
            output.RenderEndTag(); // div.row

            output.AddAttribute("class", "row", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("class", "span" + this.SpanSize.ToString(), false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("id", "create-form", false);
            output.AddAttribute("action", "?action=create", false);
            output.AddAttribute("method", "post", false);
            output.AddAttribute("enctype", "multipart/form-data", false);
            output.AddAttribute("class", "form-horizontal well", false);
            output.AddAttribute("data-validate", "form bootstrap", false);
            output.RenderBeginTag(HtmlTextWriterTag.Form);

            output.RenderBeginTag(HtmlTextWriterTag.Fieldset);

            output.RenderBeginTag(HtmlTextWriterTag.Legend);
            output.WriteEncodedText("Add record");
            output.RenderEndTag(); // legend

            foreach (string col in this.table.Keys) {
                Field f = this.table.GetField(col);
                if (!f.ReadOnly) {
                    output.AddAttribute("class", "control-group", false);
                    output.RenderBeginTag(HtmlTextWriterTag.Div);
                    f.RenderLabel(output);
                    output.AddAttribute("class", "controls", false);
                    output.RenderBeginTag(HtmlTextWriterTag.Div);
                    f.RenderField(output);
                    output.RenderEndTag(); // div.controls
                    output.RenderEndTag(); // div.control-group
                }
            }

            output.AddAttribute("class", "form-actions");
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("type", "submit", false);
            output.AddAttribute("class", "btn btn-primary", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.AddAttribute("class", "icon-check icon-white", false);
            output.RenderBeginTag(HtmlTextWriterTag.I);
            output.RenderEndTag(); // i
            output.WriteEncodedText(" Save record");
            output.RenderEndTag(); // button.btn
            output.WriteEncodedText(" ");
            output.AddAttribute("type", "reset", false);
            output.AddAttribute("class", "btn", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.WriteEncodedText("Clear");
            output.RenderEndTag(); // button.btn

            output.RenderEndTag(); // div.form-actions

            output.RenderEndTag(); // fieldset
            output.RenderEndTag(); // form

            // UPDATE MODAL
            output.AddAttribute("id", "update-modal", false);
            output.AddAttribute("class", "modal hide fade", false);
            output.AddAttribute("tabindex", "-1", false);
            output.AddAttribute("role", "dialog", false);
            output.AddAttribute("aria-labelledby", "update-modal-label", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("id", "update-form", false);
            output.AddAttribute("action", "?action=update", false);
            output.AddAttribute("method", "post", false);
            output.AddAttribute("enctype", "multipart/form-data", false);
            output.AddAttribute("class", "form-horizontal", false);
            output.AddAttribute("data-validate", "form bootstrap", false);
            output.RenderBeginTag(HtmlTextWriterTag.Form);

            output.AddAttribute("class", "modal-header", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("type", "button", false);
            output.AddAttribute("class", "close", false);
            output.AddAttribute("data-dismiss", "modal", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.WriteEncodedText("x");
            output.RenderEndTag(); // button.close
            output.AddAttribute("id", "update-modal-label", false);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("Update record");
            output.RenderEndTag(); // h3#update-modal-label
            output.RenderEndTag(); // div.modal-header

            output.AddAttribute("class", "modal-body", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            foreach (string col in this.table.Keys) {
                Field f = this.table.GetField(col);
                if (f.ReadOnly) {
                    f.RenderField(output, "update", Field.FieldType.Hidden, true);
                } else {
                    output.AddAttribute("class", "control-group", false);
                    output.RenderBeginTag(HtmlTextWriterTag.Div);
                    f.RenderLabel(output, "update");
                    output.AddAttribute("class", "controls", false);
                    output.RenderBeginTag(HtmlTextWriterTag.Div);
                    f.RenderField(output, "update", Field.FieldType.None, true);
                    output.RenderEndTag(); // div.controls
                    output.RenderEndTag(); // div.control-group
                }
            }

            output.RenderEndTag(); // div.modal-body

            output.AddAttribute("class", "modal-footer");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("type", "submit", false);
            output.AddAttribute("class", "btn btn-primary", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.AddAttribute("class", "icon-edit icon-white", false);
            output.RenderBeginTag(HtmlTextWriterTag.I);
            output.RenderEndTag(); // i
            output.WriteEncodedText(" Save record");
            output.RenderEndTag(); // button.btn
            output.WriteEncodedText(" ");
            output.AddAttribute("type", "button", false);
            output.AddAttribute("class", "btn", false);
            output.AddAttribute("data-dismiss", "modal", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.WriteEncodedText("Cancel");
            output.RenderEndTag(); // button.btn
            output.RenderEndTag(); // div.modal-footer

            output.RenderEndTag(); // form
            output.RenderEndTag(); // div#update-modal
            // END

            // DELETE MODAL
            output.AddAttribute("id", "delete-modal", false);
            output.AddAttribute("class", "modal hide fade", false);
            output.AddAttribute("tabindex", "-1", false);
            output.AddAttribute("role", "dialog", false);
            output.AddAttribute("aria-labelledby", "delete-modal-label", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("id", "delete-form", false);
            output.AddAttribute("action", "?action=delete", false);
            output.AddAttribute("method", "post", false);
            output.AddAttribute("enctype", "multipart/form-data", false);
            output.RenderBeginTag(HtmlTextWriterTag.Form);

            output.AddAttribute("class", "modal-header", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("type", "button", false);
            output.AddAttribute("class", "close", false);
            output.AddAttribute("data-dismiss", "modal", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.WriteEncodedText("x");
            output.RenderEndTag(); // button.close
            output.AddAttribute("id", "delete-modal-label", false);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("Delete record");
            output.RenderEndTag(); // h3#delete-modal-label
            output.RenderEndTag(); // div.modal-header

            output.AddAttribute("class", "modal-body", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderBeginTag(HtmlTextWriterTag.P);
            output.WriteEncodedText("Do you really want to delete this record?");
            output.RenderEndTag(); // p
            output.AddAttribute("id", "delete-modal-table", false);
            output.AddAttribute("class", "table table-bordered table-striped", false);
            output.RenderBeginTag(HtmlTextWriterTag.Table);
            foreach (string col in this.table.Keys) {
                output.RenderBeginTag(HtmlTextWriterTag.Tr);
                this.table[col].RenderHeaderCell(output);
                output.RenderEndTag(); // tr
            }
            output.RenderEndTag(); // table#delete-modal-table
            foreach (string pk in this.table.PrimaryKeys) {
                this.table.GetField(pk).RenderField(output, "delete", Field.FieldType.Hidden);
            }
            output.RenderEndTag(); // div.modal-body

            output.AddAttribute("class", "modal-footer");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("type", "submit", false);
            output.AddAttribute("class", "btn btn-danger", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.AddAttribute("class", "icon-remove icon-white", false);
            output.RenderBeginTag(HtmlTextWriterTag.I);
            output.RenderEndTag(); // i
            output.WriteEncodedText(" Delete record");
            output.RenderEndTag(); // button.btn
            output.WriteEncodedText(" ");
            output.AddAttribute("type", "button", false);
            output.AddAttribute("class", "btn", false);
            output.AddAttribute("data-dismiss", "modal", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.WriteEncodedText("Cancel");
            output.RenderEndTag(); // button.btn
            output.RenderEndTag(); // div.modal-footer

            output.RenderEndTag(); // form
            output.RenderEndTag(); // div#delete-modal
            // END

            // UPLOAD MODAL
            output.AddAttribute("id", "upload-modal", false);
            output.AddAttribute("class", "modal hide fade", false);
            output.AddAttribute("tabindex", "-1", false);
            output.AddAttribute("role", "dialog", false);
            output.AddAttribute("aria-labelledby", "upload-modal-label", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.AddAttribute("data-backdrop", "static", false);
            output.AddAttribute("data-keyboard", "false", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("class", "modal-header", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("id", "upload-modal-label", false);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("Please wait...");
            output.RenderEndTag(); // h3#upload-modal-label
            output.RenderEndTag(); // div.modal-header

            output.AddAttribute("class", "modal-body", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("class", "progress progress-striped active", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("id", "upload-modal-progressbar", false);
            output.AddAttribute("class", "bar", false);
            output.AddAttribute("style", "width: 0%", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderEndTag(); // div#upload-modal-progressbar
            output.RenderEndTag(); // div.progress
            output.AddAttribute("id", "upload-modal-text", false);
            output.RenderBeginTag(HtmlTextWriterTag.Span);
            output.WriteEncodedText("0%");
            output.RenderEndTag(); // span#upload-modal-text
            output.RenderEndTag(); // div.modal-body
            output.RenderEndTag(); // div#upload-modal
            // END

            // SUCCESS MODAL
            output.AddAttribute("id", "success-modal", false);
            output.AddAttribute("class", "modal hide fade", false);
            output.AddAttribute("tabindex", "-1", false);
            output.AddAttribute("role", "dialog", false);
            output.AddAttribute("aria-labelledby", "success-modal-label", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.AddAttribute("data-backdrop", "static", false);
            output.AddAttribute("data-keyboard", "false", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("class", "modal-header alert-success", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("id", "success-modal-label", false);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("Operation successful");
            output.RenderEndTag(); // h3#success-modal-label
            output.RenderEndTag(); // div.modal-header

            output.AddAttribute("class", "modal-body alert-success", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderBeginTag(HtmlTextWriterTag.P);
            output.WriteEncodedText("The requested operation was successful!");
            output.RenderEndTag(); // p.text-success
            output.RenderEndTag(); // div.modal-body

            output.AddAttribute("class", "modal-footer", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("id", "success-modal-btn", false);
            output.AddAttribute("type", "button", false);
            output.AddAttribute("class", "btn btn-primary", false);
            output.AddAttribute("data-dismiss", "modal", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.AddAttribute("class", "icon-ok icon-white", false);
            output.RenderBeginTag(HtmlTextWriterTag.I);
            output.RenderEndTag(); // i
            output.WriteEncodedText(" OK");
            output.RenderEndTag(); // button.btn
            output.RenderEndTag(); // div.modal-footer

            output.RenderEndTag(); // div#success-modal
            // END

            // ERROR MODAL
            output.AddAttribute("id", "error-modal", false);
            output.AddAttribute("class", "modal hide fade", false);
            output.AddAttribute("tabindex", "-1", false);
            output.AddAttribute("role", "dialog", false);
            output.AddAttribute("aria-labelledby", "success-modal-label", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.AddAttribute("data-backdrop", "static", false);
            output.AddAttribute("data-keyboard", "false", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.AddAttribute("class", "modal-header alert-error", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("id", "error-modal-label", false);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("Error");
            output.RenderEndTag(); // h3#error-modal-label
            output.RenderEndTag(); // div.modal-header

            output.AddAttribute("class", "modal-body alert-error", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderBeginTag(HtmlTextWriterTag.P);
            output.WriteEncodedText("The requested operation failed! Check your inputs and try again.");
            output.RenderEndTag(); // p.text-error
            output.RenderEndTag(); // div.modal-body

            output.AddAttribute("class", "modal-footer", false);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute("id", "error-modal-btn", false);
            output.AddAttribute("type", "button", false);
            output.AddAttribute("class", "btn btn-primary", false);
            output.AddAttribute("data-dismiss", "modal", false);
            output.AddAttribute("aria-hidden", "true", false);
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.AddAttribute("class", "icon-ok icon-white", false);
            output.RenderBeginTag(HtmlTextWriterTag.I);
            output.RenderEndTag(); // i
            output.WriteEncodedText(" OK");
            output.RenderEndTag(); // button.btn
            output.RenderEndTag(); // div.modal-footer

            output.RenderEndTag(); // div#error-modal
            // END

            output.RenderEndTag(); // div.span12
            output.RenderEndTag(); // div.row

            output.AddAttribute("src", "js/validate.js", false);
            output.RenderBeginTag(HtmlTextWriterTag.Script);
            output.RenderEndTag(); // script
            output.AddAttribute("src", "js/CRUDTable.js", false);
            output.RenderBeginTag(HtmlTextWriterTag.Script);
            output.RenderEndTag(); // script
        }

        /// <summary>
        /// If you want your code to make useless calls, then this method is a good candidat.
        /// </summary>
        /// <param name="output">Don't even bother passing something valid here, cast anything and it should do the job</param>
        public override void RenderEndTag(HtmlTextWriter output)
        {
            // Magic, do not touch
        }
    }
}
