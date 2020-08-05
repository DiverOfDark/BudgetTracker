<script >
    import { GitBranchIcon, Edit2Icon, XCircleIcon } from 'svelte-feather-icons';
    import { formatUnixMonth, formatUnixTime, formatPaymentKind, formatMoney } from '../../services/Shared';
    import { tooltip } from '../../services/Tooltip';

    export let parentId;
    export let payment;
    export let expandCollapse;
    export let dragStart;
    export let editPayment;
    export let deletePayment;
    export let splitPayment;

    export let debts;
    export let spentCategories;
    export let moneyColumns;

    let providerName = "";
    let accountName = "";
    $: {
        let columnId = payment.group ? payment.group.columnId : payment.payment ? payment.payment.columnId : undefined;
        if (columnId) {
            let column = $moneyColumns.find(t=>t.id.value == columnId.value);
            providerName = column.provider;
            accountName = column.accountName;
        }
    }

    let categoryName = "???";
    $: {
        let item = payment.group ? payment.group : payment.payment ? payment.payment : undefined;
        if (item) {
            if (item.categoryId.value) {
                let foundItem = $spentCategories.find(t=>t.id.value == item.categoryId.value);
                categoryName = foundItem != null ? foundItem.category : "!!!";
            }
            if (item.debtId.value) {
                let foundItem = $debts.find(t=>t.model.id.value == item.debtId.value);
                categoryName = foundItem != null ? foundItem.model.description : "!!!";
            }
        }
    }
</script>

<style>
    th button {
        color:inherit;
        line-height:inherit;
        font-weight:inherit;
        font-size:inherit;
        text-transform:inherit;
    }

    .italic {
        font-style: italic;
    }
</style>

{#if payment.summary}
    <tr>
        <th colspan="8">
            <span class="card-title">
                <button class="btn btn-link btn-anchor" on:click="{() => expandCollapse([...parentId, payment.summary.id])}">
                    {formatUnixMonth(payment.summary.when.seconds)}
                </button>
            </span>
            {#each payment.summary.summaryList as total}
                <span class="badge badge-primary">{formatMoney(total.amount)} {total.currency}</span>&nbsp;
            {/each}
            <span class="badge badge-secondary">
                {payment.summary.uncategorizedCount} без категорий
            </span>
        </th>
    </tr>
    {#each payment.summary.paymentsList as item, idx}
        <svelte:self payment={item} {moneyColumns} {debts} {spentCategories} {expandCollapse} {dragStart} {editPayment} {deletePayment} {splitPayment} parentId="{[ ...parentId, payment.summary.id ]}" />
    {/each}
{:else if (payment.group)}
    <tr>
        <td class="text-nowrap">{formatUnixTime(payment.group.when.seconds)}</td>
        <td class="text-nowrap">{formatPaymentKind(payment.group.kind)}</td>
        <td class="text-nowrap">{categoryName}</td>
        <td class="text-nowrap">{providerName}</td>
        <td class="text-nowrap">{accountName}</td>
        <td class="text-nowrap">
            <b>{formatMoney(payment.group.amount)}</b> 
            <i>{payment.group.ccy}</i>
        </td>
        <td>
            <b>{payment.group.what}</b>
            <button class="btn btn-link btn-anchor" on:click="{() => expandCollapse([...parentId, payment.group.id])}">
                ({payment.group.paymentCount})
            </button>
        </td>
        <td class="text-nowrap">
            <button class="btn btn-link btn-anchor" on:click="{() => expandCollapse([...parentId, payment.group.id])}">
                ({payment.group.paymentCount})
            </button>
        </td>
    </tr>
    {#each payment.group.paymentsList as item, idx}
        <svelte:self payment={item} {moneyColumns} {debts} {spentCategories} {expandCollapse} {dragStart} {editPayment} {deletePayment} {splitPayment} parentId="{[ ...parentId, payment.group.id ]}" />
    {/each}
{:else if (payment.payment)}
    <tr class="{parentId.length > 1 ? "italic" : ""}">
        <td class="text-nowrap">{formatUnixTime(payment.payment.when.seconds)}</td>
        <td class="text-nowrap">{formatPaymentKind(payment.payment.kind)}</td>
        <td class="text-nowrap">
            <span class="btn btn-sm btn-info p-1 m-1" style="cursor: grab" draggable={true} on:dragstart={event => dragStart(event, payment.payment)}>
                {categoryName}
            </span>
        </td>
        <td class="text-nowrap">{providerName}</td>
        <td class="text-nowrap">{accountName}</td>
        <td class="text-nowrap"><b>{formatMoney(payment.payment.amount)}</b> <i>{payment.payment.ccy}</i>
        </td>
        <td><b>{payment.payment.what}</b></td>
        <td class="text-nowrap">
            <button class="btn btn-link btn-anchor" on:click="{() => splitPayment(payment.payment)}">
                <span class="fe fe-git-branch"  use:tooltip="{"Разделить"}"></span>
            </button>&nbsp;
            <button class="btn btn-link btn-anchor" on:click="{() => editPayment(payment.payment)}">
                <span class="fe fe-edit-2" use:tooltip="{"Редактировать"}"></span>
            </button>&nbsp;
            <button class="btn btn-link btn-anchor" on:click="{() => deletePayment(payment.payment)}">
                <span class="fe fe-x-circle" use:tooltip="{"Удалить"}"></span>
            </button>
        </td>
    </tr>
{:else}
    <tr>
        <td colspan="7">{JSON.stringify(payment)}</td>
    </tr>
{/if}