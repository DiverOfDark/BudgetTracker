<svelte:window on:click="{close()}" />
<div on:click="{event => event.stopPropagation()}" class="autocomplete">
  <input
    type="text"
    class="{className}"
    {name}
    {placeholder}
    {required}
    {disabled}
    autocomplete="{name}"
    bind:value="{value}"
    on:input="{onChange}"
    on:keydown="{event => onKeyDown(event)}"
  >
  <ul class:hide-results="{!isOpen}" class="autocomplete-results" bind:this="{list}">
{#each items as result, i}
    <li on:click="{() => close(i)}" class:is-active="{i === arrowCounter}" class="autocomplete-result">
      {result}
    </li>
{/each}
  </ul>
</div>

<script>

let list;

export let name= '';
export let value = '';
export let placeholder= '';
export let required= false;
export let disabled= false;

// autocomplete props
export let items= [];
export let isOpen= false;
export let arrowCounter= 0;

// options
export let className= '';
export let maxItems= 10;

export let onChange = function (event) {
    const height = items.length > maxItems ? maxItems : items.length
    list.style.height = `${height * 2.25}rem`
    isOpen = true;
};

export let onKeyDown = function (event) {
      if (event.keyCode === 40 && arrowCounter < items.length) {
        // ArrowDown
        arrowCounter = arrowCounter + 1;
      } else if (event.keyCode === 38 && arrowCounter > 0) {
        // ArrowUp
        arrowCounter = arrowCounter - 1;
      } else if (event.keyCode === 13) {
        // Enter
        event.preventDefault()
        if (arrowCounter === -1) {
          arrowCounter = 0 // Default select first item of list
        }
        close(arrowCounter)
      } else if (event.keyCode === 27) {
        // Escape
        event.preventDefault()
        close()
      }
    };

export let close = function (index = -1) {
    isOpen = false;
    arrowCounter = -1;
    if (index > -1) {
        value = items[index];
    }
};
</script>

<style>
  * {
    box-sizing: border-box;
  }

  input {
    height: 2rem;
    font-size: 1rem;
    padding: 0.25rem 0.5rem;
  }

  .autocomplete {
    position: relative;
  }

  .hide-results {
    display: none;
  }

  .autocomplete-results {
    padding: 0;
    margin: 0;
    border: 1px solid #dbdbdb;
    height: 6rem;
    overflow: auto;
    width: 100%;

    background-color: white;
    box-shadow: 2px 2px 24px rgba(0, 0, 0, 0.1);
    position: absolute;
    z-index: 100;
  }

  .autocomplete-result {
    color: #7a7a7a;
    list-style: none;
    text-align: left;
    min-height: 2rem;
    padding: 0.25rem 0.5rem;
    cursor: pointer;
  }

  .autocomplete-result > :global(span) {
    background-color: none;
    color: #242424;
    font-weight: bold;
  }

  .autocomplete-result.is-active,
  .autocomplete-result:hover {
    background-color: #dbdbdb;
  }
</style>
