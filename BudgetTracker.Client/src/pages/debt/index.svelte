<svelte:head>
    <title>BudgetTracker - Долги и займы</title>
</svelte:head>

<script lang="ts">
    import Form from './add.svelte';
    import Link from '../../svero/Link.svelte';
    import Modal from '../../components/Modal.svelte';
    import SoWService from '../../services/SoWService';
    import * as commons from '../../generated/Commons_pb';
    import * as protos from '../../generated/Debts_pb';
    import {formatMoney} from '../../services/Shared'
    import { writable, get } from 'svelte/store';
    import { onDestroy } from 'svelte';
    import { formatUnixDate } from '../../services/Shared';

    let debts = writable<protos.DebtView.AsObject[]>([]);

    let showCreate = false;
    let showEdit = false;

    let editDebtModel: protos.Debt.AsObject | undefined = undefined;
    
    function parseDebts(stream: protos.DebtsStream.AsObject) {
        if (stream.added) {
            let newDebts = get(debts);
            newDebts = [...newDebts, stream.added];
            debts.set(newDebts);
        } else if (stream.removed) {
            let newDebts = get(debts);
            newDebts = newDebts.filter((f: protos.DebtView.AsObject) => f.model!.id!.value != stream.removed!.model!.id!.value);
            debts.set(newDebts);
        } else if (stream.updated) {
            let newDebts = get(debts);
            newDebts = newDebts.map((f: protos.DebtView.AsObject) => {
                if (f.model!.id!.value == stream.updated!.model!.id!.value) {
                    return stream!.updated;
                }
                return f;
            });
            debts.set(newDebts);
        } else if (stream.snapshot) {
            let newStores = stream.snapshot.debtsList;
            debts.set(newStores);
        } else {
            console.error("Unsupported operation");
        }
    }

    function formatTimestamp(timestamp: commons.Timestamp.AsObject) {
         return formatUnixDate(timestamp.seconds + timestamp.nanos / 10e9);
    }

    function createDebt() {
        showCreate = true;
    }

    function editDebt(debt: protos.DebtView.AsObject) {
        editDebtModel = debt.model;
        showEdit = true;
    }

    async function deleteDebt(debt: protos.DebtView.AsObject) {
        await SoWService.deleteDebt(debt.model!.id!);
    }

    let unsubscribe = SoWService.getDebts(parseDebts);

    onDestroy(unsubscribe);

    //used in views:
    Link; Modal; Form; showCreate; showEdit; debts; editDebtModel; formatMoney; formatTimestamp; editDebt; deleteDebt; createDebt;
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
                        <button class="btn btn-primary btn-sm" on:click="{() => createDebt()}">
                            Добавить
                        </button>
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
                        {#each $debts as debt}
                            <tr>
                                <td>{formatTimestamp(debt.model.issued)}</td>
                                <td>{debt.model.daysCount} дней</td>
                                <td>{debt.model.percentage}%</td>
                                <td>{formatMoney(debt.model.amount)} {debt.model.ccy}</td>
                                <td>{formatMoney(debt.model.amount * (1 + debt.model.percentage / 100))} {debt.model.ccy}</td>
                                <td title="Последний платеж - {debt.lastPaymentDate || "неизвестно"}">
                                    {formatMoney(debt.returned)} {debt.model.ccy}
                                </td>
                                <td>{formatMoney((debt.model.amount * (1 + debt.model.percentage / 100)) - debt.returned)} {debt.model.ccy}</td>
                                <td>{debt.model.description}</td>
                                <td>
                                    <button class="btn btn-link btn-anchor" on:click="{() => editDebt(debt)}">
                                        <span class="fe fe-edit-2"></span>
                                    </button>
                                    <button class="btn btn-link btn-anchor" on:click="{() => deleteDebt(debt)}">
                                        <span class="fe fe-x-circle"></span>
                                    </button>
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

{#if showCreate}
<Modal bind:show="{showCreate}">
    <div slot="title">
        Добавить долг
    </div>
    <Form action="Добавить" on:close="{() => showCreate = false}" />
</Modal>
{/if}

{#if showEdit}
<Modal bind:show="{showEdit}">
    <div slot="title">
        Редактировать долг
    </div>
    <Form action="Редактировать" model="{editDebtModel}" on:close="{() => showEdit = false}" />
</Modal>
{/if}