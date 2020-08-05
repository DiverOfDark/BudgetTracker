
<script lang="ts">
    import LinearChartWidget from './Widgets/linearChartWidget.svelte';
    import * as interfaces from '../../generated-types';

    export let router: any = {};

	let widgetModel: interfaces.LinearChartWidgetViewModel = {
        title: '',
        period: 0,
        values: [],
        columns: 0,
        id: '',
        kind: '',
        rows: 0,
		settings: {},
		dates: [],
		exemptTransfers: false
	};

    let loadData = async function(provider: string, account: string, exemptTransfers: boolean) {
        widgetModel = await interfaces.TableController.chart(provider, account, exemptTransfers)
    }

    $: {
        if (router && router.params) {
            let provider = router.params.provider;
            let account = router.params.account;
            let exemptTransfers = router.params.exemptTransfers || false;

            loadData(provider, account, exemptTransfers);
        }
    }

    LinearChartWidget; widgetModel;
</script>

<svelte:head>
    <title>BudgetTracker - {widgetModel.title}</title>
</svelte:head>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <LinearChartWidget model={widgetModel} fullScreen="true" />
            </div>
        </div>
    </div>
</div>
<!--
@{
    ViewData["Title"] = Model.Title;
    ViewBag.FullScreen = true;
}
-->