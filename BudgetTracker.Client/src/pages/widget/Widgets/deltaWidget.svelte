<script lang="ts">
    import * as interfaces from './../../../generated-types';
    import { formatMoney } from './../../../services/Shared';

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

    let direction = function(item: interfaces.Pair) {
        return item.value > 0 ? "fe-chevrons-up" : Math.abs(item.value) < 0.01 ? "fe-code" : "fe-chevrons-down";  
    }

    color;format;direction; colorSuffix; styleSuffix;
</script>

<div class="card-body p-3 text-center @colorSuffix">
    <div class="h3 @styleSuffix">
    {#each model.deltas as item}
            <div class="text-nowrap">
                <span class="text-muted-dark">&Delta; {item.name}: </span>
                <span class="{color(item)}">{format(item)}</span>&nbsp;<span class="text-muted-dark">{model.ccy}</span>
                <span class="fe {color(item)} fe {direction(item)}"></span>
            </div>
    {/each}
    </div>
    <div class="text-muted">
        {model.title}
    </div>
</div>
