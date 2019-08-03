
<script>
    import LinearChartWidget from './Widgets/linearChartWidget.svelte';
    import { TableController, LinearChartWidgetViewModel } from '../../generated-types';

    export let router = {};

    let widgetModel;

    let loadData = async function(provider, account, exemptTransfers) {
        widgetModel = await TableController.chart(provider, account, exemptTransfers)
    }

    $: {
        if (router && router.params) {
            let provider = router.params.provider;
            let account = router.params.account;
            let exemptTransfers = router.params.exemptTransfers || false;

            loadData(provider, account, exemptTransfers);
        }
    }
</script>



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