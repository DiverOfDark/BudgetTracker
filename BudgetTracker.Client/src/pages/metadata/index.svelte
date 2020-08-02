<script lang="ts">
    import {MetadataController} from '../../generated-types';
    import * as interfaces from '../../generated-types';

    import { PlusIcon, ArrowUpIcon, ArrowDownIcon, Edit2Icon, XCircleIcon } from 'svelte-feather-icons';
    
    import Link from '../../svero/Link.svelte';

    let columns: interfaces.MoneyColumnMetadataJsModel[] = [];

    let getUsedIn = function(meta: interfaces.MoneyColumnMetadataJsModel): interfaces.MoneyColumnMetadataJsModel[] {
      return columns.filter(s=> s != meta && s.function != null && (s.function.indexOf("[" + meta.userFriendlyName + "]") != -1 || s.function.indexOf("[" + meta.provider + "/" + meta.accountName + "]") != -1))
    }

    let splitFunc = function(from: string): string[] {
        var result = [];
        var current = "";
        for(var i = 0; i < from.length; i++) {
            if (from[i] == '[')
            {
                result.push(current);
                current = "";
            }
            current = current + from[i];
            if (from[i] == ']')
            {
                result.push(current);
                current = "";
            }
        }

        result.push(current);
        return result;
    }

    let getStyleForReference = function(from: string): string {
        if (from[0] == '[' && from[from.length - 1] == ']') {
            let reference = from.slice(1,-1).split("/");

            let isComputed = reference.length == 1;

            let provider = isComputed ? "Computed" : reference[0];
            let accountName = isComputed ? reference : reference.slice(1).join("/");

            var items = columns.filter(v=> v.provider == provider && (isComputed ? v.userFriendlyName == accountName : v.accountName == accountName)).length;
            
            if (items == 0) {
                return "color: red; font-weight: bold;";
            }
        }
        return "";
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
    getUsedIn; deleteColumn; updateColumnOrder; Link; splitFunc; getStyleForReference;
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
                                    <PlusIcon size="16" />
                                </Link>
                            </th>
                        </tr>
                        {#each columns as meta, i}
                            <tr>
                                <td class="text-nowrap">
                                    {#if meta != columns[0]}
                                        <button on:click="{() => updateColumnOrder(meta.id, true)}" class="btn btn-link btn-anchor">
                                            <ArrowUpIcon size="16" />
                                        </button>
                                    {/if}
                                    {#if meta != columns[columns.length - 1]}
                                        <button on:click="{() => updateColumnOrder(meta.id, false)}" class="btn btn-link btn-anchor" class:pull-right="{meta == columns[0]}">
                                            <ArrowDownIcon size="16" />
                                        </button>
                                    {/if}
                                </td>
                                <td>{meta.provider}</td>
                                <td>
                                    {#if meta.isComputed}
                                        {#each splitFunc(meta.function) as step}
                                            <span style="{getStyleForReference(step)}">{step}</span>
                                        {/each}
                                    {:else}
                                        {meta.accountName}
                                    {/if}
                                </td>
                                <td>{meta.userFriendlyName}</td>
                                <td>
                                    {#each getUsedIn(meta) as usedIn}
                                        {usedIn.userFriendlyName}<br/>
                                    {/each}
                                </td>
                                <td class="text-nowrap">
                                    <Link className="btn btn-link btn-anchor" href="/Metadata/Edit/{meta.id}">
                                        <Edit2Icon size="16" />
                                    </Link>
                                    <button on:click="{() => deleteColumn(meta.id)}" class="btn btn-link btn-anchor">
                                        <XCircleIcon size="16" />
                                    </button>
                                </td>
                            </tr>
                        {/each}
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>
