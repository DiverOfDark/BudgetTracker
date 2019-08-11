<svelte:head>
    <title>BudgetTracker - Движение денежных средств</title>
</svelte:head>

<script>
    import Link from '../../svero/Link.svelte';
    import moment from 'moment';

    import {PaymentController, PaymentViewModelController } from '../../generated-types';
    import {compare, formatMoney} from '../../services/Shared';
    import { tooltip } from '../../services/Tooltip'
    import PaymentRow from './paymentRow.svelte';

    let hideCategorized = false;

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

    reload();
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
                        <button class="btn btn-secondary btn-sm ml-2" on:click="{() => hideCategorized = !hideCategorized}">
                            {hideCategorized ? "Показать все" : "Скрыть с категориями"}
                        </button>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table card-table table-sm table-hover table-striped">
                        <thead>
                        <tr>
                            <th>
                                <button class="btn btn-link btn-anchor" on:click="{() => sorting = "date"}">
                                    Когда
                                    {#if sorting == "date"}
                                        <span class="fe fe-chevron-down"></span>
                                    {/if}
                                </button>
                            </th>
                            <th>Тип</th>
                            <th>Категория</th>
                            <th>Провайдер</th>
                            <th>Счёт</th>
                            <th>
                                <button class="btn btn-link btn-anchor" on:click="{() => sorting = "amount"}">
                                    Сумма
                                    {#if sorting == "amount"}
                                        <span class="fe fe-chevron-down"></span>
                                    {/if}
                                </button>
                            </th>
                            <th>Сообщение</th>
                            <th>Удалить</th>
                        </tr>
                        </thead>
                        <tbody>
                        {#each keys as monthKey}

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
                                {#each months[monthKey].values as payment}
                                    <PaymentRow {payment} {hideCategorized} {deletePayment} />
                                {/each}
                            {/if}
                        {/each}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>