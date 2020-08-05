<script lang="ts">
    //@ts-ignore
    import { ArrowLeftIcon } from 'svelte-feather-icons';
</script>

<svelte:head>
    <title>BudgetTracker - Ошибка</title>
</svelte:head>

<div class="container text-center">
    <div class="display-1 text-muted mb-5"><i class="si si-exclamation"></i> 404</div>
    <h1 class="h2 mb-3">Упс.. Что-то пошло не так..</h1>
    <p class="h4 text-muted font-weight-normal mb-7">Сорьки&hellip;</p>
    <a class="btn btn-primary" href="javascript:history.back()">
        <ArrowLeftIcon size="16" customClass="mr-2" />Назад
    </a>
</div>