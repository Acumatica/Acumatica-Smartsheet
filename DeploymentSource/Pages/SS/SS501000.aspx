<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SS501000.aspx.cs" Inherits="Page_SS501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Projects" TypeName="SmartSheetIntegration.SmartsheetSyncProcess" >
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" TabIndex="100" TemporaryFilterCaption="Filter Applied">
		<Levels>
			<px:PXGridLevel DataKeyNames="ContractCD" DataMember="Projects">
			    <RowTemplate>
                    <px:PXCheckBox ID="edSelected" runat="server" AlreadyLocalized="False" DataField="Selected" Text="Selected">
                    </px:PXCheckBox>
                    <px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" AllowEdit="True">
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="edDescription" runat="server" AlreadyLocalized="False" DataField="Description" Width="250px">
                    </px:PXTextEdit>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True">
                    </px:PXSegmentMask>
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Width="100px">
                    </px:PXDropDown>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ContractCD" Width="150px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Description" Width="250px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Status" Width="100px">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
