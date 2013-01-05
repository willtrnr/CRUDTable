using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace CRUDTable
{
    public class Table
    {

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// The fields
        /// </summary>
        protected Dictionary<string, Field> fields = new Dictionary<string, Field>();

        /// <summary>
        /// The columns
        /// </summary>
        protected Dictionary<string, Column> columns = new Dictionary<string, Column>();

        /// <summary>
        /// The primary keys
        /// </summary>
        protected List<string> primaryKeys = new List<string>();

        /// <summary>
        /// The row count
        /// </summary>
        protected int rowCount = 0;

        /// <summary>
        /// Gets the <see cref="Column" /> with the specified column.
        /// </summary>
        /// <value>
        /// The <see cref="Column" />.
        /// </value>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public Column this[string column]
        {
            get
            {
                if (this.columns.ContainsKey(column)) {
                    return this.columns[column];
                } else {
                    throw new KeyNotFoundException();
                }
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<string> Keys
        {
            get
            {
                return this.fields.Keys;
            }
        }

        /// <summary>
        /// Gets the primary keys.
        /// </summary>
        /// <value>
        /// The primary keys.
        /// </value>
        public IEnumerable<string> PrimaryKeys
        {
            get
            {
                return this.primaryKeys;
            }
        }

        /// <summary>
        /// Gets the row count.
        /// </summary>
        /// <value>
        /// The row count.
        /// </value>
        public int Count
        {
            get
            {
                return this.rowCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table" /> class.
        /// </summary>
        /// <param name="tablename">The name of the table.</param>
        public Table(string tablename = null)
        {
            this.TableName = tablename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table" /> class.
        /// </summary>
        /// <param name="tablename">The name of the table.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="primarykeys">The primary keys.</param>
        public Table(string tablename, Dictionary<string, Field> fields, Dictionary<string, Column> columns, List<string> primarykeys)
        {
            this.TableName = tablename;
            foreach (string key in fields.Keys) {
                this.fields.Add(key, fields[key].Clone());
            }
            foreach (string key in columns.Keys) {
                this.columns.Add(key, columns[key].Clone());
            }
            foreach (string key in primarykeys) {
                this.primaryKeys.Add(key);
                foreach (string col in this.columns.Keys) {
                    if (this.columns[col].Type == Column.ColumnType.File) {
                        this.columns[col].AddPrimaryKey(this.columns[key]);
                    }
                }
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Table Clone()
        {
            return new Table(this.TableName, this.fields, this.columns, this.primaryKeys);
        }

        /// <summary>
        /// Determines whether the table has primary keys.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the table has primary keys; otherwise, <c>false</c>.
        /// </returns>
        public bool HasPrimaryKeys()
        {
            return this.primaryKeys.Count > 0;
        }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public Field GetField(string column)
        {
            if (this.fields.ContainsKey(column)) {
                return this.fields[column];
            } else {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Analyzes the table.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <exception cref="System.Exception">Failed to parse the reference string for the field ` + f.Name + '.</exception>
        public void AnalyzeTable(SqlConnection conn)
        {
            this.fields.Clear();
            this.columns.Clear();
            this.primaryKeys.Clear();
            this.rowCount = 0;

            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_help @objname = @table";
            SqlParameter tableName = new SqlParameter("@table", SqlDbType.VarChar, 776);
            tableName.Value = this.TableName;
            cmd.Parameters.Add(tableName);
            cmd.Prepare();

            using (SqlDataReader results = cmd.ExecuteReader()) {
                results.NextResult();
                while (results.Read()) {
                    Field f = new Field((string)results["Column_name"]);
                    Column c = new Column((string)results["Column_name"]);
                    switch ((string)results["Type"]) {
                        case "int":
                            f.Type = Field.FieldType.Number;
                            f.DataType = SqlDbType.Int;
                            f.AddValidator(new Validators.NumValidator());
                            c.Type = Column.ColumnType.Int;
                            break;
                        case "numeric":
                            f.Type = Field.FieldType.Number;
                            f.DataType = SqlDbType.Decimal;
                            f.Precision = byte.Parse(((string)results["Prec"]).Trim());
                            f.Scale = byte.Parse(((string)results["Scale"]).Trim());
                            f.AddValidator(new Validators.DecValidator());
                            c.Type = Column.ColumnType.Numeric;
                            break;
                        case "text":
                            f.Type = Field.FieldType.TextArea;
                            f.DataType = SqlDbType.Text;
                            c.Type = Column.ColumnType.Text;
                            break;
                        case "date":
                            f.Type = Field.FieldType.Date;
                            f.DataType = SqlDbType.Date;
                            f.AddValidator(new Validators.DateValidator());
                            c.Type = Column.ColumnType.Date;
                            break;
                        case "datetime":
                            f.Type = Field.FieldType.DateTime;
                            f.DataType = SqlDbType.DateTime;
                            c.Type = Column.ColumnType.DateTime;
                            break;
                        case "varbinary":
                            f.Type = Field.FieldType.File;
                            f.DataType = SqlDbType.VarBinary;
                            c.Type = Column.ColumnType.File;
                            break;
                        case "varchar":
                        default:
                            f.Type = Field.FieldType.Text;
                            f.DataType = SqlDbType.VarChar;
                            c.Type = Column.ColumnType.Text;
                            break;
                    }
                    f.MaxLength = (int)results["Length"];
                    f.Required = ((string)results["Nullable"] == "no");
                    if (f.Required) f.AddValidator(new Validators.NotNullValidator());
                    this.fields.Add((string)results["Column_name"], f);
                    this.columns.Add((string)results["Column_name"], c);
                }
                results.NextResult();
                while (results.Read()) {
                    if (this.fields.ContainsKey((string)results["Identity"])) {
                        this.fields[(string)results["Identity"]].ReadOnly = true;
                    }
                }
                results.NextResult();
                results.NextResult();
                results.NextResult();
                results.NextResult();
                while (results.Read()) {
                    if ((string)results["constraint_type"] == "FOREIGN KEY") {
                        Field f = this.fields[(string)results["constraint_keys"]];
                        f.Type = Field.FieldType.Select;
                        if (results.Read()) {
                            Regex r = new Regex(@"\.(\w+) \((\w+)\)$");
                            Match m = r.Match((string)results["constraint_keys"]);
                            if (m.Groups.Count == 3) {
                                f.ForeignTable = m.Groups[1].Captures[0].ToString();
                                f.ForeignKey = m.Groups[2].Captures[0].ToString();
                                f.FetchOptions(conn);
                            } else {
                                throw new Exception("Failed to parse the reference string for the field `" + f.Name + "'.");
                            }
                        } else {
                            throw new Exception("Failed to parse the foreign key for the field `" + f.Name + "'.");
                        }
                    } else if ((string)results["constraint_type"] == "PRIMARY KEY" || (string)results["constraint_type"] == "PRIMARY KEY (clustered)") {
                        string[] keys = ((string)results["constraint_keys"]).Split(',');
                        foreach (string key in keys) {
                            string trimmed = key.Trim();
                            if (this.fields.ContainsKey(trimmed)) {
                                this.fields[trimmed].Required = true;
                            }
                            if (this.columns.ContainsKey(trimmed)) {
                                this.columns[trimmed].PrimaryKey = true;
                            }
                            this.primaryKeys.Add(trimmed);
                            foreach (string col in this.columns.Keys) {
                                if (this.columns[col].Type == Column.ColumnType.File) {
                                    this.columns[col].AddPrimaryKey(this.columns[trimmed]);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the record contained in the fields.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="forceanalyze">if set to <c>true</c> force the analysis of the table.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public bool Create(SqlConnection conn, bool forceanalyze = false)
        {
            if (forceanalyze || this.columns.Count == 0 || this.fields.Count == 0 || this.columns.Count != this.fields.Count) {
                this.AnalyzeTable(conn);
            }

            if (this.primaryKeys.Count > 0) {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO " + this.TableName + " (";
                foreach (string key in this.Keys) {
                    Field f = this.fields[key];
                    if (!f.ReadOnly) {
                        cmd.CommandText += key + ", ";
                    }
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + ") VALUES (";
                foreach (string key in this.Keys) {
                    Field f = this.fields[key];
                    if (!f.Validate()) return false;
                    if (!f.ReadOnly) {
                        cmd.CommandText += "@" + key + ", ";
                        SqlParameter param = new SqlParameter("@" + key, f.DataType, f.MaxLength);
                        param.Precision = f.Precision;
                        param.Scale = f.Scale;
                        param.Value = (f.Value == null) ? DBNull.Value : f.Value;
                        param.IsNullable = !f.Required;
                        cmd.Parameters.Add(param);
                    }
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + ")";
                cmd.Prepare();
                return (cmd.ExecuteNonQuery() > 0);
            } else {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="forceanalyze">if set to <c>true</c> force the analysis of the table.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void Read(SqlConnection conn, bool forceanalyze = false)
        {
            if (forceanalyze || this.columns.Count == 0 || this.fields.Count == 0 || this.columns.Count != this.fields.Count) {
                this.AnalyzeTable(conn);
            }

            foreach (string key in this.columns.Keys) {
                this.columns[key].Clear();
            }
            this.rowCount = 0;

            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT " + string.Join(", ", this.columns.Keys.ToArray()) + " FROM " + this.TableName;
            using (SqlDataReader results = cmd.ExecuteReader()) {
                while (results.Read()) {
                    foreach (string col in this.columns.Keys) {
                        this.columns[col].Add(results[col]);
                    }
                    this.rowCount++;
                }
            }
        }

        /// <summary>
        /// Reads one row matching the value of the fields.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="forceanalyze">if set to <c>true</c> force the analysis of the table.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void ReadOne(SqlConnection conn, bool forceanalyze = false)
        {
            if (forceanalyze || this.columns.Count == 0 || this.fields.Count == 0 || this.columns.Count != this.fields.Count) {
                throw new InvalidOperationException();
            }

            foreach (string key in this.columns.Keys) {
                this.columns[key].Clear();
            }
            this.rowCount = 0;

            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT " + string.Join(", ", this.columns.Keys.ToArray()) + " FROM " + this.TableName + " WHERE ";
            foreach (string key in this.primaryKeys) {
                Field f = this.fields[key];
                cmd.CommandText += key + "=@pk_" + key + " AND ";
                SqlParameter param = new SqlParameter("@pk_" + key, f.DataType, f.MaxLength);
                param.Precision = f.Precision;
                param.Scale = f.Scale;
                param.Value = f.Value;
                cmd.Parameters.Add(param);
            }
            cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 5);
            cmd.Prepare();
            using (SqlDataReader results = cmd.ExecuteReader()) {
                while (results.Read()) {
                    foreach (string col in this.columns.Keys) {
                        this.columns[col].Add(results[col]);
                    }
                    this.rowCount++;
                }
            }
        }

        /// <summary>
        /// Updates the record contained in the fields.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="forceanalyze">if set to <c>true</c> force the analysis of the table.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public bool Update(SqlConnection conn, bool forceanalyze = false)
        {
            if (forceanalyze || this.columns.Count == 0 || this.fields.Count == 0 || this.columns.Count != this.fields.Count) {
                this.AnalyzeTable(conn);
            }

            if (this.primaryKeys.Count > 0) {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE " + this.TableName + " SET ";
                foreach (string key in this.Keys) {
                    Field f = this.fields[key];
                    if (!f.ReadOnly && f.Value != null) {
                        cmd.CommandText += key + "=@" + key + ", ";
                        SqlParameter param = new SqlParameter("@" + key, f.DataType, f.MaxLength);
                        param.Precision = f.Precision;
                        param.Scale = f.Scale;
                        param.Value = f.Value;
                        param.IsNullable = !f.Required;
                        cmd.Parameters.Add(param);
                    }
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + " WHERE ";
                foreach (string key in this.primaryKeys) {
                    Field f = this.fields[key];
                    if (!f.Validate()) return false;
                    cmd.CommandText += key + "=@pk_" + key + " AND ";
                    SqlParameter param = new SqlParameter("@pk_" + key, f.DataType, f.MaxLength);
                    param.Precision = f.Precision;
                    param.Scale = f.Scale;
                    param.Value = f.Value;
                    cmd.Parameters.Add(param);
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 5);
                cmd.Prepare();
                return (cmd.ExecuteNonQuery() > 0);
            } else {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Deletes the record contained in the fields.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="forceanalyze">if set to <c>true</c> force the analysis of the table.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public bool Delete(SqlConnection conn, bool forceanalyze = false)
        {
            if (forceanalyze || this.columns.Count == 0 || this.fields.Count == 0 || this.columns.Count != this.fields.Count) {
                this.AnalyzeTable(conn);
            }
            
            if (this.primaryKeys.Count > 0) {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM " + this.TableName + " WHERE ";
                foreach (string key in this.primaryKeys) {
                    Field f = this.fields[key];
                    cmd.CommandText += key + "=@" + key + " AND ";
                    SqlParameter param = new SqlParameter("@" + key, f.DataType, f.MaxLength);
                    param.Precision = f.Precision;
                    param.Scale = f.Scale;
                    param.Value = f.Value;
                    cmd.Parameters.Add(param);
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 5);
                cmd.Prepare();
                return (cmd.ExecuteNonQuery() > 0);
            } else {
                throw new InvalidOperationException();
            }
        }

    }
}
