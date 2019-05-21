<script>
  import { Router, Route } from 'svero';

  import BTTable from './pages/BTTable.svelte';
  import NotFound from './pages/NotFound.svelte';
  import Footer from './components/Footer.svelte'
  import Nav from './components/Nav.svelte'
</script>

<div class="page">
	<div class="page-main">
		<Nav />
		<div class="page-content">
        <Router>
            <Route path="*" component={NotFound} />
            <Route path="/Table" component={BTTable} />
        </Router>
		</div>
	</div>
	<Footer />
</div>