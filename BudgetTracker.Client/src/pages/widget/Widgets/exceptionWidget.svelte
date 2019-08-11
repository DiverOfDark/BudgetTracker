<script>
    export let model;
</script>

<div class="card-status bg-red"></div>
<div class="card-body bg-red-lighter" style="height:168px">
    {#if (model.detailed != null)}
        <div class="m-0" title="{model.detailed}">
            <b>Ошибка:</b>
            <br/>
            <i>{model.message}</i>
        </div>
    {/if}
</div>