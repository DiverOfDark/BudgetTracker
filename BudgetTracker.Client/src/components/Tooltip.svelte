<script context="module">
    import { writable } from 'svelte/store';

    export let text = writable("");
    export let show = writable(false);
    export let position = writable({left: 0, top: 0, width: 0, height: 0})

    export function doShowTooltip(node, newText) {
	  let mouseover = () => {
	    position.set(node.getBoundingClientRect());
	    text.set(newText);
	    show.set(true);
	  }
      
      let mouseout = () => show.set(false);

	  node.addEventListener('mouseover', mouseover);
      node.addEventListener('mouseout', mouseout);

	  return {
	    destroy() {
          node.removeEventListener('mouseover', mouseover);
          node.removeEventListener('mouseout', mouseout);
	    }
	  };
	}
</script>
<script>
    let self = 0;

    $: left = $position.left;
    $: top = $position.top + $position.height;
    $: arrowLeft = Math.min(self, $position.width) * 0.382;
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

{#if $show}
    <div class="tooltip fade show bs-tooltip-bottom" 
    style="transform: translate3d({left}px, {top}px, 0px)"
    role="tooltip" bind:clientWidth={self}
    x-placement="bottom">
        <div class="arrow" style="left: {arrowLeft}px;"></div>
        <div class="tooltip-inner">
            {$text}
        </div>
    </div>
{/if}