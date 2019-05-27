<script>
  import { Router, Route } from 'svero';

  import TableIndex from './pages/table/index.svelte';
  import MetadataIndex from './pages/metadata/index.svelte';
  import MetadataEdit from './pages/metadata/edit.svelte';
  import NotFound from './pages/NotFound.svelte';
  import Footer from './components/Footer.svelte'
  import Nav from './components/Nav.svelte';
  import { tooltip, text, show, position } from './services/Tooltip';

  let self = 0;
  let self2 = 0;

  $: left = $position.left;
  $: topForBottom = $position.top + $position.height + window.scrollY;
  $: topForTop = $position.top + window.scrollY - self2;
  $: showOnBottom = document.body.scrollHeight && document.body.scrollHeight < topForBottom;
  $: top = showOnBottom ? topForBottom : topForTop;
  $: arrowLeft = Math.min(self, $position.width) * 0.382;
</script>

<div class="page">
	<div class="page-main">
		<Nav />
		<div class="page-content">
      <Router>
        <Route path="*" component={NotFound} />
        <Route path="/Table" component={TableIndex} />
        <Route path="/Metadata" component={MetadataIndex} />
        <Route path="/Metadata/Edit" component={MetadataEdit} />
        <Route path="/Metadata/Edit/:id" component={MetadataEdit} />
      </Router>
		</div>
	</div>
	<Footer />
{#if $show}
    <div class="tooltip fade show bs-tooltip-{showOnBottom ? 'bottom' : 'top'}" 
    style="transform: translate3d({left}px, {top}px, 0px)"
    role="tooltip" bind:clientWidth={self}
    x-placement="bottom" bind:clientHeight={self2}>
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
</div>

<style>
    .tooltip {
        position: absolute; 
        transform: translate3d(0px, 295px, 0px); 
        top: 0px; 
        left: 0px; 
        will-change: transform;
    }
</style>