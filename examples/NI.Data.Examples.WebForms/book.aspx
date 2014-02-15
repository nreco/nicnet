<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="NI.Data" %>

<script runat="server" language="c#">


protected override void OnLoad(EventArgs e) {
	base.OnLoad(e);

}

protected void bookDS_Init(object sender, EventArgs e) {
	bookDS.Dalc = NI.Data.Examples.WebForms.App_Code.DataHelper.GetDalc();
	bookDS.Condition = new QueryConditionNode((QField)"id", Conditions.Equal, new QConst(Request["id"]) );	
}

protected void bookFormView_DataBound(object sender, EventArgs e) {
	if (bookFormView. DataItemCount == 0) {
		bookFormView.ChangeMode(FormViewMode.Insert);
	}
}

protected void bookFormView_ItemInserted(object sender, FormViewInsertedEventArgs e) {
	Response.Redirect(String.Format("book.aspx?id={0}", e.Values["id"]), true);
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

			<div class="alert alert-info">This example demostrates how to use FormView with DalcDataSource
				<br />
				Note: For the sake of example simplicity DataSetDalc implementation persists all data in Session.
			</div>
		</div>
    </div>
	<hr />

	<Dalc:DalcDataSource runat="server" id="bookDS" 
		OnInit="bookDS_Init"
		TableName="books" 
		DataSetMode="true" 
		DataKeyNames="id" 
		AutoIncrementNames="id"/>

	<div class="row">
		<div class="col-md-12">

			<asp:FormView runat="server" ID="bookFormView" DataSourceID="bookDS" DefaultMode="ReadOnly"
				DataKeyNames="id" style="width:100%;" 
				OnDataBound="bookFormView_DataBound"
				OnItemInserted="bookFormView_ItemInserted">
				<ItemTemplate>
					<h2>FormView <small>Book Details</small></h2>
					<hr />
					<div role="form" class="form-horizontal">
						<div class="form-group">
							<label class="col-sm-2 control-label">Title</label>
							<div class="col-sm-10">
								<%# HttpUtility.HtmlEncode( Convert.ToString(Eval("title")) ) %>
							</div>
							
						</div>

						<div class="form-group">
							<label class="col-sm-2 control-label">Description</label>
							<div class="col-sm-10">
								<%# HttpUtility.HtmlEncode( Convert.ToString(Eval("description"))) %>
							</div>
						</div>

						<div class="form-group">
							<label class="col-sm-2 control-label">Rating</label>
							<div class="col-sm-10">
								<%# Eval("rating") %>
							</div>
						</div>

						<div class="form-group">
							<div class="col-sm-offset-2 col-sm-10">
								<asp:LinkButton CssClass="btn btn-default" runat="server" ID="Edit" CommandName="Edit" CausesValidation="false" Text="Edit" />
								<a class="btn btn-default" href="default.aspx">Back to list</a>
							</div>
						</div>
					</div>
				</ItemTemplate>
				<EditItemTemplate>
					<h2>FormView <small>Edit Book</small></h2>
					<hr />
					<div role="form" class="form-horizontal">
						<div class="form-group">
							<label class="col-sm-2 control-label">Title</label>
							<div class="col-sm-10">
								<asp:TextBox runat="server" ID="title" Text='<%# Bind("title") %>' />
							</div>
						</div>

						<div class="form-group">
							<label class="col-sm-2 control-label">Description</label>
							<div class="col-sm-10">
								<asp:TextBox id="description" TextMode="MultiLine" Columns="60" Rows="3" runat="server" Text='<%# Bind("description") %>' />
							</div>
						</div>

						<div class="form-group">
							<label class="col-sm-2 control-label">Rating</label>
							<div class="col-sm-10">
								<asp:DropDownList ID="rating" runat="server" SelectedValue='<%# Bind("rating") %>'>
									<asp:ListItem Value="1" Text="1"/>
									<asp:ListItem Value="2" Text="2"/>
									<asp:ListItem Value="3" Text="3"/>
									<asp:ListItem Value="4" Text="4"/>
									<asp:ListItem Value="5" Text="5"/>
								</asp:DropDownList>
							</div>
						</div>

						<div class="form-group">
							<div class="col-sm-offset-2 col-sm-10">
								<asp:LinkButton CssClass="btn btn-primary" runat="server" ID="Update" CommandName="Update" CausesValidation="true" Text="Save" />
								<asp:LinkButton CssClass="btn btn-default" runat="server" ID="Cancel" CommandName="Cancel" CausesValidation="false" Text="Cancel" />
							</div>
						</div>
					</div>
				</EditItemTemplate>

				<InsertItemTemplate>
					<h2>FormView <small>Add Book</small></h2>
					<hr />
					<div role="form" class="form-horizontal">
						<div class="form-group">
							<label class="col-sm-2 control-label">Title</label>
							<div class="col-sm-10">
								<asp:TextBox runat="server" ID="title" Text='<%# Bind("title") %>' />
							</div>
						</div>

						<div class="form-group">
							<label class="col-sm-2 control-label">Description</label>
							<div class="col-sm-10">
								<asp:TextBox id="description" TextMode="MultiLine" Columns="60" Rows="3" runat="server" Text='<%# Bind("description") %>' />
							</div>
						</div>

						<div class="form-group">
							<label class="col-sm-2 control-label">Rating</label>
							<div class="col-sm-10">
								<asp:DropDownList ID="rating" runat="server" SelectedValue='<%# Bind("rating") %>'>
									<asp:ListItem Value="1" Text="1"/>
									<asp:ListItem Value="2" Text="2"/>
									<asp:ListItem Value="3" Text="3"/>
									<asp:ListItem Value="4" Text="4"/>
									<asp:ListItem Value="5" Text="5"/>
								</asp:DropDownList>
							</div>
						</div>

						<div class="form-group">
							<div class="col-sm-offset-2 col-sm-10">
								<asp:LinkButton CssClass="btn btn-primary" runat="server" ID="Insert" CommandName="Insert" CausesValidation="true" Text="Add" />
							</div>
						</div>
					</div>
				</InsertItemTemplate>

			</asp:FormView>
		</div>
	</div>


</div>

</form>
</body>
</html>