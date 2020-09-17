<script lang="ts">
    import { GitBranchIcon, Edit2Icon, XCircleIcon } from 'svelte-feather-icons';
    import { formatUnixMonth, formatUnixTime, formatPaymentKind, formatMoney } from '../../services/Shared';
    import { tooltip } from '../../services/Tooltip';
    import * as interfaces from '../../generated/Payments_pb';
    import * as commonsProtos from '../../generated/Commons_pb';
    import * as svelte from 'svelte/store';
    import * as accountsProtos from '../../generated/Accounts_pb';
    import * as debtsProtos from '../../generated/Debts_pb';
    import * as spentCategoriesProtos from '../../generated/SpentCategories_pb';

    export let parentId : commonsProtos.UUID.AsObject[];
    export let payment : interfaces.PaymentView.AsObject;
    export let expandCollapse: (ids: commonsProtos.UUID.AsObject[]) => void;
    export let dragStart: (ev: DragEvent, payment: interfaces.Payment.AsObject) => void;
    export let editPayment: (p: interfaces.Payment.AsObject) => void;
    export let deletePayment: (p: interfaces.Payment.AsObject) => void;
    export let splitPayment: (p: interfaces.Payment.AsObject) => void;

    export let debts : svelte.Writable<debtsProtos.DebtView.AsObject[]>;
    export let spentCategories : svelte.Writable<spentCategoriesProtos.SpentCategory.AsObject[]>;
    export let moneyColumns : svelte.Writable<accountsProtos.MoneyColumnMetadata.AsObject[]>;

    let providerName = "";
    let accountName = "";
    $: {
        let columnId = payment.group ? payment.group.columnId : payment.payment ? payment.payment.columnId : undefined;
        if (columnId) {
            let column = $moneyColumns.find(t=>t!.id!.value == columnId!.value);
            providerName = column!.provider;
            accountName = column!.accountName;
        }
    }

    let categoryName = "???";
    $: {
        let item = payment.group ? payment.group : payment.payment ? payment.payment : undefined;
        if (item) {
            if (item!.categoryId!.value) {
                let foundItem = $spentCategories.find(t=>t!.id!.value == item!.categoryId!.value);
                categoryName = foundItem != null ? foundItem.category : "!!!";
            }
            if (item!.debtId!.value) {
                let foundItem = $debts.find(t=>t!.model!.id!.value == item!.debtId!.value);
                categoryName = foundItem != null ? foundItem!.model!.description : "!!!";
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
            <a href="#/" on:click="{() => { expandCollapse([...parentId, payment.summary.id]); return false;}}">
                {formatUnixMonth(payment.summary.when.seconds)}
            </a>
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
            <a href="#/" on:click="{() => { expandCollapse([...parentId, payment.group.id]); return false;}}">
                ({payment.group.paymentCount})
            </a>
        </td>
        <td class="text-nowrap">
            <a href="#/" on:click="{() => { expandCollapse([...parentId, payment.group.id]); return false;}}">
                ({payment.group.paymentCount})
            </a>
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
            <a href="#/"  on:click="{() => { splitPayment(payment.payment); return false;}}" use:tooltip="{"Разделить"}">
                <GitBranchIcon size="16" />
            </a>&nbsp;
            <a href="#/" on:click="{() => { editPayment(payment.payment); return false;}}" use:tooltip="{"Редактировать"}">
                <Edit2Icon size="16" />
            </a>&nbsp;
            <a href="#/" on:click="{() => { deletePayment(payment.payment); return false;}}" use:tooltip="{"Удалить"}">
                <XCircleIcon size="16" />
            </a>
        </td>
    </tr>
{:else}
    <tr>
        <td colspan="7">{JSON.stringify(payment)}</td>
    </tr>
{/if}