<svelte:head>
    <title>BudgetTracker - Движение денежных средств</title>
</svelte:head>

<script lang="ts">
    import Link from '../../svero/Link.svelte';
    import SoWService from '../../services/SoWService';
    import PaymentRow from './paymentRow.svelte';
    // import Modal from '../../components/Modal.svelte';
    import * as protosCommons from '../../generated/Commons_pb';
    import * as protosCategories from '../../generated/SpentCategories_pb';
    import * as protosDebts from '../../generated/Debts_pb';
    import * as protosPayments from '../../generated/Payments_pb';
    import { get } from 'svelte/store';
    import { onDestroy } from 'svelte';

    let debts = SoWService.getDebts(onDestroy).debts;
    let spentCategories = SoWService.getSpentCategories(onDestroy).spentCategories;
    let payments = SoWService.getPayments(onDestroy).payments;
    let moneyColumns = SoWService.getMoneyColumnMetadatas(onDestroy).moneyColumnMetadatas;

    class CategoryDropDown {
        constructor(name: string, id: protosCommons.UUID.AsObject, isDebt: boolean) {
            this.name = name;
            this.id = id;
            this.isDebt = isDebt;
        }

        name: string;
        id: protosCommons.UUID.AsObject;
        isDebt: boolean;
    }
    let categories: CategoryDropDown[] = [];
    function updateCategories() {
        categories = get(spentCategories).reduce((acc: CategoryDropDown[], category: protosCategories.SpentCategory.AsObject) => {
            if (!acc.find(t=>t.name == category.category)) {
                acc = [...acc, new CategoryDropDown(category.category, category.id!, false) ];
            }
            return acc;
        }, []).concat(get(debts).reduce((acc: CategoryDropDown[], debt: protosDebts.DebtView.AsObject) => {
            if (!acc.find(t=>t.name == debt.model!.description)) {
                acc = [...acc, new CategoryDropDown(debt.model!.description, debt.model!.id!, true) ];
            }
            return acc;
        }, []));
    }

    onDestroy(spentCategories.subscribe(updateCategories));
    onDestroy(debts.subscribe(updateCategories));

    let sorting: number = 0;
    let showCategorized = true;

    async function switchCategorized() {
        await SoWService.showCategorized(!showCategorized);
    }

    async function expandCollapse(ids: protosCommons.UUID.AsObject[]) {
        await SoWService.expandCollapse(ids);
    }

    async function deletePayment(payment: protosPayments.Payment.AsObject) {
        await SoWService.deletePayment(payment.id!);
    }

    async function editPayment(payment: protosPayments.Payment.AsObject) {
        console.trace(payment);
        // TODO open window and edit
    } 

    async function splitPayment(payment: protosPayments.Payment.AsObject) {
        console.trace(payment);
        // TODO open window and split
    }

    function dragStart(ev: DragEvent, payment: protosPayments.Payment.AsObject) {
        if (ev.dataTransfer) {
            ev.dataTransfer.setData("payment", JSON.stringify(payment));
        }
    }
    
    function dragover(ev: DragEvent) {
        ev.preventDefault();
        if (ev.dataTransfer) {
            ev.dataTransfer.dropEffect = 'move';
        }
	}

    async function drop(ev: DragEvent, newCategory: CategoryDropDown) {
        ev.preventDefault();
        if (ev.dataTransfer) {
            var payment: protosPayments.Payment.AsObject = JSON.parse(ev.dataTransfer.getData("payment"));
            if (newCategory.isDebt) {
                payment.categoryId = undefined;
                payment.debtId = newCategory.id;
            } else {
                payment.categoryId = newCategory.id;
                payment.debtId = undefined;
            }
            SoWService.editPayment(payment);
        }
    }
   
    PaymentRow; Link; showCategorized; switchCategorized; deletePayment; sorting; expandCollapse; spentCategories; categories; payments; dragStart; dragover; drop; editPayment; splitPayment; moneyColumns; // used in view
</script>

<style>
        td button {
        line-height:inherit;
        font-weight:inherit;
        font-size:inherit;
        text-transform:inherit;
    }

    .bold { font-weight: bolder }

    .italic { font-style: italic }

    .italic td {
        padding-left: 0.4rem;
    }

    .italic td:first-child {
        padding-left: 2rem;
    }

    .italic td:last-child {
        padding-left: 0.9rem;
    }
</style>

<div></div>
<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">
                        Движение денежных средств 
                     </h3>
                    <div class="card-options">
                        &nbsp;
                        <Link class="btn btn-primary btn-sm" href="/Payment/Category">
                            Категории расходов
                        </Link>
                        <button class="btn btn-secondary btn-sm ml-2" on:click="{() => switchCategorized()}">
                            {showCategorized ? "Скрыть с категориями" : "Показать все"}
                        </button>
                    </div>
                </div>
                <div class="card-header">
                    {#each categories as category}
                        <span on:drop={event => drop(event, category)} on:dragover={dragover} class="btn btn-sm {category.isDebt ? "btn-warning" : "btn-success"} p-1 m-1" style="cursor: no-drop">
                            {category.name}
                        </span>
                    {/each}
                </div>
                <div class="table-responsive">
                    <table class="table card-table table-sm table-hover table-striped">
                        <thead>
                        <tr>
                            <th>
                                Когда
                                <span class="fe fe-chevron-down"></span>
                            </th>
                            <th>Тип</th>
                            <th>Категория</th>
                            <th>Провайдер</th>
                            <th>Счёт</th>
                            <th>Сумма</th>
                            <th>Сообщение</th>
                            <th>Удалить</th>
                        </tr>
                        </thead>
                        <tbody>
                        {#each $payments as payment, idx}
                            <PaymentRow payment={{summary:payment}} {debts} {moneyColumns} {spentCategories} {expandCollapse} {dragStart} {editPayment} {deletePayment} {splitPayment} parentId="{[]}" />
                        {/each}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>