<script>
  import { Router, Route } from 'svero';

  import TableIndex from './pages/table/index.svelte';
  import MetadataIndex from './pages/metadata/index.svelte';
  import MetadataEdit from './pages/metadata/edit.svelte';
  import UtilityTasks from './pages/utility/tasks.svelte';
  import UtilityScriptConsole from './pages/utility/scriptconsole.svelte';
  import UtilityScreenshot from './pages/utility/screenshot.svelte';
  import NotFound from './pages/NotFound.svelte';
  import Footer from './components/Footer.svelte'
  import Nav from './components/Nav.svelte';
  import { tooltip, text, show, position } from './services/Tooltip';

  let selfWidth = 0;
  let selfHeight = 0;

  $: left = $position.left;
  $: topForBottom = $position.top + $position.height + window.scrollY;
  $: topForTop = $position.top + window.scrollY - selfHeight;
  $: showOnBottom = document.body.scrollHeight > topForBottom + selfHeight;
  $: top = showOnBottom ? topForBottom : topForTop;
  $: arrowLeft = Math.min(selfWidth, $position.width) * 0.382;
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
        <Route path="/Utility/Tasks" component={UtilityTasks} />
        <Route path="/Utility/ScriptConsole" component={UtilityScriptConsole} />
        <Route path="/Utility/Screenshot" component={UtilityScreenshot} />
      </Router>
		</div>
	</div>
	<Footer />
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