<svelte:head>
    <title>BudgetTracker - Долги и займы</title>
</svelte:head>

<script lang="ts">
    import Link from '../../svero/Link.svelte';
    import {DebtController, DebtJsViewModel} from '../../generated-types'
    import {formatMoney} from '../../services/Shared'

    let debts: DebtJsViewModel[] = [];

    DebtController.indexJson().then(s=> debts = s);

    //used in views:
    Link; debts; formatMoney;
</script>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">
                        Долги и займы
                    </h3>
                    <div class="card-options">
                        &nbsp;
                        <Link class="btn btn-primary btn-sm" href="/Debt/Edit">
                            Добавить
                        </Link>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table card-table table-sm table-hover table-striped">
                        <thead>
                        <tr>
                            <th>Когда выдан</th>
                            <th>Срок (дней)</th>
                            <th>Процентная ставка (годовых)</th>
                            <th>Объём</th>
                            <th>К возврату</th>
                            <th>Выплачено</th>
                            <th>Осталось выплатить</th>
                            <th>Описание</th>
                            <th></th>
                        </tr>
                        </thead>
                        <tbody>
                        {#each debts as debt}
                            <tr>
                                <td>{debt.when}</td>
                                <td>{debt.daysCount} дней</td>
                                <td>{debt.percentage}%</td>
                                <td>{formatMoney(debt.amount)} {debt.ccy}</td>
                                <td>{formatMoney(debt.amount * (1 + debt.percentage / 100))} {debt.ccy}</td>
                                <td title="Последний платеж - {debt.lastPaymentDate}">
                                    {formatMoney(debt.returned)} {debt.ccy}
                                </td>
                                <td>{formatMoney((debt.amount * (1 + debt.percentage / 100)) - debt.returned)} {debt.ccy}</td>
                                <td>{debt.description}</td>
                                <td>
                                    <Link href="/Debt/Edit/{debt.id}">
                                        <span class="fe fe-edit-2"></span>
                                    </Link>
                                </td>
                            </tr>
                        {/each}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>
