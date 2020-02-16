<script lang="ts">
    import SoWService from '../../services/SoWService';

    let command = "";
    let result = "";
    let exception = "";

    let executeScript = async function() {
        let response = await SoWService.executeScript(command);
        result = response.result;
        exception = response.exception;
    }

    let handleKeydown = function(event: KeyboardEvent) {
        if (event.key == "Enter" && event.ctrlKey) {
            executeScript();
        }
    }

    executeScript; command; result; exception; handleKeydown;
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

<svelte:window on:keydown={handleKeydown}/>

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
                            <form on:submit|preventDefault="{() => executeScript()}">
                                <textarea name="script" rows="10" cols="80" class="form-control" bind:value="{command}"></textarea>
                                <br/>
                                <input type="submit" class="btn btn-primary" value="Выполнить" />
                            </form>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    {#if exception}
                        <pre class="text-danger">{exception}</pre>
                    {:else}
                        <pre>{@html result}</pre>
                    {/if}
                </div>
            </div>
        </div>
    </div>
</div>