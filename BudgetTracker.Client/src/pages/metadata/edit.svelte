<script>
    import {MetadataController} from '../../generated-types';
    import AutoComplete from '../../components/Autocomplete.svelte'
    import { navigateTo } from '../../svero/utils';

    export let router = {};
    
    MetadataController.indexJson().then(cols => {
        knownColumns = cols.map(s=>s.isComputed ? s.userFriendlyName : s.provider + "/" + s.accountName);
        if (router.params && router.params.id) {
            let actualCol = cols.find(s=>s.id == router.params.id);
            if (actualCol) {
                id = actualCol.id;
                isComputed = actualCol.isComputed;
                provider = actualCol.provider;
                accountName = actualCol.accountName;
                function2 = actualCol.function;
                userFriendlyName = actualCol.userFriendlyName;
                autogenerateStatements = actualCol.autogenerateStatements;
            }
        };
    });

    let knownColumns = [];
    let id = "";
    let isComputed = true;
    let provider;
    let accountName;
    let function2 = "";
    let userFriendlyName = "";
    let autogenerateStatements = false;

    let submit = async function() {
        await MetadataController.metadataEdit(id, userFriendlyName, function2, autogenerateStatements);
        navigateTo("/Metadata");
    }

    let getAutocompleteFor = function(input) {
        let lastIndex = input.lastIndexOf(']') + 1;
        lastIndex = input.indexOf('[', lastIndex);

        if (lastIndex == -1)
            return [];

        var searchPart = input.substring(lastIndex + 1);
        var possibleItems = knownColumns.map(s=>"[" + s + "]");
        var matched = possibleItems.filter(t=>t.indexOf(searchPart) != -1 && input.indexOf(t) == -1);

        return matched.map(v => input.substring(0, lastIndex) + v).sort();
    }

    $: autocompleteVariants = getAutocompleteFor(function2);
</script>

<svelte:head>
    <title>BudgetTracker - Редактировать столбец</title>
</svelte:head>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    Редактировать столбец
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                                <div class="form-horizontal">
                                    {#if !isComputed}
                                        <div class="form-group">
                                            <label class="control-label">Поставщик</label>
                                            <div control-labelstyle="padding-top: 7px;">
                                                {provider}
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label">Название аккаунта</label>
                                            <div control-labelstyle="padding-top: 7px;">
                                                {accountName}
                                            </div>
                                        </div>
                                    {:else}
                                        <div class="form-group">
                                            <label class="control-label">Функция</label>
                                            <div control-labelstyle="padding-top: 7px;">
                                                <AutoComplete className="form-control" items="{autocompleteVariants}" bind:value="{function2}" />
                                            </div>
                                        </div>
                                    {/if}

                                    <div class="form-group">
                                        <label class="control-label">Название</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" class="form-control" bind:value="{userFriendlyName}" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Автоматически создавать ДДС из баланса</label>
                                        <div class="col-1" control-labelstyle="padding-top: 7px;">
                                            <input type="checkbox" class="form-control" bind:checked="{autogenerateStatements}" />
                                        </div>
                                    </div>
                                    <div class="form-group text-center">
                                        <button class="btn btn-primary btn-default form-control" on:click="{() => submit()}" >{!id ? "Создать" : "Обновить"}</button>
                                    </div>
                                </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>