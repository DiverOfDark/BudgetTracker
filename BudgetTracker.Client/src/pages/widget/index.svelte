<svelte:head>
    <title>Отчёт - BudgetTracker</title>
</svelte:head>

<script lang="ts">
    import { WidgetController } from '../../generated-types';

    let showButtons = false;
    let period = 0;

    let format = (month:number) => {
        if (month == 0) return "всё время";
        return month + " месяцев";
    }

    let loadData = async (month:number) => {
        period = month;
        model = await WidgetController.index(month);
    }

    let createWidget = function() {

    }

    let editWidget = function() {

    }
    
    let deleteWidget = function() {
        
    }

    let moveLeft = function() {
        
    }
    
    let moveRight = function() {
        
    }
    
    let model;
    let periodFriendly;
    
    $: periodFriendly = format(period);

    loadData(0);

    // used implicitly
    showButtons; period; moveLeft; moveRight; deleteWidget; createWidget; editWidget; model; periodFriendly;
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
            {#each column.rows as widget}
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
                        {widget.settings.kind}
<!--
                        @{ await Html.RenderPartialAsync(widget.TemplateName, widget); }
    -->
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