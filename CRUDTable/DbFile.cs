using System;
using System.Runtime.Serialization;

namespace CRUDTable
{

    [Serializable()]
    public class DbFile : ISerializable
    {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the MIME type.
        /// </summary>
        /// <value>
        /// The MIME type.
        /// </value>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbFile" /> class.
        /// </summary>
        public DbFile()
        {
            this.Name = "Unknown";
            this.MimeType = "application/octet-stream";
        }

        public DbFile Clone()
        {
            DbFile f = new DbFile();
            f.Name = this.Name;
            f.MimeType = this.MimeType;
            f.Data = (byte[])this.Data.Clone();
            return f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbFile" /> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="ctxt">The context.</param>
        public DbFile(SerializationInfo info, StreamingContext ctxt)
        {
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.MimeType = (string)info.GetValue("MimeType", typeof(string));
            this.Data = (byte[])info.GetValue("Data", typeof(byte[]));
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="ctxt">The context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Name", this.Name);
            info.AddValue("MimeType", this.MimeType);
            info.AddValue("Data", this.Data);
        }

    }
}
