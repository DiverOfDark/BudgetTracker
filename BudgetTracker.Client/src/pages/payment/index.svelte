<svelte:head>
    <title>BudgetTracker - Движение денежных средств</title>
</svelte:head>

<script>
    import {Link} from 'svero';
    import moment from 'moment';

    import {PaymentController} from '../../generated-types';
    import {compare, formatMoney} from '../../services/Shared';

    let showGrouped = true;
    let hideCategorized = true;

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

    let reload = async function() {
        let payments = await PaymentController.index();

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
                collapsed: true,
                values: groupedMonths.get(t),
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

    let regroup = function() {
        keys.forEach(t => {
            months[t].groupedValues = months[t].values.reduce(function(a,b) {
                a.push(b);
                return a;
            }, []);
        });
    }

    let resort = function() {
        keys.forEach(t => {
            months[t].values = months[t].values.sort((b,a)=>{ 
                if (sorting == "date")
                    return a.whenMoment.unix() - b.whenMoment.unix();
                if (sorting == "amount")
                    return compare(a.ccy, b.ccy) * 10 + a.amount - b.amount;
            });
        })
        regroup();
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
<!--
                        @if (ViewBag.Groups == null)
                        {
                            <span class="badge badge-primary">@Model.First().PaymentModels.First().Category</span>                            
                        }
-->
                     </h3>
                    <div class="card-options">
                        &nbsp;
                        <Link class="btn btn-primary btn-sm" href="/Payment/Category">
                            Категории расходов
                        </Link>
                        <button class="btn btn-secondary btn-sm ml-2" on:click="{() => showGrouped = !showGrouped}">
                            {showGrouped ? "Разгруппировать": "Сгруппировать"}
                        </button>
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

                            {#if !months[monthKey].collapsed}
                                {#each months[monthKey].values as payment}
                                <!-- sorting!-->
                                {#if payment.category == null && payment.debt == null || !hideCategorized}
                                <tr class="collapse-@month.Key collapse show">
                                    <td class="text-nowrap">{moment(payment.when).format("DD.MM.YY H:mm:ss")}</td>
                                    <td class="text-nowrap">{payment.kind}</td>
                                    <td class="text-nowrap">{(payment.category == null ? payment.debt : payment.category) || ''}</td>
                                    <td class="text-nowrap">{payment.provider}</td>
                                    <td class="text-nowrap">{payment.account}</td>
                                    <td>
                                        <b>{formatMoney(payment.amount)}</b> 
                                        <i>{payment.ccy}</i>
                                    </td>
                                    <td>
                                        <b>{payment.what}</b>
                                        <!--
                                        @if (payment.Count > 1)
                                        {
                                            <a href="@Url.Action("PaymentList", new { id=payment.Id })" target="_blank">
                                                (@payment.Count)
                                            </a>
                                        }
                                        -->
                                    </td>
                                    <td>
                                        <!--
                                        @if (payment.Count > 1)
                                        {
                                            <a href="@Url.Action("PaymentList", new { id=payment.Id })" target="_blank">
                                                (@payment.Count)
                                            </a>
                                        }
                                        else
                                        {
                                            <a href="@Url.Action("SplitPayment", new {id = payment.Id})">
                                                <span class="fe fe-git-branch"></span>
                                            </a>
                                            <a href="@Url.Action("EditPayment", new {id = payment.Id})">
                                                <span class="fe fe-edit-2"></span>
                                            </a>
                                            <a href="@Url.Action("DeletePayment", new {id = payment.Id})">
                                                <span class="fe fe-x-circle"></span>
                                            </a>
                                        }
                                        -->
                                    </td>
                                </tr>
                                {/if}
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