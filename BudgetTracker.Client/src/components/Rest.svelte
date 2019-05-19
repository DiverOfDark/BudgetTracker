<script context="module">
    import fetch from 'unfetch';

	export async function systemInfo() { 
        let fetched = await fetch(`/System/SiteInfo`);
        let json = await fetched.json();

        return json;
	}
</script>