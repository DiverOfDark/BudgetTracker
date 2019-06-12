splitPayment

<!--

@using Controllers.ViewModels.Payment
@using Microsoft.EntityFrameworkCore.Internal
@using Model
@inject ObjectRepository Repository
@model Controllers.ViewModels.Payment.PaymentViewModel
@{
    ViewData["Title"] = "Разделить платеж";
}


<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-6">
            <div class="card">
                <div class="card-header">
                    Разделить платеж
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                            @using (Html.BeginForm("SplitPayment", "Payment", FormMethod.Post))
                            {
                                @Html.HiddenFor(v => v.Id)
                                <div class="form-horizontal">
                                    <div class="form-group">
                                        <label class="control-label">Когда: <span class="font-weight-bold font-italic">@Model.When.ToString("g")</span></label>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Категория:</label>
                                        <span class="font-weight-bold font-italic">@(Model.Category ?? Model.Debt)</span>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Провайдер/аккаунт</label>
                                        <span class="font-weight-bold font-italic">@Model.Provider / @Model.Account</span>
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
                                        <span class="font-weight-bold font-italic">@Model.Kind.GetDisplayName()</span>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Количество</label>
                                        <span class="font-weight-bold font-italic">@Model.Amount</span>
                                        <span class="font-weight-bold font-italic">@Model.Ccy</span>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Вынести в отдельный платеж</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            @Html.EditorFor(model => model.Amount, new {htmlAttributes = new {@class = "form-control"}})
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">На что потрачено</label>
                                        <span class="font-weight-bold font-italic">@Model.Items.First().What</span>
                                    </div>
                                    <div class="form-group text-center">
                                        <input type="submit" value="Разделить" class="btn btn-default"/>
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

-->