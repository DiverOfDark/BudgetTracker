spentCategories

<!--
@using Model
@model IEnumerable<SpentCategoryModel>
@{
    ViewData["Title"] = "Категории расходов";

}
<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                    <div class="card-header">
                        <h3 class="card-title">
                            Категории расходов
                        </h3>
                    </div>
                    <div class="table-responsive">
                        <table class="table card-table table-sm table-hover table-striped">
                            <thead>
                            <tr>
                                <th>Категория</th>
                                <th>Тип</th>
                                <th>Шаблон</th>
                                <th>Удалить</th>
                            </tr>
                            </thead>
                            <tbody>
                            @foreach (var category in Model.OrderBy(v => v.Category).ThenBy(v => v.Pattern))
                            {
                                <tr>
                                    <td>@category.Category</td>
                                    <td>@category.Kind.GetDisplayName()</td>
                                    <td>@category.Pattern</td>
                                    <td>
                                        <a href="@Url.Action("EditCategory", new {id = category.Id})">
                                            <span class="fe fe-edit"></span>
                                        </a>
                                        <a href="@Url.Action("DeleteCategory", new {id = category.Id})">
                                            <span class="fe fe-x-circle"></span>
                                        </a>
                                    </td>
                                </tr>
                            }
        
                            <form action="@Url.Action("CreateCategory")">
                                <tr>
                                    <td>
                                        <input type="text" id="category" name="category" class="form-control form-control-sm"/>
                                    </td>
                                    <td>
                                        @Html.DropDownList("kind", typeof(PaymentKind).GetSelectList(PaymentKind.Unknown), new{ @class="form-control form-control-sm"})
                                    </td>
                                    <td>
                                        <input type="text" id="pattern" name="pattern" class="form-control form-control-sm"/>
                                    </td>
                                    <td>
                                        <input type="submit" class="btn btn-sm btn-azure" value="Добавить"/>
                                    </td>
                                </tr>
                            </form>
                            </tbody>
                        </table>
                    </div>
                </div>
        </div>
    </div>
</div>

-->