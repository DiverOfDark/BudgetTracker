<script>
  import { onMount, onDestroy, getContext } from 'svelte';

  const { assignRoute, unassignRoute, activePath } = getContext('__svero__');

  export let path = '/';
  export let component = undefined;
  export let condition = undefined;
  export let redirect = undefined;

  onMount(() => {
    assignRoute({ path, component, condition, redirect });
  });

  onDestroy(() => {
    console.log(path);
    unassignRoute(path);
  });
</script>

{#if $activePath === path && !component}
  <slot />
{/if}
