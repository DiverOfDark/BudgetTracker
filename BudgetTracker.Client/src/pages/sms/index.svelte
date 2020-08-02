<svelte:head>
    <title>BudgetTracker - SMS</title>
</svelte:head>

<style type="text/css">
    .month {
        color:inherit;
        line-height:inherit;
        font-weight:inherit;
        font-size:inherit;
        text-transform:inherit;
    }
</style>

<script>
    import Link from '../../svero/Link.svelte';
    import { SmsListController } from '../../generated-types';
    import { XCircleIcon } from 'svelte-feather-icons';
    import { formatDate, formatDateTime, formatMonth } from '../../services/Shared'

    let reload = async function() {
        months = await SmsListController.indexJson();
        months.forEach(element => {
            element.collapsed = false;
        });
    };
    
    let deleteSms = async function(id) {
        await SmsListController.deleteSms(id);
        await reload();
    }

    let showHidden = false;
    let months = [];

    reload();
</script>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">
                        SMS 
                    </h3>
                    <div class="card-options">
                        <Link class="btn btn-primary btn-sm" href="/SmsList/SmsRules">
                            Правила обработки SMS
                        </Link>
                        <button class="btn btn-outline-primary btn-sm ml-2" on:click="{() => showHidden = !showHidden}">
                            {#if showHidden}
                                Скрыть обработанное
                            {:else}
                                Показать обработанное
                            {/if}
                        </button>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table card-table table-sm table-hover table-striped">
                        <thead>
                            <tr>
                                <th>Когда</th>
                                <th>От кого</th>
                                <th>Сообщение</th>
                                <th>Удалить</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each months as month, idx}
                                <tr>
                                    <th colspan="4">
                                        <span class="card-title">
                                            <button on:click="{() => month.collapsed = !month.collapsed}" class="btn btn-anchor btn-link month">
                                                {formatMonth(month.when)}
                                            </button>
                                        </span>
                                        <span class="badge badge-secondary">{showHidden ? month.sms.length : month.sms.filter(t=>!t.isHidden).length} SMS</span>
                                    </th>
                                </tr>
                                {#if !month.collapsed}
                                    {#each month.sms as sms, idx}
                                        {#if !sms.isHidden || showHidden}
                                            <tr>
                                                <td class="text-nowrap">{formatDateTime(sms.when)}</td>
                                                <td>{sms.from}</td>
                                                <td>{sms.message}</td>
                                                <td>
                                                    <button class="btn btn-link btn-anchor" on:click="{() => deleteSms(sms.id)}">
                                                        <XCircleIcon size="16" />
                                                    </button>
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