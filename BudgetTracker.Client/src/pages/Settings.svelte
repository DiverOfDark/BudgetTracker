<script>
    import {SettingsController, UtilityController } from '../generated-types';
    import Link from '../svero/Link.svelte';

    import { DeleteIcon, XCircleIcon } from 'svelte-feather-icons';

    let newPassword = '';

    let settings = {
        configs:[]
    };

    let reload = async function() {
        let response = await SettingsController.indexJson();
        settings.canDownloadDbDump = response.canDownloadDbDump;
        settings.configs = response.possibleScrapers.map(s=> {
            let found = response.scraperConfigs.find(t=>t.scraperName == s);
            if (found) {
                found.new = false;
                if (found.login == undefined) {
                    found.login = "<не указан>";
                }
                if (found.password == undefined) {
                    found.password = "<не указан>";
                }
                return found;
            }

            return {
                new: true,
                scraperName: s,
                login: '',
                password: '',
                lastSuccessfulBalanceScraping: '',
                lastSuccessfulStatementScraping: '',
            };
        })
    }

    let updatePassword = async function() {
        await SettingsController.updatePassword(newPassword);
    }

    let clearLastSuccessful = async function(id) {
        await SettingsController.clearLastSuccessful(id);
        await reload();
    }

    let addConfig = async function(name, login, password) {
        await SettingsController.addScraper(name, login, password);
        await reload();
    }

    let deleteConfig = async function(id) {
        await SettingsController.deleteConfig(id);
        await reload();
    }

    reload();
</script>

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
                    <div class="form-group">
                        <label class="form-label">
                            Пароль для входа
                        </label>
                        <input type="password" autocomplete="new-password" class="form-control" bind:value="{newPassword}" placeholder="Пароль" />
                    </div>
                    <div class="form-footer">
                        <button on:click="{() => updatePassword()}" class="btn btn-primary btn-block">Обновить</button>
                    </div>
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
                    {#if settings && settings.canDownloadDbDump}
                        <a class="btn btn-pill btn-outline-info btn-sm mb-2" href="{UtilityController.downloadDump}">Скачать дамп базы</a>
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
                    {#each settings.configs as item}
                        <tr>
                            <td>{item.scraperName}</td>
                            {#if !item.new}
                                <td class="text-muted">{item.login}</td>
                                <td class="text-muted">{item.password}</td>
                                <td>
                                    Баланс: <b>{item.lastSuccessfulBalanceScraping}</b><br/>
                                    Выписка: <b>{item.lastSuccessfulStatementScraping}</b>
                                    {#if item.lastSuccessfulBalanceScraping != "-" || item.lastSuccessfulStatementScraping != "-"}
                                        <button on:click="{() => clearLastSuccessful(item.id)}" class="btn btn-anchor btn-link">
                                            <DeleteIcon size="16" />
                                        </button>
                                    {/if}
                                </td>
                                <td>
                                    <button class="btn btn-anchor btn-link" on:click="{() => deleteConfig(item.id)}">
                                        <XCircleIcon size="16" />
                                    </button>
                                </td>
                            {:else}
                                <td><input type="text" bind:value="{item.login}" placeholder="Логин" class="form-control form-control-sm"/></td>
                                <td><input type="text" bind:value="{item.password}" placeholder="Пароль" class="form-control form-control-sm"/></td>
                                <td>&mdash;</td>
                                <td><button type="submit" on:click="{() => addConfig(item.scraperName, item.login, item.password)}" class="form-control btn btn-outline-primary btn-sm">Включить</button></td>
                            {/if}
                        </tr>
                    {/each}
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>