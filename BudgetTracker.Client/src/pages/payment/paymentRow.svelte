<script>
    export let payment;
    export let hideCategorized;

    export let deletePayment;

    export let useItalic = false;

    import moment from 'moment'

    import Link from '../../svero/Link.svelte';
    import {formatMoney} from '../../services/Shared';
    import {tooltip} from '../../services/Tooltip';

    $: tooltipText = payment.expanded ? "Сгруппировать": "Разгруппировать";
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

{#if payment.category == null && payment.debt == null || !hideCategorized}
    <tr class="collapse-@month.Key collapse show" class:bold="{payment.expanded}" class:italic="{useItalic}">
        <td class="text-nowrap">{payment.whenMoment.format("DD.MM.YY H:mm:ss")}</td>
        <td class="text-nowrap">{payment.kind}</td>
        <td class="text-nowrap">{(payment.category == null ? payment.debt : payment.category) || ''}</td>
        <td class="text-nowrap">{payment.provider}</td>
        <td class="text-nowrap">{payment.account}</td>
        <td class="text-nowrap">
            <b>{formatMoney(payment.amount)}</b> 
            <i>{payment.ccy}</i>
        </td>
        <td>
            <b>{payment.what}</b>
            {#if !!payment.grouped}
                <button class="btn btn-link btn-anchor" on:click="{() => payment.expanded = !payment.expanded}" use:tooltip="{tooltipText}">
                    ({payment.group.length})
                </button>
            {/if}
        </td>
        <td class="text-nowrap">
            {#if !!payment.grouped}
                <button class="btn btn-link btn-anchor" on:click="{() => payment.expanded = !payment.expanded}" use:tooltip="{tooltipText}">
                    ({payment.group.length})
                </button>
            {:else}
                <Link class="btn btn-link btn-anchor" href="/Payment/Split/{payment.id}">
                    <span class="fe fe-git-branch"  use:tooltip="{"Разделить"}"></span>
                </Link>&nbsp;
                <Link class="btn btn-link btn-anchor" href="/Payment/Edit/{payment.id}">
                    <span class="fe fe-edit-2" use:tooltip="{"Редактировать"}"></span>
                </Link>&nbsp;
                <button class="btn btn-link btn-anchor" on:click="{() => deletePayment(payment.id)}" use:tooltip="{"Удалить"}">
                    <span class="fe fe-x-circle"></span>
                </button>
            {/if}
        </td>
    </tr>

    {#if payment.expanded}
        {#each payment.group as childPayment}
            <svelte:self useItalic="true" payment={childPayment} {hideCategorized} {deletePayment} />
        {/each}
        <tr>
            <td colspan="8">
                &nbsp;
            </td>
        </tr>
    {/if}
{/if}