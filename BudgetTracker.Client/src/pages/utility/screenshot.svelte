<svelte:head>
    BudgetTracker - скриншот фоновых задач
</svelte:head>

<script>
    import { UtilityController } from '../../generated-types';
    import { readable } from 'svelte/store';

    const image = readable(UtilityController.screenshotFile, set => {
      const interval = setInterval(() => {
        set(UtilityController.screenshotFile + '?rand=' + Math.random());
      }, 3000);

      return () => clearInterval(interval);
    });

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