<svelte:head>
    <title>BudgetTracker - Разделить платеж</title>
</svelte:head>

<script lang="ts">
    import { createEventDispatcher } from 'svelte';
    import * as svelte from 'svelte/store';
    import paymentProtos from '../../generated/Payments_pb';
    import * as protosCategories from '../../generated/SpentCategories_pb';
    import * as protosDebts from '../../generated/Debts_pb';
    import * as protosAccounts from '../../generated/Accounts_pb';
    import moment from 'moment';
    import SoWService from '../../services/SoWService';
    import { formatPaymentKind } from '../../services/Shared';

    export let model = new paymentProtos.Payment().toObject();
    export let debts: svelte.Writable<protosDebts.DebtView.AsObject[]>;
    export let spentCategories: svelte.Writable<protosCategories.SpentCategory.AsObject[]>;
    export let moneyColumns: svelte.Writable<protosAccounts.MoneyColumnMetadata.AsObject[]>;

    const dispatch = createEventDispatcher();

    let currentCategory = svelte.get(spentCategories).find(t=>t.id?.value == model.categoryId?.value)?.category ??
                          svelte.get(debts).find(t=>t.model?.id?.value == model.debtId?.value)?.model?.description ?? '<не определен>';

    let column = $moneyColumns.find(t=>t.id?.value == model?.columnId?.value);

    let amount = 0;

    async function split() {
        await SoWService.splitPayment(model, amount);
        dispatch('close');
    }
</script>
<div class="form-horizontal">
    <div class="form-group">
        <label class="control-label">Когда: <span class="font-weight-bold font-italic">{moment.unix(model.when.seconds + model.when.nanos / 10e9).format("DD.MM.YY H:mm:ss")}</span></label>
    </div>
    <div class="form-group">
        <label class="control-label">Категория:</label>
        <span class="font-weight-bold font-italic">{currentCategory}</span>
    </div>
    <div class="form-group">
        <label class="control-label">Провайдер/аккаунт</label>
        <span class="font-weight-bold font-italic">{column.provider} / {column.accountName}</span>
    </div>
    <div class="form-group">
        <label class="control-label">Reference: <span class="font-weight-bold font-italic">{model.statement || "<не определен>"}</span></label>
    </div>
    <div class="form-group">
        <label class="control-label">SMS:</label>
        <div control-labelstyle="padding-top: 7px;">
            <textarea rows="2" class="form-control" disabled="disabled">{model.sms || "<не определена>"}</textarea>
        </div>
    </div>

    <div class="form-group">
        <label class="control-label">Тип</label>
        <span class="font-weight-bold font-italic">{formatPaymentKind(model.kind)}</span>
    </div>
    <div class="form-group">
        <label class="control-label">Количество</label>
        <span class="font-weight-bold font-italic">{model.amount}</span>
        <span class="font-weight-bold font-italic">{model.ccy}</span>
    </div>
    <div class="form-group">
        <label class="control-label">Вынести в отдельный платеж</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" class="form-control" bind:value="{amount}" />
        </div>
        <div class="form-group">
            <label class="control-label">На что потрачено</label>
            <span class="font-weight-bold font-italic">{model.what}</span>
        </div>
        <div class="form-group text-center">
            <button class="btn btn-default" on:click="{() => split()}">Разделить</button>
        </div>
    </div>
</div>