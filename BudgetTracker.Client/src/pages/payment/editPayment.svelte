<svelte:head>
    <title>BudgetTracker - Редактировать столбец</title>
</svelte:head>

<script>
    import {PaymentController, PaymentViewModelController, DebtModelController, MoneyColumnMetadataModelController, SpentCategoryModelController} from '../../generated-types';
    import {compare} from '../../services/Shared'
    import {navigateTo} from 'svero';
    import moment from 'moment';

    export let router = {};

    let id = "";
    let amount = 0;
    let ccy = "";
    let what = "";
    let categoryId = "";
    let columnId = "";
    let debtId = "";
    let kind = "";

    let when = "";
    let statement = "";
    let sms = "";

    let debts = [];
    let columns = [];
    let spents = [];

    async function load() {
        let responses = await Promise.all([PaymentViewModelController.find(router.params.id), DebtModelController.list(), MoneyColumnMetadataModelController.list(), SpentCategoryModelController.list()]);

        let payment = responses[0];

        id = payment.id;
        amount = payment.amount;
        ccy = payment.ccy;
        what = payment.what;
        categoryId = payment.categoryId;
        columnId = payment.columnId;
        debtId = payment.debtId;
        kind = payment.kindId.toString();

        when = payment.when;
        statement = payment.statementReference;
        sms = payment.sms;

        let distinct = function(list, accessor) {
            let result = list.reduce((a,b)=>{
                if (!a.find(t=>accessor(t) === accessor(b))) {
                    a.push(b);
                }
                return a;
            }, []);

            result = result.sort((a,b)=>compare(accessor(a),accessor(b)));

            return result;
        }

        debts = distinct(responses[1], s=>s.description);
        columns = distinct(responses[2], s=>s.provider + s.accountName);
        spents = distinct(responses[3], s=>s.category);
    }

    async function update() {
        await PaymentController.editPayment(id, amount, ccy, what, categoryId, columnId, debtId, kind);
        navigateTo("/Payment");
    }

    load();
</script>

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
                                <div class="form-horizontal">
                                    <div class="form-group">
                                        <label class="control-label">Когда: <span class="font-weight-bold font-italic">{moment(when).format("DD.MM.YY H:mm:ss")}</span></label>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Категория:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select class="form-control" bind:value="{categoryId}">
                                                <option value=""></option>
                                                {#each spents as spent}
                                                    <option value="{spent.id}">{spent.category}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Долг, к которому относится платеж:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select class="form-control" bind:value="{debtId}">
                                                <option value=""></option>
                                                {#each debts as debt}
                                                    <option value="{debt.id}">{debt.description}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Провайдер/аккаунт</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select class="form-control" bind:value="{columnId}">
                                                <option value=""></option>
                                                {#each columns as column}
                                                    <option value="{column.id}">{column.provider} / {column.accountName}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Reference: 
                                            <span class="font-weight-bold font-italic">{statement || "<не определен>"}</span></label>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">SMS:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <textarea rows="2" class="form-control" disabled="disabled">{sms || "<не определена>"}</textarea>
                                        </div>
                                    </div>

                                    <div class="form-group">
                                        <label class="control-label">Тип</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select class="form-control" bind:value="{kind}">
                                                <option value="0">Трата</option>
                                                <option value="1">Доход</option>
                                                <option value="2">Перевод</option>
                                                <option value="-1">Неизвестно</option>
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Количество</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" bind:value="{amount}" class="form-control" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Валюта</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select class="form-control" bind:value="{ccy}">
                                                <option value="RUB">₽</option>
                                                <option value="USD">$</option>
                                                <option value="EUR">€</option>
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">На что потрачено</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" bind:value="{what}" class="form-control" />
                                        </div>
                                    </div>
                                    <div class="form-group text-center">
                                        <button class="btn btn-default" on:click="{() => update()}">Обновить</button>
                                    </div>
                                </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>