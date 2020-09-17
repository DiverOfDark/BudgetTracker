<svelte:head>
    BudgetTracker - скриншот фоновых задач
</svelte:head>

<script lang="ts">
    import { writable } from 'svelte/store';
    import { onDestroy } from 'svelte';
    import SoWService from '../../services/SoWService';

    const image = writable("");

    let cancelSubscription = SoWService.getScreenshot(x => {
        if (x)
            image.set("data:image/png;base64," + x)
        else
            image.set("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");
    });

    onDestroy(cancelSubscription);
</script>

<style>
    img {
        width: 100%;
    }
</style>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">
                        Chrome
                    </h3>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                            <img id="screenshot" src="{$image}" alt="screenshot" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>