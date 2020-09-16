<svelte:head>
    <title>BudgetTracker - Редактировать столбец</title>
</svelte:head>

<script lang="ts">
    import { createEventDispatcher, onMount } from 'svelte';
    import * as svelte from 'svelte/store';
    import paymentProtos from '../../generated/Payments_pb';
    import * as protosCategories from '../../generated/SpentCategories_pb';
    import * as protosDebts from '../../generated/Debts_pb';
    import * as protosAccounts from '../../generated/Accounts_pb';
    import moment from 'moment';
    import SoWService from '../../services/SoWService';
	import {compare} from '../../services/Shared';

    export let model = new paymentProtos.Payment().toObject();

    export let debts: svelte.Writable<protosDebts.DebtView.AsObject[]>;
    export let spentCategories: svelte.Writable<protosCategories.SpentCategory.AsObject[]>;
    export let moneyColumns: svelte.Writable<protosAccounts.MoneyColumnMetadata.AsObject[]>;

    let debtsList: protosDebts.Debt.AsObject[] = [];
    let moneyColumnsList: protosAccounts.MoneyColumnMetadata.AsObject[] = [];

    let currentCategory = svelte.get(spentCategories).find(t=>t.id?.value == model.categoryId?.value)?.category ?? "";
    let currentDebt = model.debtId?.value;
    let currentColumn = model.columnId?.value;
    let currentKind = model.kind.valueOf().toString();
    let spentCategoriesList : string[];

    $: model.categoryId = $spentCategories.find(t=>t.category == currentCategory)?.id;
    $: model.debtId = currentDebt != undefined ? { value: currentDebt } : undefined;
    $: model.columnId = currentColumn != undefined ? { value: currentColumn } : undefined;
    $: model.kind = Number.parseInt(currentKind);

    $: spentCategoriesList = $spentCategories.map(t=>t.category).filter((v, i, a) => a.indexOf(v) === i);

    const dispatch = createEventDispatcher();
    
    $: {
        debtsList = $debts.map(t=>t.model!!);
        moneyColumnsList = $moneyColumns.filter(t=>t.userFriendlyName.length > 0).sort((a,b)=>compare(a.provider + a.accountName, b.provider + b.accountName));
    } 
    let submit = async function() {
        SoWService.editPayment(model);
        dispatch('close');
    }
</script>

<div class="form-horizontal">
    <div class="form-group">
        <label class="control-label">Когда: <span class="font-weight-bold font-italic">{moment.unix(model.when.seconds + model.when.nanos / 10e9).format("DD.MM.YY H:mm:ss")}</span></label>
    </div>
    <div class="form-group">
        <label class="control-label">Категория:</label>
        <div control-labelstyle="padding-top: 7px;">
            <select class="form-control" bind:value="{currentCategory}">
                <option value=""></option>
                {#each spentCategoriesList as spent}
                    <option value="{spent}">{spent}</option>
                {/each}
            </select>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Долг, к которому относится платеж:</label>
        <div control-labelstyle="padding-top: 7px;">
            <select class="form-control" bind:value="{currentDebt}">
                <option value=""></option>
                {#each debtsList as debt}
                    <option value="{debt.id.value}">{debt.description}</option>
                {/each}
            </select>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Провайдер/аккаунт</label>
        <div control-labelstyle="padding-top: 7px;">
            <select class="form-control" bind:value="{currentColumn}">
                <option value=""></option>
                {#each moneyColumnsList as column}
                    <option value="{column.id.value}">{column.provider} / {column.accountName}</option>
                {/each}
            </select>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Reference: 
            <span class="font-weight-bold font-italic">{model.statement || "<не определен>"}</span></label>
    </div>
    <div class="form-group">
        <label class="control-label">SMS:</label>
        <div control-labelstyle="padding-top: 7px;">
            <textarea rows="2" class="form-control" disabled="disabled">{model.sms || "<не определена>"}</textarea>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Тип</label>
        <div control-labelstyle="padding-top: 7px;">
            <select class="form-control" bind:value="{currentKind}">
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
            <input type="text" bind:value="{model.amount}" class="form-control" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Валюта</label>
        <div control-labelstyle="padding-top: 7px;">
            <select class="form-control" bind:value="{model.ccy}">
                <option value="RUB">₽</option>
                <option value="USD">$</option>
                <option value="EUR">€</option>
                <option value="GBP">£</option>
            </select>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">На что потрачено</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{model.what}" class="form-control" />
        </div>
    </div>
    <div class="form-group text-center">
        <button class="btn btn-default" on:click="{() => submit()}">Обновить</button>
    </div>
</div>