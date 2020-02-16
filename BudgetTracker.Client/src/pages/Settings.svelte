<script lang="ts">
    import { writable } from 'svelte/store';
    import { onDestroy } from 'svelte';
    import SoWService from '../services/SoWService';
    import protoSettings from '../generated/Settings_pb';
    import Link from '../svero/Link.svelte';

    let settings = writable(new protoSettings.Settings().toObject());

    let newPassword = '';
    let downloadInProgress = false;

    async function updateSettingsPassword() {
        await SoWService.updateSettingsPassword(newPassword);
        newPassword = '';
    }

    async function downloadDump() {
        downloadInProgress = true;
        let response = await SoWService.downloadDump();
        let blob = new File([response.buffer], "database.litedb", {type: 'application/octet-stream'});
        let href = URL.createObjectURL(blob);

        let link = document.createElement('a');
        link.href = href;
        link.download = "database.litedb";
        document.body.appendChild(link);
        link.dispatchEvent(new MouseEvent('click', {bubbles: true, cancelable: true, view: window}));
        link.remove();
        URL.revokeObjectURL(href);
        downloadInProgress = false;
    }
    
    let unsubscribe = SoWService.getSettings(x => settings.set(x));

    onDestroy(() => unsubscribe());

    Link; downloadDump; updateSettingsPassword; settings; downloadInProgress;
</script>

<style type="text/css">
    @keyframes spinner-border {
        to { transform: rotate(360deg); }
    }

    .spinner-border {
        display: inline-block;
        width: 24px;
        height: 24px;
        vertical-align: text-bottom;
        border: 2px solid currentColor;
        border-right-color: transparent;
        border-radius: 50%;
        animation: spinner-border .75s linear infinite;
    }
</style>

<svelte:head>
    <title>BudgetTracker - Настройки</title>
</svelte:head>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    Общие настройки
                </div>
                <div class="card-body">
                    <form on:submit|preventDefault="{() => updateSettingsPassword()}">
                        <div class="form-group">
                            <label class="form-label">
                                Пароль для входа
                            </label>
                            <input type="password" autocomplete="new-password" class="form-control" bind:value="{newPassword}" placeholder="Пароль" />
                        </div>
                        <div class="form-footer">
                            <input type="submit" value="Обновить" class="btn btn-primary btn-block" />
                        </div>
                    </form>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    Утилиты
                </div>
                <div class="card-body">
                    <Link class="btn btn-pill btn-outline-info btn-sm mb-2" href="/Utility/Tasks">Фоновые&nbsp;задачи</Link>
                    <br/>
                    {#if $settings.canDownloadDbDump}
                        <input type="button" class="btn btn-pill btn-outline-info btn-sm mb-2" on:click="{() => downloadDump()}" value="Скачать дамп базы" disabled={downloadInProgress} />
                        {#if downloadInProgress}
                            <div class="spinner-border text-primary" role="status">
                                <span class="sr-only">Loading...</span>
                            </div>
                        {/if}
                        <br/>
                    {/if}
                    <Link class="btn btn-pill btn-outline-info btn-sm mb-2" href="/Utility/Screenshot">Скриншот&nbsp;браузера</Link>
                    <br/>
                    <Link class="btn btn-pill btn-outline-info btn-sm mb-2" href="/Utility/ScriptConsole">Командная&nbsp;консоль</Link>
                </div>
            </div>
        </div>

        
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    Источники данных
                </div>
                <table class="table card-table table-vcenter text-nowrap">
                    <thead>
                    <tr>
                        <th class="w-1">Тип</th>
                        <th>Логин</th>
                        <th>Пароль</th>
                        <th>Последний успешный сбор данных</th>
                        <th class="w-1"></th>
                    </tr>
                    </thead>
                    <tbody>
                    {#each $settings.scraperConfigsList as item}
                        <tr>
                            <td>{item.scraperName}</td>
                            {#if item.enabled}
                                <td class="text-muted">{item.login}</td>
                                <td class="text-muted">{item.password}</td>
                                <td>
                                    Баланс: <b>{item.lastSuccessfulBalanceScraping}</b><br/>
                                    Выписка: <b>{item.lastSuccessfulStatementScraping}</b>
                                    {#if item.lastSuccessfulBalanceScraping != "-" || item.lastSuccessfulStatementScraping != "-"}
                                        <button on:click="{() => SoWService.clearLastSuccessful(item.id)}" class="btn btn-anchor btn-link"><span class="fe fe-delete"></span></button>
                                    {/if}
                                </td>
                                <td>
                                    <button class="btn btn-anchor btn-link" on:click="{() => SoWService.deleteConfig(item.id)}">
                                        <span class="fe fe-x-circle"></span>
                                    </button>
                                </td>
                            {:else}
                                <td><input type="text" bind:value="{item.login}" placeholder="Логин" class="form-control form-control-sm"/></td>
                                <td><input type="text" bind:value="{item.password}" placeholder="Пароль" class="form-control form-control-sm"/></td>
                                <td>&mdash;</td>
                                <td><button type="submit" on:click="{() => SoWService.addConfig(item.scraperName, item.login, item.password)}" class="form-control btn btn-secondary btn-sm">Включить</button></td>
                            {/if}
                        </tr>
                    {/each}
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>