<script>
    import {systemInfo} from './Rest.svelte'

    export let footer;
    export let info;

    systemInfo().then(i => info = i);;
</script>

<footer class="footer">
    <div class="container">
        <div class="row align-items-center flex-row-reverse">
            {#if info}
            <div class="col-auto ml-auto" data-toggle="top-tooltip" title="{info.stats}">
                <p>
                    {info.currentVersion}
                    
                    {#if info.hasNewerVersion}
                        latest: 
                        <a href="{info.latestVersionUrl}">
                            {info.latestVersion}
                        </a>
                    {/if}
                    @@ 
                    {info.launchTime}
                </p>
            </div>
            {/if}
            <div class="col-12 col-lg-auto mt-3 mt-lg-0 text-center">
                <p>&copy; 2019 - BudgetTracker</p>
            </div>
        </div>
    </div>
</footer>
