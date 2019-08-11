<svelte:head>
    <title>BudgetTracker - Разделить платеж</title>
</svelte:head>

<script>
    import {PaymentController, PaymentViewModelController, DebtModelController, MoneyColumnMetadataModelController, SpentCategoryModelController} from '../../generated-types';
    import {compare} from '../../services/Shared'
    import { navigateTo } from '../../svero/utils';
    import moment from 'moment';

    export let router = {};

    let payment = {};

    let amount = 0;

    async function load() {
        payment = await PaymentViewModelController.find(router.params.id);
    }

    async function split() {
        await PaymentController.splitPayment(payment.id, amount);
        navigateTo("/Payment");
    }

    load();
</script>

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
                            <div class="form-horizontal">
                                <div class="form-group">
                                    <label class="control-label">Когда: <span class="font-weight-bold font-italic">{moment(payment.when).format("DD.MM.YY H:mm:ss")}</span></label>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Категория:</label>
                                    <span class="font-weight-bold font-italic">{payment.category || payment.debt || '<не определен>'}</span>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Провайдер/аккаунт</label>
                                    <span class="font-weight-bold font-italic">{payment.provider} / {payment.account}</span>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Reference: <span class="font-weight-bold font-italic">{payment.statementReference || "<не определен>"}</span></label>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">SMS:</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <textarea rows="2" class="form-control" disabled="disabled">{payment.sms || "<не определена>"}</textarea>
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="control-label">Тип</label>
                                    <span class="font-weight-bold font-italic">{payment.kind}</span>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Количество</label>
                                    <span class="font-weight-bold font-italic">{payment.amount}</span>
                                    <span class="font-weight-bold font-italic">{payment.ccy}</span>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Вынести в отдельный платеж</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <input type="text" class="form-control" bind:value="{amount}" />
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">На что потрачено</label>
                                        <span class="font-weight-bold font-italic">{payment.what}</span>
                                    </div>
                                    <div class="form-group text-center">
                                        <button class="btn btn-default" on:click="{() => split()}">Разделить</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>