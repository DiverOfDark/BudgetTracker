<svelte:head>
    <title>BudgetTracker - Категории расходов</title>
</svelte:head>

<script>
    import {SpentCategoryModelController, PaymentController } from '../../generated-types';
    import {compare} from '../../services/Shared';
    import { navigateTo } from '../../svero/utils';

    let categories = [];

    let newCategory = "";
    let newPattern = "";
    let newKind = "";

    async function load() {
        categories = await SpentCategoryModelController.list();
        categories.sort((a,b) => compare(a.category, b.category) * 10 + compare(a.pattern, b.pattern));
    }

    let edit = function(id) {
        navigateTo("/Payment/Category/Edit/" + id);
    }

    let deleteCategory = async function(id) {
        await SpentCategoryModelController.delete(id);
        await load();
    }

    let create = async function() {
        await PaymentController.createCategory(newPattern, newCategory, newKind);
        await load();
        newCategory = "";
        newPattern = "";
        newKind = "";
    }

    load();
</script>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                    <div class="card-header">
                        <h3 class="card-title">
                            Категории расходов
                        </h3>
                    </div>
                    <div class="table-responsive">
                        <table class="table card-table table-sm table-hover table-striped">
                            <thead>
                                <tr>
                                    <th>Категория</th>
                                    <th>Тип</th>
                                    <th>Шаблон</th>
                                    <th>Удалить</th>
                                </tr>
                            </thead>
                            <tbody>
                                {#each categories as category}
                                <tr>
                                    <td>{category.category}</td>
                                    <td>{category.kind}</td>
                                    <td>{category.pattern || ''}</td>
                                    <td>
                                        <button class="btn btn-link btn-anchor" on:click="{() => edit(category.id)}">
                                            <span class="fe fe-edit"></span>
                                        </button>
                                        <button class="btn btn-link btn-anchor" on:click="{() => deleteCategory(category.id)}">
                                            <span class="fe fe-x-circle"></span>
                                        </button>
                                    </td>
                                </tr>
                                {/each}

                                <tr>
                                    <td>
                                        <input type="text" bind:value="{newCategory}" class="form-control form-control-sm"/>
                                    </td>
                                    <td>
                                        <select bind:value="{newKind}" class="form-control form-control-sm">
                                            <option value="0">Трата</option>
                                            <option value="1">Доход</option>
                                            <option value="2">Перевод</option>
                                            <option value="-1">Неизвестно</option>
                                        </select>
                                    </td>
                                    <td>
                                        <input type="text" bind:value="{newPattern}" class="form-control form-control-sm"/>
                                    </td>
                                    <td>
                                        <button class="btn btn-sm btn-azure" on:click="{() => create()}">Добавить</button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
        </div>
    </div>
</div>