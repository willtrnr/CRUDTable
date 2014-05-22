using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.UI;

namespace CRUDTable
{
    public class Column
    {

        public enum ColumnType { Text, Int, Numeric, Date, DateTime, File };

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public ColumnType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Column" /> is a primary key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if primary key; otherwise, <c>false</c>.
        /// </value>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// The rows
        /// </summary>
        protected List<object> rows = new List<object>();

        /// <summary>
        /// The primary keys
        /// </summary>
        protected Dictionary<string, Column> primaryKeys = new Dictionary<string, Column>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Column" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="title">The title.</param>
        /// <param name="type">The type.</param>
        /// <param name="pk">if set to <c>true</c> the column is a primary key.</param>
        public Column(string name, string title = null, ColumnType type = ColumnType.Text, bool pk = false)
        {
            this.Name = name;
            this.Title = (title == null) ? name : title;
            this.Type = type;
            this.PrimaryKey = pk;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Column Clone()
        {
            return new Column(this.Name, this.Title, this.Type, this.PrimaryKey);
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object" /> with the specified row.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object" />.
        /// </value>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public object this[int row]
        {
            get
            {
                if (row < this.rows.Count) {
                    return this.rows[row];
                } else {
                    return null;
                }
            }

            set
            {
                if (row < this.rows.Count) {
                    this.rows[row] = (object)value;
                } else {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Adds the primary key.
        /// </summary>
        /// <param name="col">The column.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void AddPrimaryKey(Column col)
        {
            if (col.PrimaryKey) {
                this.primaryKeys.Add(col.Name, col);
            } else {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(object value)
        {
            this.rows.Add(value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this.rows.Clear();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public string ToString(int row)
        {
            if (row < this.rows.Count) {
                if (this.rows[row] != null && this.rows[row] != DBNull.Value) {
                    switch (this.Type) {
                        case ColumnType.Int:
                            return ((int)this.rows[row]).ToString();
                        case ColumnType.Numeric:
                            return ((decimal)this.rows[row]).ToString();
                        case ColumnType.Date:
                            return ((DateTime)this.rows[row]).ToString("yyyy-MM-dd");
                        case ColumnType.DateTime:
                            return ((DateTime)this.rows[row]).ToString("yyyy-MM-dd HH:mm:ss");
                        case ColumnType.File:
                            using (MemoryStream s = new MemoryStream()) {
                                try {
                                    s.Write((byte[])this.rows[row], 0, ((byte[])this.rows[row]).Length);
                                    s.Seek(0, SeekOrigin.Begin);
                                    BinaryFormatter bf = new BinaryFormatter();
                                    DbFile f = (DbFile)bf.Deserialize(s);
                                    return f.Name;
                                } catch (Exception ex) {
                                    return ex.Message;
                                }
                            }
                        case ColumnType.Text:
                        default:
                            return this.rows[row].ToString();
                    }
                } else {
                    return "";
                }
            } else {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Renders the header cell.
        /// </summary>
        /// <param name="output">The output.</param>
        public void RenderHeaderCell(HtmlTextWriter output)
        {
            output.RenderBeginTag(HtmlTextWriterTag.Th);
            output.WriteEncodedText((this.Name.ToLower() == "id") ? "ID" : char.ToUpper(this.Name[0]) + this.Name.Substring(1));
            output.RenderEndTag();
        }

        /// <summary>
        /// Renders the cell.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="row">The row.</param>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public void RenderCell(HtmlTextWriter output, int row)
        {
            if (row < this.rows.Count) {
                switch (this.Type) {
                    case ColumnType.File:
                        output.RenderBeginTag(HtmlTextWriterTag.Td);
                        if (this.rows[row] != null && this.rows[row] != DBNull.Value) {
                            using (MemoryStream s = new MemoryStream()) {
                                try {
                                    DbFile f = this.GetDbFile(row);
                                    string url = "?action=file&tablecol=" + HttpUtility.UrlEncode(this.Name);
                                    foreach (string key in this.primaryKeys.Keys) {
                                        url += "&" + HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(this.primaryKeys[key].ToString(row));
                                    }
                                    output.AddAttribute("href", url, true);
                                    output.RenderBeginTag(HtmlTextWriterTag.A);
                                    if (f.MimeType.StartsWith("image/")) {
                                        output.AddAttribute("src", url, true);
                                        output.RenderBeginTag(HtmlTextWriterTag.Img);
                                        output.RenderEndTag();
                                    } else {
                                        output.WriteEncodedText(f.Name);
                                    }
                                    output.RenderEndTag();
                                } catch (Exception ex) {
                                    output.WriteEncodedText(ex.Message);
                                }
                            }
                        }
                        output.RenderEndTag();
                        break;
                    default:
                        output.RenderBeginTag((this.PrimaryKey) ? HtmlTextWriterTag.Th : HtmlTextWriterTag.Td);
                        output.WriteEncodedText(this.ToString(row));
                        output.RenderEndTag();
                        break;
                }
            } else {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Renders the template cell.
        /// </summary>
        /// <param name="output">The output.</param>
        public void RenderTemplateCell(HtmlTextWriter output)
        {
            output.RenderBeginTag((this.PrimaryKey) ? HtmlTextWriterTag.Th : HtmlTextWriterTag.Td);
            output.WriteEncodedText("${" + this.Name + "}");
            output.RenderEndTag();
        }

        /// <summary>
        /// Gets the db file.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public DbFile GetDbFile(int row)
        {
            if (row < this.rows.Count) {
                if (this.rows[row] != null && this.rows[row] != DBNull.Value) {
                    using (MemoryStream s = new MemoryStream()) {
                        s.Write((byte[])this.rows[row], 0, ((byte[])this.rows[row]).Length);
                        s.Seek(0, SeekOrigin.Begin);
                        BinaryFormatter bf = new BinaryFormatter();
                        return (DbFile)bf.Deserialize(s);
                    }
                } else {
                    throw new InvalidOperationException();
                }
            } else {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
