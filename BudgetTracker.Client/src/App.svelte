<script lang="ts">
  //@ts-ignore
  import { Router, Route } from 'svero';

  import TableIndex from './pages/table/index.svelte';
  import MetadataIndex from './pages/metadata/index.svelte';
  import NotFound from './pages/NotFound.svelte';
  import Footer from './components/Footer.svelte'
  import Nav from './components/Nav.svelte';
  import Tooltip from './components/Tooltip.svelte';

  // used in templating:
  Router; Route; TableIndex; MetadataIndex; NotFound; Footer; Nav; Tooltip; 
</script>

<div class="page">
	<div class="page-main">
		<Nav />
		<div class="page-content">
      <Router>
        <Route path="*" component={NotFound} />
        <Route path="/Table" component={TableIndex} />
        <Route path="/Metadata" component={MetadataIndex} />
      </Router>
		</div>
	</div>
	<Footer />
  <Tooltip />
</div>