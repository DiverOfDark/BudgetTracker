<script>
    import Link from '../svero/Link.svelte';

    import { writable, get } from 'svelte/store';
    import { onMount, onDestroy } from 'svelte';

    export let href;
    export let title;

    export let active;

    let listener = function() {
        active = window.location.pathname.indexOf(href) == 0 ? "active" : "";;
    };

    onMount(() => window.addEventListener('popstate', listener));
    onDestroy(() => window.removeEventListener('popstate', listener));

    listener();
</script>

<Link className="nav-link {active}" {href}>{title}</Link>