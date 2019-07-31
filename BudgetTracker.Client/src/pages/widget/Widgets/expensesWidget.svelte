<script lang="ts">
	import { ExpensesWidgetViewModel } from './../../../generated-types';
    import { formatMoney } from './../../../services/Shared';
	import { onMount } from 'svelte';
	import c3 from 'c3';

	export let model: ExpensesWidgetViewModel = {
        title: '',
        period: 0,
        names: [],
        values: [],
        columns: 0,
        expenseSettings: {
            currency: ''
        },
        id: '',
        kind: '',
        rows: 0,
        settings: {}
    };

	let chartDiv: HTMLElement;

    let refresh = function() {
        if (!chartDiv) {
            return () => {}
        }

        let columnsData = model.values.map((t,i)=> [ 'col' + i, t  ])
        let namesData : any = {};
        for(var i =0;i<model.names.length; i++) {
            namesData['col' + i] = model.names[i];
        }

        var valuesSum = model.values.reduce((a,b) => a+b);
    
        var columns = [...columnsData];
        
        var chart = c3.generate({
            bindto: chartDiv,
            data: {
                columns: columns,
                type: 'donut',
                order: null,
                names: namesData
            },
            donut: {
                title: formatMoney(valuesSum) + " " + model.expenseSettings.currency,
                label: {
                    format: formatMoney
                }
            },
            tooltip: {
                format: {
                    value: function (value) {
                        return formatMoney(value) + " " + model.expenseSettings.currency;
                    }
                }
            },
            size: {
                height: 300
            },
            axis: {
            },
            legend: {
                show: true,
                position: 'right'
            },
            padding: {
                bottom: 0,
                top: 0
            }
        });

        return chart.unload;
    }

    $: { model && refresh(); }
	onMount(() => refresh());
</script>

<div class="card-body">
    <div class="h4 text-muted-dark text-center">
        {model.title} ({model.period} месяц)
    </div>
    <div bind:this="{chartDiv}" class="pie-chart" style="height: 100%;"></div>
</div>