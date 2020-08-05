 <script lang="ts">
    import { navigateTo } from '../../svero/utils';
    import * as interfaces from '../../generated-types';

    export let router: any = {};

    let model: interfaces.WidgetJsViewModel = {
        kind: '',
        kindId: 0,
        title: '',
        id: '',
        properties: {
            providerName: '',
            accountName: '',
            stringFormat: '',
            currency: ''
        }
    };

    let title = 'Создать виджет';

    let loadData = async function(id: string) {
        model = await interfaces.WidgetViewModelController.find(id);
        invalidateAccount(model.properties.providerName, model.properties.accountName);

        if (!model.properties.stringFormat) {
            model.properties.stringFormat = '';
        }

        if (!model.properties.currency) {
            model.properties.currency = '';
        }
    }

    let providers : string[] = [];

    let accounts: string[] = [];

    let accountsMap: Map<string, string[]> = new Map();

    let loadStatics = async function() {
        let mcms = await interfaces.MoneyColumnMetadataModelController.list();
        providers = [...new Set(mcms.map(f=>f.provider))].sort();
        
        for(var prId = 0; prId < providers.length; prId++) {
            accountsMap.set(providers[prId], [...new Set(mcms.filter(s=>s.provider == providers[prId]).map(s=>s.userFriendlyName || s.accountName))].sort());
        }

        invalidateAccount(model.properties.accountName, model.properties.providerName);
    }

    let invalidateAccount = function(provider: string, account: string) {
        if (!model.properties.providerName || !providers) {
            return
        }

        let result = accountsMap.get(provider) || [];
        if (result && result.indexOf(account) == -1) {
            model.properties.accountName = result[0];
        }
        accounts = result;
    }

    $: {
        if (router && router.params) {
            if (router.params.id) {
                title = 'Редактировать виджет';

                loadData(router.params.id);
            }
        }
    }

    let save = async function() {
        await interfaces.WidgetController.editWidget(model.id, model.title, interfaces.WidgetKindEnum.getEnums()[model.kindId], model.properties);
        navigateTo('/');
    }

    $: { invalidateAccount(model.properties.providerName, model.properties.accountName); }

    loadStatics();

    let showAccount = false;

    $: {
        showAccount = model.kindId == interfaces.WidgetKindEnum.LastValue.getId()
            || model.kindId == interfaces.WidgetKindEnum.LinearChart.getId()
            || model.kindId == interfaces.WidgetKindEnum.Donut.getId()
            || model.kindId == interfaces.WidgetKindEnum.Delta.getId();
    }

    // used in template
    model; title; providers; accounts; showAccount; save;
</script>
<svelte:head>
    <title>BudgetTracker - {title}</title>
</svelte:head>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    {title}
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                            <div class="form-horizontal">
                                <div class="form-group">
                                    <label class="control-label">Название</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <input type="text" bind:value="{model.title}" class="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Тип виджета</label>
                                    <div control-labelstyle="padding-top: 7px;">
                                        <select class="form-control" bind:value="{model.kindId}">
                                            {#each interfaces.WidgetKindEnum.getEnums() as wk}
                                                <option value="{wk.getId()}">{wk.getLabel()}</option>
                                            {/each}
                                        </select>
                                    </div>
                                </div>

                                {#if showAccount}
                                    <div class="form-group">
                                        <label class="control-label">Провайдер данных</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select bind:value="{model.properties.providerName}" class="form-control">
                                                {#each providers as pr}
                                                    <option value="{pr}">{pr}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Аккаунт</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select bind:value="{model.properties.accountName}" class="form-control">
                                                {#each accounts as account} 
                                                    <option value="{account}">{account}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                {/if}

                                {#if (model.kindId == interfaces.WidgetKindEnum.LastValue.getId() )}
                                    <div class="form-group">
                                        <label class="control-label">Тип графика</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select bind:value="{model.properties.graphKind}" class="form-control">
                                                {#each interfaces.GraphKindEnum.getEnums() as gke}
                                                    <option value="{gke.getName()}">{gke.getLabel()}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Формат строки</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" bind:value="{model.properties.stringFormat}" class="form-control" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Менять цвет на черный, если последнее обновление было более чем 36ч назад</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="checkbox" bind:checked="{model.properties.notifyStaleData}" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Исключать из графика переводы</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="checkbox" bind:checked="{model.properties.exemptTransfers}" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Компактный вид</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="checkbox" bind:checked="{model.properties.compact}" />
                                        </div>
                                    </div>
                                {:else if model.kindId == interfaces.WidgetKindEnum.Expenses.getId()}
                                    <div class="form-group">
                                        <label class="control-label">Валюта по которой показывать траты:</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" bind:value="{model.properties.currency}" class="form-control" />
                                        </div>
                                    </div>
                                {:else if model.kindId == interfaces.WidgetKindEnum.LinearChart.getId()} 
                                    <!-- No properties for this -->
                                {:else if model.kindId == interfaces.WidgetKindEnum.Donut.getId()} 
                                    <!-- No properties for this -->
                                {:else if model.kindId == interfaces.WidgetKindEnum.Delta.getId()} 
                                    <div class="form-group">
                                        <label class="control-label">Периодичность</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select bind:value={model.properties.deltaInterval} class="form-control">
                                                {#each interfaces.DeltaIntervalEnum.getEnums() as delta}
                                                    <option value="{delta.getName()}">{delta.getLabel()}</option>
                                                {/each}
                                            </select>
                                        </div>
                                    </div>
                                {:else}
                                    <div class="well">
                                        Неподдерживаемый тип виджета
                                    </div>
                                {/if}
                                <div class="form-group text-center">
                                    <button on:click="{() => save()}" class="btn btn-default">{title}</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>