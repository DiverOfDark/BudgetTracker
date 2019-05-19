<script>
	import Footer from '../components/Footer.svelte'
	import Nav from '../components/Nav.svelte'
</script>

<div class="page">
	<div class="page-main">
		<Nav />
		<div class="page-content">
			<main>
				<slot></slot>
			</main>
		</div>
	</div>
	<Footer />
</div>