<svelte:head>
    <title>BudgetTracker - Движение денежных средств</title>
</svelte:head>

<script lang="ts">
    import Link from '../../svero/Link.svelte';
    import SoWService from '../../services/SoWService';
    // import Modal from '../../components/Modal.svelte';
    // import {compare} from '../../services/Shared';
    import * as protosCommons from '../../generated/Commons_pb';
    import * as protosCategories from '../../generated/SpentCategories_pb';
    import * as protosDebts from '../../generated/Debts_pb';
    import * as protosPayments from '../../generated/Payments_pb';
    import { writable, get } from 'svelte/store';
    import { onDestroy } from 'svelte';

    let spentCategories = writable<protosCategories.SpentCategory.AsObject[]>([]);
    let debts = writable<protosDebts.DebtView.AsObject[]>([]);
    let payments = writable<protosPayments.PaymentView.AsObject[]>([]);
  
    let sorting: number = 0;

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

    onDestroy(spentCategories.subscribe(updateCategories));
    onDestroy(debts.subscribe(updateCategories));

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

    let showCategorized = true;

    function parseDebts(stream: protosDebts.DebtsStream.AsObject) {
        if (stream.added) {
            let newDebts = get(debts);
            newDebts = [...newDebts, stream.added];
            debts.set(newDebts);
        } else if (stream.removed) {
            let newDebts = get(debts);
            newDebts = newDebts.filter((f: protosDebts.DebtView.AsObject) => f.model!.id!.value != stream.removed!.model!.id!.value);
            debts.set(newDebts);
        } else if (stream.updated) {
            let newDebts = get(debts);
            newDebts = newDebts.map((f: protosDebts.DebtView.AsObject) => {
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

    function parseSpentCategories(stream: protosCategories.SpentCategoriesStream.AsObject) {
        if (stream.added) {
            let newCategories = get(spentCategories);
            newCategories = [...newCategories, stream.added];
            spentCategories.set(newCategories);
        } else if (stream.removed) {
            let newCategories = get(spentCategories);
            newCategories = newCategories.filter((f: protosCategories.SpentCategory.AsObject) => f.id!.value != stream.removed!.id!.value);
            spentCategories.set(newCategories);
        } else if (stream.updated) {
            let newCategories = get(spentCategories);
            newCategories = newCategories.map((f: protosCategories.SpentCategory.AsObject) => {
                if (f.id!.value == stream.updated!.id!.value) {
                    return stream!.updated;
                }
                return f;
            });
            spentCategories.set(newCategories);
        } else if (stream.snapshot) {
            let newCategories = stream.snapshot.spentCategoriesList;
            spentCategories.set(newCategories);
        } else {
            console.error("Unsupported operation");
        }
    }

    function parsePayments(stream: protosPayments.PaymentsStream.AsObject) {
        console.log(stream);
    }

    async function switchCategorized() {
        await SoWService.showCategorized(!showCategorized);
    }

    async function setOrdering(ordering: number) {
        await SoWService.setOrdering(ordering);
    }
    
    function dragStart(ev: DragEvent, payment: protosPayments.PaymentView) {
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
            var payment: protosPayments.PaymentView = JSON.parse(ev.dataTransfer.getData("payment"));
            payment;
        }

        newCategory;
        /*
        await PaymentController.editPayment(payment.id, payment.amount, payment.ccy, payment.what, newCategory.isDebt ? payment.categoryId : newCategory.id, payment.columnId,  newCategory.isDebt ? newCategory.id : payment.debtId, payment.kind);

        if (newCategory.isDebt) {
            payment.debt = newCategory.name;
        } else {
            payment.category = newCategory.name;
        }

        reload();*/
    }

    onDestroy(SoWService.getDebts(parseDebts));
    onDestroy(SoWService.getSpentCategories(parseSpentCategories));
    onDestroy(SoWService.getPayments(parsePayments));

    Link; showCategorized; switchCategorized; sorting; setOrdering; spentCategories; categories; payments; dragStart; dragover; drop; // used in view
/*
    import moment from 'moment';

    import {PaymentController, PaymentViewModelController, SpentCategoryJsModel, DebtModelController } from '../../generated-types';
    import {compare, formatMoney} from '../../services/Shared';
    import { tooltip } from '../../services/Tooltip'
    import PaymentRow from './paymentRow.svelte';

    let sorting = "date"; // "amount"

    let months = [];
    let keys = [];

    function groupBy(list, keyGetter) {
        const map = new Map();
        list.forEach((item) => {
            const key = keyGetter(item);
            const collection = map.get(key);
            if (!collection) {
                map.set(key, [item]);
            } else {
                collection.push(item);
            }
        });
        return map;
    }

    let state = {};

    let reload = async function() {
        state = {};
        if (keys) {
            keys.forEach(t=> {
                state["collapsed_" + t] = months[t].collapsed;

                months[t].values.forEach(val => {
                    if (val.grouped && !!val.expanded) {
                        val.group.forEach(item => {
                            state["collapsed_" + t + "_" + item.id] = true;
                        });
                    }
                })
            });
        }

        let payments = await PaymentViewModelController.list();

        payments.forEach(t=>t.whenMoment = moment(t.when));

        let groupedMonths = groupBy(payments, s => s.whenMoment.format("MM.YYYY"));
        keys = [];

        let iter = groupedMonths.keys();

        var curr = iter.next();
        while(!curr.done) {
            keys.push(curr.value);
            curr = iter.next();
        }

        keys = keys.sort((b,a)=>moment(a, "MM.YYYY").unix() - moment(b, "MM.YYYY").unix());

        keys.forEach(t=>{
            months[t] = {
                collapsed: state["collapsed_" + t] || false,
                values: groupValues(groupedMonths.get(t), "collapsed_" + t + "_"),
                totals: groupedMonths.get(t).reduce((a,b) => {
                    a[b.ccy] = (a[b.ccy] || 0) + b.amount;
                    return a;
                }, {}),
                categoryLess: groupedMonths.get(t).reduce((a,b)  => {
                    if (!b.debt && !b.category) 
                        return a+1;
                    return a;
                }, 0)
            };
        });

        resort()
    }

    let groupValues = function(values, keyPrefix) {
        let grouped = groupBy(values, v => (v.category || v.debt || v.what || '').toLowerCase() + v.ccy + v.kind);

        let iter = grouped.keys();
        var curr = iter.next();
        let a = [];

        let distinctReducer = (a,b) => {
            if (!a && !b)
                return a;
            if (!a && b || !b && a)
                return "-";

            if (a.toString().toLowerCase() == b.toString().toLowerCase())
                return a;
            return "-";
        }

        while (!curr.done) {
            let group = grouped.get(curr.value);

            if (group.length == 1) {
                a.push(group[0]);
            }
            else {
                a.push({
                    grouped: true,
                    expanded: group.map(s=>s.id).reduce((a,b) => a || Object.keys(state).indexOf(keyPrefix + b) != -1, false),
                    whenMoment: group.map(s=>s.whenMoment).reduce((a,b)=>a>b?a:b),
                    when: group.map(s=>s.whenMoment).reduce((a,b)=>a>b?a:b).toString(),
                    amount: group.map(s=>s.amount).reduce((a,b)=>a+b),
                    ccy: group[0].ccy,
                    id: group[0].id,
                    what: group.map(s=>s.what).reduce(distinctReducer),
                    provider: group.map(s=>s.provider).reduce(distinctReducer),
                    account: group.map(s=>s.account).reduce(distinctReducer),
                    category: group.map(s=>s.category).reduce(distinctReducer),
                    debt: group.map(s=>s.debt).reduce(distinctReducer),
                    kind: group.map(s=>s.kind).reduce(distinctReducer),

                    group: group
                });
            }
            curr = iter.next();
        }

        return a;
    }

    let resort = function() {
        let comparer = (b,a)=>{ 
                if (sorting == "date")
                    return compare(a.kind, b.kind) * 10 + a.whenMoment.unix() - b.whenMoment.unix();
                if (sorting == "amount")
                    return compare(a.kind, b.kind) * 100 + compare(a.ccy, b.ccy) * 10 + a.amount - b.amount;
            };
        keys.forEach(t => {
            months[t].values = months[t].values.sort(comparer);

            months[t].values.forEach(val => {
                if (val.grouped) {
                    val.group = val.group.sort(comparer);
                }
            })
        })
    }

    let deletePayment = async function(id) {
        await PaymentController.deletePayment(id);
        
        await reload();
    }

    let showHeader = function(month, hideCategorized) {
        return month.values.filter(s=>!hideCategorized || s.category == null && s.debt == null).length > 0;
    }

    $: resort(sorting);

    reload();*/

</script>

<style>
    th button {
        color:inherit;
        line-height:inherit;
        font-weight:inherit;
        font-size:inherit;
        text-transform:inherit;
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
                                <button class="btn btn-link btn-anchor" on:click="{() => setOrdering(0) }">
                                    Когда
                                    {#if sorting == 0}
                                        <span class="fe fe-chevron-down"></span>
                                    {/if}
                                </button>
                            </th>
                            <th>Тип</th>
                            <th>Категория</th>
                            <th>Провайдер</th>
                            <th>Счёт</th>
                            <th>
                                <button class="btn btn-link btn-anchor" on:click="{() => setOrdering(1) }">
                                    Сумма
                                    {#if sorting == 1}
                                        <span class="fe fe-chevron-down"></span>
                                    {/if}
                                </button>
                            </th>
                            <th>Сообщение</th>
                            <th>Удалить</th>
                        </tr>
                        </thead>
                        <tbody>
<!--
                        {#each keys as monthKey, idx}

                            {#if (showHeader(months[monthKey], hideCategorized))}
                            <tr>
                                <th colspan="8">
                                    <span class="card-title">
                                        <button class="btn btn-link btn-anchor" on:click="{() => months[monthKey].collapsed = !months[monthKey].collapsed}">
                                            {moment(monthKey, "MM.YYYY").format("MMMM YYYY")}
                                        </button>
                                    </span>
                                    {#each Object.entries(months[monthKey].totals) as total}
                                        <span class="badge badge-primary">{formatMoney(total[1])} {total[0]}</span>&nbsp;
                                    {/each}
                                    <span class="badge badge-secondary">
                                        {months[monthKey].categoryLess} без категорий
                                    </span>
                                </th>
                            </tr>
                            {/if}

                            {#if !months[monthKey].collapsed}
                                {#each months[monthKey].values as payment, idx}
                                    <PaymentRow {payment} {hideCategorized} {deletePayment} {dragStart} />
                                {/each}
                            {/if}
                        {/each}-->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>