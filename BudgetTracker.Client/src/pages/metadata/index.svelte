<script lang="ts">
    import {MetadataController, MoneyColumnMetadataJsModel} from '../../generated-types';
    
    import Link from '../../svero/Link.svelte';

    let columns: MoneyColumnMetadataJsModel[] = [];

    let getUsedIn = function(meta: MoneyColumnMetadataJsModel): MoneyColumnMetadataJsModel[] {
      return columns.filter(s=> s != meta && s.function != null && (s.function.indexOf("[" + meta.userFriendlyName + "]") != -1 || s.function.indexOf("[" + meta.provider + "/" + meta.accountName + "]") != -1))
    }

    MetadataController.indexJson().then(i=>columns = i);

    let deleteColumn = async function(id: string) {
        await MetadataController.metadataDelete(id);
        columns = await MetadataController.indexJson();
    }

    let updateColumnOrder = async function(id: string, moveUp: boolean) {
        await MetadataController.updateColumnOrder(id, moveUp);
        columns = await MetadataController.indexJson();
    };

    // used in view
    getUsedIn; deleteColumn; updateColumnOrder; Link;
</script>

<style>
    .pull-right {
        float: right;
    }
</style>

<svelte:head>
    <title>BudgetTracker - Столбцы</title>
</svelte:head>
<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    Столбцы
                </div>
                <div class="table-responsive">
                    <table class="table card-table table-sm table-vcenter table-hover table-striped">
                        <tr>
                            <th></th>
                            <th>Поставщик</th>
                            <th>Название</th>
                            <th>Дружелюбное имя</th>
                            <th>Используется</th>
                            <th>
                                <Link href="/Metadata/Edit">
                                    <span class="fe fe-plus"></span>
                                </Link>
                            </th>
                        </tr>
                        {#each columns as meta}
                            <tr>
                                <td class="text-nowrap">
                                    {#if meta != columns[0]}
                                        <button on:click="{() => updateColumnOrder(meta.id, true)}" class="btn btn-link btn-anchor">
                                            <span class="fe fe-arrow-up"></span>
                                        </button>
                                    {/if}
                                    {#if meta != columns[columns.length - 1]}
                                        <button on:click="{() => updateColumnOrder(meta.id, false)}" class="btn btn-link btn-anchor" class:pull-right="{meta == columns[0]}">
                                            <span class="fe fe-arrow-down"></span>
                                        </button>
                                    {/if}
                                </td>
                                <td>{meta.provider}</td>
                                <td>{!meta.isComputed ? meta.accountName : meta.function}</td>
                                <td>{meta.userFriendlyName}</td>
                                <td>
                                    {#each getUsedIn(meta) as usedIn}
                                        {usedIn.userFriendlyName}<br/>
                                    {/each}
                                </td>
                                <td class="text-nowrap">
                                    <Link className="btn btn-link btn-anchor" href="/Metadata/Edit/{meta.id}">
                                        <span class="fe fe-edit-2"></span>
                                    </Link>
                                    {#if meta.canDelete}
                                        <button on:click="{() => deleteColumn(meta.id)}" class="btn btn-link btn-anchor">
                                            <span class="fe fe-x-circle"></span>
                                        </button>
                                    {/if}
                                </td>
                            </tr>
                        {/each}
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>
