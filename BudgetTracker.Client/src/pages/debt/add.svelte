<script lang="ts">
    import moment from 'moment';
	import { createEventDispatcher } from 'svelte';
    import SoWService from '../../services/SoWService';
	import { onMount } from 'svelte';

    import debtProtos from '../../generated/Debts_pb';
    import commonProtos from '../../generated/Commons_pb';

    export let model = new debtProtos.Debt().toObject();

    export let action = "Добавить";

	const dispatch = createEventDispatcher();

    function dateToString(from: commonProtos.Timestamp.AsObject): string {
        return moment.unix(from.seconds + from.nanos / 10e9).format("DD.MM.YYYY");
    }

    function stringToDate(from: string): commonProtos.Timestamp.AsObject {
        const totalSeconds = moment(from, "DD.MM.YYYY").unix();
        let result: commonProtos.Timestamp.AsObject = {
            seconds: Math.floor(totalSeconds),
            nanos: (totalSeconds - Math.floor(totalSeconds))*10e9
        }
        return result;
    }

    let when = "";

    onMount(() => { if (model.issued) when = dateToString(model.issued) });

    let submit = async function() {
        model.issued = stringToDate(when);
        SoWService.updateDebt(model);
        dispatch('close');
    }
    
    action; submit; model;
</script>

<div class="form-horizontal">
    <div class="form-group">
        <label class="control-label">Когда</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{when}" class="form-control" placeholder="ДД.ММ.ГГГГ" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Отдано</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{model.amount}" class="form-control" placeholder="1000" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Валюта</label>
        <div control-labelstyle="padding-top: 7px;">
            <select class="form-control" bind:value="{model.ccy}">
                <option value="RUB">₽</option>
                <option value="USD">$</option>
                <option value="EUR">€</option>
            </select>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Процентная ставка (годовых)</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{model.percentage}" class="form-control" placeholder="5.5" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Срок (дней)</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{model.daysCount}" class="form-control" placeholder="182" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Описание</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{model.description}" class="form-control" placeholder="Заемщик / номер договора" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Строка для определения платежей</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" bind:value="{model.regexForTransfer}" class="form-control" placeholder="Например, номер договора. То, что позволит отнести перевод денег к этому долгу." />
        </div>
    </div>
    <div class="form-group text-center">
        <button on:click="{() => submit()}" class="btn btn-primary">{action}</button>
    </div>
</div>