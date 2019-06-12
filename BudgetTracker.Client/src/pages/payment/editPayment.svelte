editPayment

<!--@using Controllers.ViewModels.Payment
@using Microsoft.EntityFrameworkCore.Internal
@using Model
@inject ObjectRepository Repository
@model Controllers.ViewModels.Payment.PaymentViewModel
@{
    ViewData["Title"] = "Редактировать столбец";
    var categoriesSelectList = Repository.Set<SpentCategoryModel>().ToList().Select(v => new SelectListItem()
    {
        Text = v.Category,
        Value = v.Id.ToString(),
        Selected = v == Model.Items.First().Category
    }).Distinct((a, b) => a.Text == b.Text).OrderBy(v => v.Text).ToList();

    categoriesSelectList.Insert(0, new SelectListItem());
    
    var debtsSelectList = Repository.Set<DebtModel>().ToList().Select(v => new SelectListItem()
    {
        Text = v.Description,
        Value = v.Id.ToString(),
        Selected = v == Model.Items.First().Debt
    }).Distinct((a, b) => a.Text == b.Text).OrderBy(v => v.Text).ToList();

    debtsSelectList.Insert(0, new SelectListItem());
    
    var providerAccountsSelectList = Repository.Set<MoneyColumnMetadataModel>().Where(v => !v.IsComputed).Select(v => new SelectListItem()
    {
        Text = v.Provider + "/" + v.AccountName,
        Value = v.Id.ToString(),
        Selected = v.Id == Model.ColumnId
    }).OrderBy(v => v.Text).ToList();

    providerAccountsSelectList.Insert(0, new SelectListItem());
}


<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-6">
            <div class="card">
                <div class="card-header">
                    Редактировать оплату
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                            @using (Html.BeginForm("EditPayment", "Payment", FormMethod.Post))
                            {
                                @Html.HiddenFor(v => v.Id)
                                <div class="form-horizontal">
                                    <div class="form-group">
                                        <label class="control-label">Когда: <span class="font-weight-bold font-italic">@Model.When.ToString("g")</span></label>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Категория:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.DropDownListFor(model => model.CategoryId, categoriesSelectList, new {@class = "form-control"})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Долг, к которому относится платеж:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.DropDownListFor(model => model.DebtId, debtsSelectList, new {@class = "form-control"})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Провайдер/аккаунт</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.DropDownListFor(model => model.ColumnId, providerAccountsSelectList, new {@class = "form-control"})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Reference: <span class="font-weight-bold font-italic">@(Model.Items.First().StatementReference ?? "<не определен>")</span></label>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">SMS:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <textarea rows="2" class="form-control" disabled="disabled">@(Model.Items.First().Sms?.Message.Replace("\n","") ?? "<не определена>")</textarea>
                                        </div>
                                    </div>

                                    <div class="form-group">
                                        <label class="control-label">Тип</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.DropDownListFor(model => model.Kind, typeof(Model.PaymentKind).GetSelectList(Model.Kind), new {@class = "form-control"})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Количество</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.EditorFor(model => model.Amount, new {htmlAttributes = new {@class = "form-control"}})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Валюта</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.EditorFor(model => model.Ccy, new {htmlAttributes = new {@class = "form-control"}})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">На что потрачено</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.EditorFor(model => model.Items.First().What, new {htmlAttributes = new {@class = "form-control"}})
                                        </div>
                                    </div>
                                    <div class="form-group text-center">
                                        <input type="submit" value="Обновить" class="btn btn-default"/>
                                    </div>
                                </div>
                            }
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts{
    <link rel="stylesheet" href="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/themes/smoothness/jquery-ui.css">
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#Function").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "ComputedAutocomplete",
                        contentType: 'application/json',
                        data: JSON.stringify(request),
                        type: "POST",
                        processData: false,
                        success: function (data) {
                            response(data);
                        },
                        error: function (XMLHttpRequest, textStatus) {
                            alert(textStatus);
                        }
                    });
                },
                minLength: 1
            });
        });
    </script>
}
-->