<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DBAdmin.Default" %>
<%@ Register Assembly="CRUDTable" Namespace="CRUDTable" TagPrefix="TP1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Container" runat="server">
  <div class="row">
    <div class="span12">
      <div class="page-header">
        <h1>Users</h1>
      </div>
    </div>
  </div>
  <!--[if IE]>
  <div class="row">
    <div class="span12">
      <div class="alert alert-error">
        <strong>Warning!</strong> Use a better browser like <a href="http://www.google.com/chrome">Google Chrome</a> right now! Or else kittens will die on each page load!
      </div>
    </div>
  </div>
  <![endif]-->
<TP1:CRUDTable ID="CRUDTable1" runat="server" TableName="users" SpanSize="12" />
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
