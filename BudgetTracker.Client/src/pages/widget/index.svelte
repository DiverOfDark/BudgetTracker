<svelte:head>
    <title>Отчёт - BudgetTracker</title>
</svelte:head>

<script lang="ts">
    import { navigateTo } from '../../svero/utils';

    import { WidgetController } from '../../generated-types';
    import { PlusIcon, CalendarIcon, EditIcon, XCircleIcon, ArrowLeftIcon, ArrowRightIcon, EyeIcon, EyeOffIcon } from 'svelte-feather-icons';
    import * as interfaces from '../../generated-types';
    import donutWidget from './Widgets/donutWidget.svelte';
    import linearChartWidget from './Widgets/linearChartWidget.svelte';
    import deltaWidget from './Widgets/deltaWidget.svelte';
    import exceptionWidget from './Widgets/exceptionWidget.svelte';
    import expensesWidget from './Widgets/expensesWidget.svelte';
    import lastValueWidget from './Widgets/lastValueWidget.svelte';
    import unconfiguredWidget from './Widgets/unconfiguredWidget.svelte';
    import unknownWidget from './Widgets/unknownWidget.svelte';

    let showButtons = false;
    let period = 0;

    let loadData = async () => {
        model = await interfaces.WidgetController.index(period);
    }

    let createWidget = function() {
        navigateTo('/Widget/Edit');
    }

    let editWidget = function(id: string) {
        navigateTo('/Widget/Edit/' + id);
    }
    
    let deleteWidget = async function(id: string) {
        await interfaces.WidgetController.deleteWidget(id);
        await loadData();
    }

    let moveLeft = async function(id: string) {
        await interfaces.WidgetController.moveWidgetLeft(id);
        await loadData();
    }
    
    let moveRight = async function(id: string) {
        await interfaces.WidgetController.moveWidgetRight(id);
        await loadData();
    }

    let createComponent = function(widget: interfaces.WidgetViewModel):any {
        switch(widget.kind) {
            case "LastValueWidgetViewModel":
                return lastValueWidget;
            case "ExpensesWidgetViewModel":
                return expensesWidget;
            case "DeltaWidgetViewModel":
                return deltaWidget;
            case "DonutWidgetViewModel":
                return donutWidget;
            case "LinearChartWidgetViewModel":
                return linearChartWidget;
            case "ExceptionWidgetViewModel":
                return exceptionWidget;
            case "UnconfiguredWidgetViewModel":
                return unconfiguredWidget;
            default:
                console.log("Unsupported widget kind: " + widget.kind);
                return unknownWidget;
        }
    }
    
    let model : interfaces.DashboardViewModel;
    let periodFriendly : String;
    
    $: {
        if (period == 0) periodFriendly = "всё время";
        else periodFriendly = period + " месяцев";
    }

    loadData();
</script>

<div class="container">
    <div class="page-header">
        <h1 class="page-title">
            Отчёт за {periodFriendly}
        </h1>
        <div class="page-options" style="text-align: right;">
            <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => showButtons = !showButtons}">
                {#if (showButtons)}
                    <EyeOffIcon size="16" />
                {:else}
                    <EyeIcon size="16" />
                {/if}
            </button>
            <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => createWidget()}">
                <PlusIcon size="16" />
            </button>
            <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => { period = 1; loadData() }}">
                <CalendarIcon size="16" />1M
            </button>
            <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => { period = 3; loadData() }}">
                <CalendarIcon size="16" />3M
            </button>
            <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => { period = 6; loadData() }}">
                <CalendarIcon size="16" />6M
            </button>
            <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => { period = 0; loadData() }}">
                <CalendarIcon size="16" />Всё
            </button>
        </div>
    </div>    
    {#if model && model.widgets}
    <div class="row card-columns">
        {#each model.widgets as column, idx3}
        <div class="col-lg-{column.columns} col-md-{Math.min(column.columns * 2, 12)} col-sm-12">
            {#each column.rows as widget, idx2 (widget.id)}
                <div class="card">
                    {#if showButtons}
                        <div class="card-header">
                            {widget.title}

                            <div class="card-options">
                                <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => editWidget(widget.id)}">
                                    <EditIcon size="16" />
                                </button>
                                <button class="btn btn-sm btn-outline-primary ml-1" on:click="{() => deleteWidget(widget.id)}">
                                    <XCircleIcon size="16" />
                                </button>
                            </div>
                        </div>
                    {/if}
                    <div style="height: {widget.rows * 12 * 14 + (widget.rows - 1) * 12 * 2}px">
                        <svelte:component this={createComponent(widget)} model={widget} />
                    </div>

                    {#if showButtons}
                        <div class="card-footer">
                            <button class="float-left btn btn-sm btn-outline-primary" on:click="{() => moveLeft(widget.id)}">
                                <ArrowLeftIcon size="16" />
                            </button>
                            <button class="float-right btn btn-sm btn-outline-primary" on:click="{() => moveRight(widget.id)}">
                                <ArrowRightIcon size="16" />
                            </button>
                        </div>
                    {/if}
                </div>
            {/each}
        </div>
        {/each}
    </div>
    {/if}
</div>