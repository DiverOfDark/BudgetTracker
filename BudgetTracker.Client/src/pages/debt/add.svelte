<svelte:head>
<title>BudgetTracker - {action} долг</title>
</svelte:head>

<script>
    //@ts-ignore
    import {navigateTo} from '/svero/main';
    import { DebtController } from '../../generated-types';
    import moment from 'moment';

    export let router = {};
    
    if (router.params && router.params.id) {
        DebtController.indexJson().then(s => {
            let actualDebt = s.find(s=>s.id == router.params.id);
            if (actualDebt) {
                id = actualDebt.id;
                when = actualDebt.when;
                amount = actualDebt.amount;
                ccy = actualDebt.ccy;
                percentage = actualDebt.percentage;
                daysCount = actualDebt.daysCount;
                description = actualDebt.description;
                regexForTransfer = actualDebt.regexForTransfer;

                action = "Редактировать";
            }
        });
    }

    let action = "Добавить"; // aka btnTitle

    let id = "";
    let when = "";
    let amount = 0;
    let ccy = "RUB";
    let percentage = 0;
    let daysCount = 365;
    let description = "";
    let regexForTransfer = "";

    let submit = async function() {
        await DebtController.editDebt(id, moment(when, "DD.MM.YYYY").toISOString(), amount, ccy, percentage, daysCount, description, regexForTransfer);
        navigateTo("/Debt");
    }

</script>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-6">
            <div class="card">
                <div class="card-header">
                    {action} долг
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
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
                                        <input type="text" bind:value="{amount}" class="form-control" placeholder="1000" />
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
                                    <label class="control-label">Процентная ставка (годовых)</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <input type="text" bind:value="{percentage}" class="form-control" placeholder="5.5" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Срок (дней)</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <input type="text" bind:value="{daysCount}" class="form-control" placeholder="182" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Описание</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <input type="text" bind:value="{description}" class="form-control" placeholder="Заемщик / номер договора" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Строка для определения платежей</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <input type="text" bind:value="{regexForTransfer}" class="form-control" placeholder="Например, номер договора. То, что позволит отнести перевод денег к этому долгу." />
                                    </div>
                                </div>
                                <div class="form-group text-center">
                                    <button on:click="{() => submit()}" class="btn btn-primary">{action}</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
