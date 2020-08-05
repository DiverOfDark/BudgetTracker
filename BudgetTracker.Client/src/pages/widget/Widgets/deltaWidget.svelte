<script lang="ts">
    import * as interfaces from './../../../generated-types';
    import { formatMoney } from './../../../services/Shared';
    import { ChevronsUpIcon, ChevronsDownIcon, CodeIcon } from 'svelte-feather-icons';

    export let model: interfaces.DeltaWidgetViewModel = {
        ccy: '',
        deltas: [],
        deltaWidgetSettings: {
            accountName: '',
            providerName: '',
            deltaInterval: interfaces.DeltaIntervalEnum.Daily
        },
        settings: {},
        title: '',
        incompleteData:false,
        columns: 2,
        id: '',
        kind: '',
        rows: 2
    };
    let colorSuffix;
    let styleSuffix;

	$: { colorSuffix = model.incompleteData ? "bg-gray-dark-darkest" : ""; }
    $: { styleSuffix = model.deltas.length < 4 ? "mt-5" : ""; }

    let color = function(item: interfaces.Pair) {
        return item.value > 0 ? "text-green" : Math.abs(item.value) < 0.01 ? "text-blue" : "text-red";
    }

    let format = function(item: interfaces.Pair) {
        let value = formatMoney(item.value);

        let prefix = item.value > 0 ? "+" : "";

        return prefix + value;
    }
</script>

<div class="card-body p-3 text-center @colorSuffix">
    <div class="h3 @styleSuffix">
    {#each model.deltas as item}
            <div class="text-nowrap">
                <span class="text-muted-dark">&Delta; {item.name}: </span>
                <span class="{color(item)}">{format(item)}</span>&nbsp;<span class="text-muted-dark">{model.ccy}</span>
                <span class="{color(item)}">
                    {#if (item.value > 0)}
                        <ChevronsUpIcon size="16" />
                    {:else if (Math.abs(item.value) < 0.01)}
                        <CodeIcon size="16" />
                    {:else}
                        <ChevronsDownIcon size="16" />
                    {/if}
                </span>
            </div>
    {/each}
    </div>
    <div class="text-muted">
        {model.title}
    </div>
</div>
