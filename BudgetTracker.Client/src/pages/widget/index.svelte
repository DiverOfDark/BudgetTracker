<svelte:head>
    <title>Отчёт - BudgetTracker</title>
</svelte:head>

<script lang="ts">
    import { WidgetController, WidgetViewModel } from '../../generated-types';
    import burstWidget from './Widgets/burstWidget.svelte';
    import chartWidget from './Widgets/chartWidget.svelte';
    import deltaWidget from './Widgets/deltaWidget.svelte';
    import exceptionWidget from './Widgets/exceptionWidget.svelte';
    import expensesWidget from './Widgets/expensesWidget.svelte';
    import lastValueWidget from './Widgets/lastValueWidget.svelte';
    import unconfiguredWidget from './Widgets/unconfiguredWidget.svelte';
    import unknownWidget from './Widgets/unknownWidget.svelte';

    let showButtons = false;
    let period = 0;

    let loadData = async () => {
        model = await WidgetController.index(period);
    }

    let createWidget = function() {
        // TODO;
    }

    let editWidget = async function(id: string) {
        id; // TODO
    }
    
    let deleteWidget = async function(id: string) {
        await WidgetController.deleteWidget(id);
        await loadData();
    }

    let moveLeft = async function(id: string) {
        await WidgetController.moveWidgetLeft(id);
        await loadData();
    }
    
    let moveRight = async function(id: string) {
        await WidgetController.moveWidgetRight(id);
        await loadData();
    }

    let createComponent = function(widget: WidgetViewModel):any {
        switch(widget.kind) {
            case "LastValueWidgetViewModel":
                return lastValueWidget;
            case "ExpensesWidgetViewModel":
                return expensesWidget;
            case "DeltaWidgetViewModel":
                return deltaWidget;
            case "ChartWidgetViewModel":
                return chartWidget;
            case "BurstWidgetViewModel":
                return burstWidget;
            case "ExceptionWidgetViewModel":
                return exceptionWidget;
            case "UnconfiguredWidgetViewModel":
                return unconfiguredWidget;
            default:
                console.log("Unsupported widget kind: " + widget.kind);
                return unknownWidget;
        }
    }
    
    let model;
    let periodFriendly;
    
    $: {
        if (period == 0) periodFriendly = "всё время";
        else periodFriendly = period + " месяцев";
    }

    loadData();

    // used implicitly
    showButtons; period; moveLeft; moveRight; deleteWidget; createWidget; editWidget; model; periodFriendly; createComponent;
</script>

<div class="container">
    <div class="page-header">
        <h1 class="page-title">
            Отчёт за {periodFriendly}
        </h1>
        <div class="page-options d-flex">
            <button class="btn btn-sm btn-secondary ml-1" on:click="{() => showButtons = !showButtons}">
                <span class="fe" class:fe-eye-off="{showButtons}" class:fe-eye="{!showButtons}"></span>
            </button>
            <button class="btn btn-sm btn-secondary ml-1" on:click="{() => createWidget()}">
                <span class="fe fe-plus"></span>
            </button>
            <button class="btn btn-sm btn-secondary ml-1" on:click="{() => loadData(1)}">
                <span class="fe fe-calendar">1M</span>
            </button>
            <button class="btn btn-sm btn-secondary ml-1" on:click="{() => loadData(3)}">
                <span class="fe fe-calendar">3M</span>
            </button>
            <button class="btn btn-sm btn-secondary ml-1" on:click="{() => loadData(6)}">
                <span class="fe fe-calendar">6M</span>
            </button>
            <button class="btn btn-sm btn-secondary ml-1" on:click="{() => loadData(0)}">
                <span class="fe fe-calendar">Всё</span>
            </button>
        </div>
    </div>    
    {#if model && model.widgets}
    <div class="row card-columns">
        {#each model.widgets as column}
        <div class="col-lg-{column.columns} col-md-{Math.min(column.columns * 2, 12)} col-sm-12">
            {#each column.rows as widget, idx2 (widget.id)}
                <div class="card">
                    {#if showButtons}
                        <div class="card-header">
                            {widget.title}

                            <div class="card-options">
                                <button class="btn btn-sm btn-secondary ml-1" on:click="{() => editWidget(widget.id)}">
                                    <span class="fe fe-edit">
                                    </span>
                                </button>
                                <button class="btn btn-sm btn-secondary ml-1" on:click="{() => deleteWidget(widget.id)}">
                                    <span class="fe fe-x-circle">
                                    </span>
                                </button>
                            </div>
                        </div>
                    {/if}
                    <div style="height: {widget.rows * 12 * 14 + (widget.rows - 1) * 12 * 2}px">
                        <svelte:component this={createComponent(widget)} model={widget} />
                    </div>

                    {#if showButtons}
                        <div class="card-footer mb-5">
                            <button class="float-left btn btn-sm btn-secondary" on:click="{() => moveLeft(widget.id)}">
                                <span class="fe fe-arrow-left"></span>
                            </button>
                            <button class="float-right btn btn-sm btn-secondary" on:click="{() => moveRight(widget.id)}">
                                <span class="fe fe-arrow-right"></span>
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