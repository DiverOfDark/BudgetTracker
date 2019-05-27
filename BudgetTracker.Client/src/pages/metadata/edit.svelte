<script lang="ts">
    import {MetadataController} from '../../generated-types';
    import { afterUpdate } from 'svelte';
  
    //@ts-ignore
    import {navigateTo} from 'svero';

    export let router:any = {};
    
    afterUpdate(() => {
        if (router.params && router.params['id']) {
            MetadataController.indexJson().then(cols => {
                cols;
                debugger;
            })
        }
    })

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

    // used in view:
    id; isComputed; provider; accountName; function2; userFriendlyName; autogenerateStatements; submit;
</script>

<svelte:head>
    <title>BudgetTracker - Редактировать столбец</title>
</svelte:head>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-6">
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
                                                <input type="text" class="form-control" bind:value="{function2}" />
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
                                        <input value="Обновить" class="btn btn-default" on:click="{() => submit()}" />
                                    </div>
                                </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!--
@section Scripts{
    <link rel="stylesheet" href="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/themes/smoothness/jquery-ui.css">
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#Function").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "ComputedAutocomplete",
                        contentType: 'application/json',
                        data: JSON.stringify(request),
                        type: "POST",
                        processData: false,
                        success: function (data) {
                            response(data);
                        },
                        error: function (XMLHttpRequest, textStatus) {
                            alert(textStatus);
                        }
                    });
                },
                minLength: 1
            });
        });
    </script>
}

-->