<svelte:head>
    <title>BudgetTracker - Редактировать категорию</title>
</svelte:head>

<script>
    import { navigateTo } from '../../svero/utils';
    import {PaymentController, SpentCategoryModelController} from '../../generated-types';
    
    export let router;

    let id;
    let kind;
    let pattern;
    let category;

    async function load() {
        let response = await SpentCategoryModelController.find(router.params.id);
        id = response.id;
        pattern = response.pattern;
        category = response.category;
        kind = response.kind;
    }

    async function update() {
        await PaymentController.editCategory(id, pattern, category, kind);
        navigateTo("/Payment/Category");
    }

    load();
</script>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-6">
            <div class="card">
                <div class="card-header">
                    Редактировать категорию
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-12">
                                <div class="form-horizontal">
                                    <div class="form-group">
                                        <label class="control-label">Категория</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" class="form-control" bind:value="{category}" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Тип</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <select bind:value="{kind}" class="form-control form-control-sm">
                                                <option value="Expense">Трата</option>
                                                <option value="Income">Доход</option>
                                                <option value="Transfer">Перевод</option>
                                                <option value="Unknown">Неизвестно</option>
                                            </select>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="control-label">Шаблон</label>
                                        <div control-labelstyle="padding-top: 7px;">
                                            <input type="text" class="form-control" bind:value="{pattern}" />
                                        </div>
                                    </div>
                                    <div class="form-group text-center">
                                        <button class="btn btn-default" on:click="{() => update()}">Обновить</button>
                                    </div>
                                </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>