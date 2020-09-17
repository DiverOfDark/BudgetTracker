<svelte:head>
    <title>BudgetTracker - Категории расходов</title>
</svelte:head>
<script lang="ts">
    import Form from './editCategory.svelte';
    import Modal from '../../components/Modal.svelte';
    import SoWService from '../../services/SoWService';
    import { formatPaymentKind } from '../../services/Shared';
    import * as protos from '../../generated/SpentCategories_pb';
    import { onDestroy } from 'svelte';
    import { EditIcon, XCircleIcon } from 'svelte-feather-icons';

    let spentCategories = SoWService.getSpentCategories(onDestroy).spentCategories;

    let newCategory = "";
    let newPattern = "";
    let newKind = 0;
    let showEdit = false;
    let editSpentCategoryModel: protos.SpentCategory.AsObject | undefined = undefined;

    let edit = function(category: protos.SpentCategory.AsObject) {
        editSpentCategoryModel = category;
        showEdit = true;
    }

    let deleteCategory = async function(category: protos.SpentCategory.AsObject) {
        SoWService.deleteSpentCategory(category.id!);
    }

    let create = async function() {
        SoWService.createSpentCategory(newCategory, newPattern, newKind);
        newCategory = "";
        newPattern = "";
        newKind = 0;
    }
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
                                {#each $spentCategories as category, idx}
                                <tr>
                                    <td>{category.category}</td>
                                    <td>{formatPaymentKind(category.kind)}</td>
                                    <td>{category.pattern || ''}</td>
                                    <td>
                                        <button class="btn btn-link btn-anchor" on:click="{() => edit(category)}">
                                            <EditIcon size="16" />
                                        </button>
                                        <button class="btn btn-link btn-anchor" on:click="{() => deleteCategory(category)}">
                                            <XCircleIcon size="16" />
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

{#if showEdit}
<Modal bind:show="{showEdit}">
    <div slot="title">
        Редактировать категорию
    </div>
    <Form model="{editSpentCategoryModel}" on:close="{() => showEdit = false}" />
</Modal>
{/if}