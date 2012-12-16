using System;

namespace Table
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.CRUDTable1.ConnectionString = (string)Application["ConnectionString"];
        }
    }
}
