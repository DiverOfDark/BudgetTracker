<script >
    import { formatUnixMonth, formatUnixTime, formatPaymentKind, formatMoney } from '../../services/Shared';
    import { tooltip } from '../../services/Tooltip';

    export let parentId;
    export let payment;
    export let expandCollapse;
    export let dragStart;
    export let editPayment;
    export let deletePayment;
    export let splitPayment;
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

<tr>
    {#if payment.summary}
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
    {:else if (payment.group)}
        <td class="text-nowrap">{formatUnixTime(payment.group.when.seconds)}</td>
        <td class="text-nowrap">{formatPaymentKind(payment.group.kind)}</td>
        <td class="text-nowrap">{(payment.group.category == null ? payment.group.debt : payment.group.category) || '???'}</td>
        <td class="text-nowrap">{payment.group.provider}</td>
        <td class="text-nowrap">{payment.group.account}</td>
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
    {:else if (payment.payment)}
        <td class="text-nowrap">{formatUnixTime(payment.payment.when.seconds)}</td>
        <td class="text-nowrap">{formatPaymentKind(payment.payment.kind)}</td>
        <td class="text-nowrap">
            <span class="btn btn-sm btn-info p-1 m-1" style="cursor: grab"
                draggable={true} on:dragstart={event => dragStart(event, payment.payment)}>
                {(payment.payment.category == null ? payment.payment.debt : payment.payment.category) || '???'}
            </span>
        </td>
        <td class="text-nowrap">{payment.payment.provider}</td>
        <td class="text-nowrap">{payment.payment.account}</td>
        <td class="text-nowrap">
            <b>{formatMoney(payment.payment.amount)}</b> 
            <i>{payment.payment.ccy}</i>
        </td>
        <td>
            <b>{payment.payment.what}</b>
        </td>
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
    {/if}
</tr>