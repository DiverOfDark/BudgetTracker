<script>
  import { tooltip, text, show, position } from '../services/Tooltip';

  let selfWidth = 0;
  let selfHeight = 0;

  $: left = $position.left;
  $: topForBottom = $position.top + $position.height + window.scrollY;
  $: topForTop = $position.top + window.scrollY - selfHeight;
  $: showOnBottom = document.body.scrollHeight > topForBottom + selfHeight;
  $: top = showOnBottom ? topForBottom : topForTop;
  $: arrowLeft = Math.min(selfWidth, $position.width) * 0.382;
</script>

<style>
    .tooltip {
        position: absolute; 
        transform: translate3d(0px, 295px, 0px); 
        top: 0px; 
        left: 0px; 
        will-change: transform;
    }
</style>

{#if $show}
    <div class="tooltip fade show bs-tooltip-{showOnBottom ? 'bottom' : 'top'}" 
        style="transform: translate3d({left}px, {top}px, 0px)"
        role="tooltip" bind:clientWidth={selfWidth}
        x-placement="bottom" bind:clientHeight={selfHeight}>
    {#if showOnBottom}
        <div class="arrow" style="left: {arrowLeft}px;"></div>
    {/if}
        <div class="tooltip-inner">
            {$text}
        </div>
    {#if !showOnBottom}
        <div class="arrow" style="left: {arrowLeft}px;"></div>
    {/if}
    </div>
{/if}
