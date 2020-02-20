<script>
  export let show = false;
  const SLOTS = $$props.$$slots;

  function close() {
      show = false;
  }
</script>

<style>
    .modal {
      overflow-y: auto;
    }

    .modal-filter {
        background: #0000007f;
        width:100%;
        height:100%;
        position: fixed;
    }

    .modal-show {
        display: block;
    }

    .modal-dialog {
        max-width: 700px;
    }
</style>

<div class="modal {show ? 'modal-show' : ''}">
  <div class="modal-filter" on:click="{close}"/>
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content" on:click|preventDefault="{()=>false}">
        {#if SLOTS.title}
        <div class="modal-header">
          <h5 class="modal-title" id="exampleModalLabel">
            <slot name="title"></slot>
          </h5>
          <button type="button" class="close" on:click="{close}">
          </button>
        </div>
        {/if}
        <div class="modal-body">
            <slot></slot>
        </div>
        {#if SLOTS.footer}
            <div class="modal-footer">
                <slot name="footer"></slot>
            </div>
        {/if}
    </div>
  </div>
</div>