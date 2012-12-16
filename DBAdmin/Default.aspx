<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DBAdmin.Default" %>
<%@ Register Assembly="CRUDTable" Namespace="CRUDTable" TagPrefix="TP1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Container" runat="server">
<TP1:CRUDTable ID="CRUDTable1" runat="server" TableName="users" SpanSize="12" />
  <script src="js/CRUDTable.js"></script>
  <script>
    $(".tablesorter").each(function (i, e) {
      var options = {
        cssAsc: 'header-up',
        cssDesc: 'header-down',
        cssHeader: '',
        headers: {}
      };
      options.headers[($(e).find("thead tr th").length - 1)] = { sorter: false };
      $(e).tablesorter(options);
    });
  </script>
</asp:Content>
