<script>
    import { tooltip } from '../services/Tooltip';
    import SowService from '../services/SoWService';

    let info = SowService.SystemInfo;
</script>

<footer class="footer">
    <div class="container">
        <div class="row align-items-center flex-row-reverse">
            {#if $info}
            <div class="col-auto ml-auto" use:tooltip="{$info.stats}">
                <p>
                    {$info.currentVersion}
                    
                    {#if info.hasNewerVersion}
                        <a href="https://github.com/DiverOfDark/BudgetTracker">
                            (there is an update!)
                        </a>
                    {/if}
                    @@ 
                    {$info.launchTime}
                </p>
            </div>
            {/if}
            <div class="col-12 col-lg-auto mt-3 mt-lg-0 text-center">
                <p>&copy; 2019-2020 &mdash; BudgetTracker</p>
            </div>
        </div>
    </div>
</footer>
