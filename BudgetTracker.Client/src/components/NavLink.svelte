<script lang="ts">
    import Link from '../svero/Link.svelte';

    import { onMount, onDestroy } from 'svelte';

    export let href : string;
    export let title : string;

    export let active : string;

    let listener = function() {
        active = window.location.pathname.indexOf(href) == 0 ? "active" : "";;
    };

    onMount(() => window.addEventListener('popstate', listener));
    onDestroy(() => window.removeEventListener('popstate', listener));

    listener();
</script>

<Link className="nav-link {active}" {href}>{title}</Link>