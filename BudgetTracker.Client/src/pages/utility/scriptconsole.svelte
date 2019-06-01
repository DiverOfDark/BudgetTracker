<script>
    let command = "";
    let result = "";

    import { UtilityController } from '../../generated-types';

    let executeScript = async function() {
        result = await UtilityController.scriptConsole(command);
    }
</script>

<svelte:head>
    <title>BudgetTracker - Script Console</title>
</svelte:head>

<style type="text/css" media="screen">
    textarea {
        top: 0;
        right: 0;
        bottom: 0;
        left: 0;
        height: 200px;
        margin: -12px -6px 6px -6px;
        background-color: black;
        color: darkgray;
        font-family: monospace;
    }
</style>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">
                        Script Console
                    </h3>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                            <textarea name="script" rows="10" cols="80" class="form-control" bind:value="{command}"></textarea>
                            <br/>
                            <button on:click="{() => executeScript()}" class="btn btn-primary">Выполнить</button>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    {@html result}
                </div>
            </div>
        </div>
    </div>
</div>