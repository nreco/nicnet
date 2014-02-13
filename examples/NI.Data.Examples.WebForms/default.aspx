<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="NI.Data" %>

<script runat="server" language="c#">


protected override void OnLoad(EventArgs e) {
	base.OnLoad(e);

}

protected void bookDS_Init(object sender, EventArgs e) {
	bookDS.Dalc = NI.Data.Examples.WebForms.App_Code.DataHelper.GetDalc();
}
</script>

<html>
<head>
	<title>NI.Data - WebForms Example</title>
	
	<script src="http://code.jquery.com/jquery-1.10.1.min.js"></script>
	<link href="http://netdna.bootstrapcdn.com/bootstrap/3.1.0/css/bootstrap.min.css" rel="stylesheet">
	<script src="http://netdna.bootstrapcdn.com/bootstrap/3.1.0/js/bootstrap.min.js"></script>
</head>

<body>
<form runat="server">
<div class="container">
	<div class="row">
		<div class="col-md-12">
			<div class="pull-left">
				<h1>
					NI.Data
					<small>Data access layer components</small>
				</h1>
			</div>
			<div class="clearfix"></div>
		</div>
    </div>
	<hr />


	<div class="row">
		<div class="col-md-12">
			<h2>ListView</h2>


			<Dalc:DalcDataSource runat="server" id="bookDS" 
				OnInit="bookDS_Init"
				TableName="books" 
				DataSetMode="true" 
				DataKeyNames="id" 
				AutoIncrementNames="id" InsertMode="false"  />
			
			<asp:ListView ID="booksList" runat="server" DataSourceID="bookDS">
				<LayoutTemplate>
					<table class="table">
						<thead>
							<tr>
								<th>Title</th>
								<th>Description</th>
								<th>Rating</th>
								<th>Actions</th>
							</tr>
						</thead>
						<tbody>
							<tr runat="server" id="itemPlaceholder" />
						</tbody>
					</table>
				</LayoutTemplate>
				<ItemTemplate>
					<tr>
						<td><%# HttpUtility.HtmlEncode( Convert.ToString(Eval("title")) ) %></td>
						<td><%# HttpUtility.HtmlEncode( Convert.ToString(Eval("description")) ) %></td>
						<td><%# Eval("rating") %></td>
						<td nowrap="nowrap">
							<asp:LinkButton ID="edit" runat="server" CommandName="Edit" Text="Edit" />
							|
							<asp:LinkButton ID="delete" runat="server" CommandName="Delete" Text="Delete" />
						</td>
					</tr>
				</ItemTemplate>
				<EditItemTemplate>
					<tr>
						<td>
							<asp:TextBox id="title" runat="server" Text='<%# Bind("title") %>' />
						</td>
						<td>
							<asp:TextBox id="description" TextMode="MultiLine" Columns="60" Rows="3" runat="server" Text='<%# Bind("description") %>' />
						</td>
						<td>
							<asp:DropDownList ID="rating" runat="server" SelectedValue='<%# Bind("rating") %>'>
								<asp:ListItem Value="1" Text="1"/>
								<asp:ListItem Value="2" Text="2"/>
								<asp:ListItem Value="3" Text="3"/>
								<asp:ListItem Value="4" Text="4"/>
								<asp:ListItem Value="5" Text="5"/>
							</asp:DropDownList>
						</td>
						<td>
							<asp:LinkButton ID="upd" CssClass="btn btn-primary btn-sm" runat="server" CommandName="Update" Text="Save" />

							<asp:LinkButton ID="cancel" CssClass="btn btn-default btn-sm" runat="server" CommandName="Cancel" Text="Cancel" />
						</td>
					</tr>
				</EditItemTemplate>
			</asp:ListView>

		</div>
	</div>



</div>

</form>
</body>
</html>