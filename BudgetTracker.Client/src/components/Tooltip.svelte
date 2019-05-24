<script lang="ts">
    export let text = "";
    export let show = false;
    export let position = {left: 0, top: 0, width: 0, height: 0}

    export let self = 0;
</script>

<style>
    .tooltip {
        position: absolute; 
        transform: translate3d(px, 295px, 0px); 
        top: 0px; 
        left: 0px; 
        will-change: transform;
    }
</style>

{#if show}
    <div class="tooltip fade show bs-tooltip-bottom" 
    style="transform: translate3d({position.left}px, {position.top + position.height}px, 0px)"
    role="tooltip" bind:clientWidth={self}
    x-placement="bottom">
        <div class="arrow" style="left: {Math.min(self, position.width) * 0.382}px;"></div>
        <div class="tooltip-inner">
            {text}
        </div>
    </div>
{/if}