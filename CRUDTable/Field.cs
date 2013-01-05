using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CRUDTable
{
    public class Field
    {
        public enum FieldType { None, Hidden, Text, TextArea, Number, Date, DateTime, Checkbox, Radio, Select, File };

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FieldType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public SqlDbType DataType { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Field" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Field" /> is read only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if read only; otherwise, <c>false</c>.
        /// </value>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>
        /// The maximum length.
        /// </value>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>
        /// The precision.
        /// </value>
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public byte Scale { get; set; }

        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the foreign table.
        /// </summary>
        /// <value>
        /// The foreign table.
        /// </value>
        public string ForeignTable { get; set; }

        /// <summary>
        /// The options
        /// </summary>
        protected List<string> options = new List<string>();

        /// <summary>
        /// The validators
        /// </summary>
        protected List<Validators.IValidator> validators = new List<Validators.IValidator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="label">The label.</param>
        /// <param name="type">The type.</param>
        public Field(string name, string label = null, FieldType type = FieldType.Text)
        {
            this.Name = name;
            this.Label = (label == null) ? name : label;
            this.Type = type;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Field Clone()
        {
            Field f = new Field(this.Name, this.Label, this.Type);
            f.DataType = this.DataType;
            f.Required = this.Required;
            f.ReadOnly = this.ReadOnly;
            f.MaxLength = this.MaxLength;
            f.Precision = this.Precision;
            f.Scale = this.Scale;
            f.ForeignKey = this.ForeignKey;
            f.ForeignTable = this.ForeignTable;
            foreach (string o in this.options) {
                f.AddOption(o);
            }
            foreach (Validators.IValidator v in this.validators) {
                f.AddValidator(v);
            }
            return f;
        }

        /// <summary>
        /// Adds the option.
        /// </summary>
        /// <param name="option">The option.</param>
        public void AddOption(string option)
        {
            if (option != null) {
                if (option.Length > this.MaxLength) this.MaxLength = option.Length;
                this.options.Add(option);
            }
        }

        /// <summary>
        /// Fetches the options.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <exception cref="System.InvalidOperationException">No FK data is available, try setting `ForeignKey' and `ForeignTable' manually.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void FetchOptions(SqlConnection conn)
        {
            if (this.ForeignKey == null || this.ForeignTable == null) {
                throw new InvalidOperationException("No FK data is available, try setting `ForeignKey' and `ForeignTable' manually.");
            }
            this.options.Clear();
            SqlCommand fk = conn.CreateCommand();
            fk.CommandText = "SELECT * FROM " + this.ForeignTable;
            using (SqlDataReader fkResults = fk.ExecuteReader()) {
                while (fkResults.Read()) {
                    this.AddOption((string)fkResults[this.ForeignKey]);
                }
            }
        }

        /// <summary>
        /// Adds the validator.
        /// </summary>
        /// <param name="validator">The validator.</param>
        public void AddValidator(Validators.IValidator validator)
        {
            if (validator != null) {
                this.validators.Add(validator);
            }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            foreach (Validators.IValidator validator in this.validators) {
                if (!validator.Validate(this.Value.ToString())) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the size class.
        /// </summary>
        /// <returns></returns>
        protected string GetSizeClass()
        {
            if (this.MaxLength <= 5) return "input-mini ";
            if (this.MaxLength <= 10) return "input-small ";
            if (this.MaxLength <= 20) return "input-medium ";
            if (this.MaxLength <= 30) return "input-large ";
            if (this.MaxLength <= 40) return "input-xlarge ";
            return "input-xxlarge ";
        }

        /// <summary>
        /// Renders the input tag.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="type">The type.</param>
        /// <param name="ns">The name space.</param>
        protected void RenderInputTag(HtmlTextWriter output, string type, string ns = null)
        {
            if (ns != null && !ns.EndsWith("-")) {
                ns += "-";
            }
            string classes = "";
            string validate = "";
            output.AddAttribute("id", ns + this.Name, true);
            output.AddAttribute("type", type, false);
            output.AddAttribute("name", this.Name, true);
            if (this.Value != null) output.AddAttribute("value", this.Value.ToString(), true);
            if (type != "file") output.AddAttribute("placeholder", char.ToUpper(this.Label[0]) + this.Label.Substring(1), true);
            if (this.Required) {
                output.AddAttribute("required", "required", false);
                validate += "notnull ";
            }
            if (this.ReadOnly) {
                output.AddAttribute("disabled", "disabled");
                classes += "disabled ";
            }
            if (this.DataType == SqlDbType.Decimal || this.DataType == SqlDbType.Float) {
                output.AddAttribute("step", "any", false);
                classes += "input-small ";
                validate += "dec ";
            } else if (type == "number") {
                classes += "input-small ";
                validate += "num ";
            } else if (type == "date") {
                classes += "input-medium ";
                validate += "date ";
            } else if (type == "text") {
                classes += GetSizeClass() + " ";
                validate += "maxlen=" + this.MaxLength.ToString() + " ";
            }
            if (classes != "") output.AddAttribute("class", classes.Trim(), false);
            if (validate != "") output.AddAttribute("data-validate", validate + "onkey", false);
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();
        }

        /// <summary>
        /// Renders the field.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="ns">The name space.</param>
        /// <param name="overrideType">Override the type.</param>
        /// <param name="enableFileRemove">if set to <c>true</c> enables file removal controls.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RenderField(HtmlTextWriter output, string ns = null, FieldType overrideType = FieldType.None, bool enableFileRemove = false)
        {
            if (ns != null && !ns.EndsWith("-")) {
                ns += "-";
            }
            switch ((overrideType != FieldType.None) ? overrideType : this.Type) {
                case FieldType.Hidden:
                    output.AddAttribute("id", ns + this.Name, true);
                    output.AddAttribute("type", "hidden", false);
                    output.AddAttribute("name", this.Name, true);
                    if (this.Value != null) output.AddAttribute("value", this.Value.ToString(), true);
                    output.RenderBeginTag(HtmlTextWriterTag.Input);
                    output.RenderEndTag();
                    break;
                case FieldType.TextArea:
                    output.AddAttribute("id", ns + this.Name, true);
                    output.AddAttribute("name", this.Name, true);
                    if (this.Required) output.AddAttribute("required", "required", false);
                    if (this.ReadOnly) {
                        output.AddAttribute("class", "disabled", false);
                        output.AddAttribute("disabled", "disabled", false);
                    }
                    output.RenderBeginTag(HtmlTextWriterTag.Textarea);
                    if (this.Value != null) output.WriteEncodedText(this.Value.ToString());
                    output.RenderEndTag();
                    break;
                case FieldType.Number:
                    this.RenderInputTag(output, "number", ns);
                    break;
                case FieldType.Date:
                    this.RenderInputTag(output, "date", ns);
                    break;
                case FieldType.DateTime:
                    this.RenderInputTag(output, "datetime", ns);
                    break;
                case FieldType.Checkbox:
                case FieldType.Radio:
                    throw new NotImplementedException();
                case FieldType.Select:
                    string classes = GetSizeClass() + " ";
                    output.AddAttribute("id", ns + this.Name, true);
                    output.AddAttribute("name", this.Name, true);
                    if (this.Required) output.AddAttribute("required", "required", false);
                    if (this.ReadOnly) {
                        output.AddAttribute("class", "disabled", false);
                        output.AddAttribute("disabled", "disabled", false);
                    }
                    if (classes != "") output.AddAttribute("class", classes.Trim());
                    output.RenderBeginTag(HtmlTextWriterTag.Select);
                    output.AddAttribute("value", "", false);
                    output.RenderBeginTag(HtmlTextWriterTag.Option);
                    output.RenderEndTag();
                    foreach (string val in this.options) {
                        output.AddAttribute("value", val, true);
                        if (this.Value != null && val == this.Value.ToString()) output.AddAttribute("selected", "selected");
                        output.RenderBeginTag(HtmlTextWriterTag.Option);
                        output.WriteEncodedText(val);
                        output.RenderEndTag();
                    }
                    output.RenderEndTag();
                    break;
                case FieldType.File:
                    this.RenderInputTag(output, "file", ns);
                    if (enableFileRemove) {
                        output.AddAttribute("for", ns + this.Name + "-remove");
                        output.AddAttribute("class", "checkbox");
                        output.RenderBeginTag(HtmlTextWriterTag.Label);
                        output.AddAttribute("id", ns + this.Name + "-remove", false);
                        output.AddAttribute("type", "checkbox", false);
                        output.AddAttribute("name", this.Name + "-remove", false);
                        output.AddAttribute("value", "1", false);
                        output.AddAttribute("data-ignore", "1", false);
                        output.RenderBeginTag(HtmlTextWriterTag.Input);
                        output.RenderEndTag();
                        output.WriteEncodedText(" Remove");
                        output.RenderEndTag();
                    }
                    break;
                case FieldType.Text:
                default:
                    this.RenderInputTag(output, "text", ns);
                    break;
            }
        }

        /// <summary>
        /// Renders the label.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="ns">The name space.</param>
        public void RenderLabel(HtmlTextWriter output, string ns = null)
        {
            if (ns != null && !ns.EndsWith("-")) {
                ns += "-";
            }
            output.AddAttribute("class", "control-label", false);
            output.AddAttribute("for", ns + this.Name, true);
            output.RenderBeginTag(HtmlTextWriterTag.Label);
            output.WriteEncodedText((this.Label.ToLower() == "id") ? "ID" : char.ToUpper(this.Label[0]) + this.Label.Substring(1));
            output.RenderEndTag();
        }
    }
}
