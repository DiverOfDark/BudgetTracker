<svelte:head>
    <title>BudgetTracker - Категории расходов</title>
</svelte:head>
<script lang="ts">
    import Form from './editCategory.svelte';
    import Modal from '../../components/Modal.svelte';
    import SoWService from '../../services/SoWService';
    import {compare} from '../../services/Shared';
    import * as protos from '../../generated/SpentCategories_pb';
    import { writable, get } from 'svelte/store';
    import { onDestroy } from 'svelte';

    let spentCategories = writable<protos.SpentCategory.AsObject[]>([]);

    let newCategory = "";
    let newPattern = "";
    let newKind = 0;
    let showEdit = false;
    let editSpentCategoryModel: protos.SpentCategory.AsObject | undefined = undefined;

    function getDisplayName(kind: any) {
        if (kind == 0)
            return "Трата";
        if (kind == 1)
            return "Доход";
        if (kind == 2)
            return "Перевод";
        return "Неизвестно";
    }

    function categorySort(a: protos.SpentCategory.AsObject, b: protos.SpentCategory.AsObject): number {
        return compare(a.category, b.category) * 10 + compare(a.pattern, b.pattern);
    }

    function parseSpentCategories(stream: protos.SpentCategoriesStream.AsObject) {
        if (stream.added) {
            let newCategories = get(spentCategories);
            newCategories = [...newCategories, stream.added];
            newCategories.sort(categorySort);
            spentCategories.set(newCategories);
        } else if (stream.removed) {
            let newCategories = get(spentCategories);
            newCategories = newCategories.filter((f: protos.SpentCategory.AsObject) => f.id!.value != stream.removed!.id!.value);
            newCategories.sort(categorySort);
            spentCategories.set(newCategories);
        } else if (stream.updated) {
            let newCategories = get(spentCategories);
            newCategories = newCategories.map((f: protos.SpentCategory.AsObject) => {
                if (f.id!.value == stream.updated!.id!.value) {
                    return stream!.updated;
                }
                return f;
            });
            newCategories.sort(categorySort);
            spentCategories.set(newCategories);
        } else if (stream.snapshot) {
            let newCategories = stream.snapshot.spentCategoriesList;
            newCategories.sort(categorySort);
            spentCategories.set(newCategories);
        } else {
            console.error("Unsupported operation");
        }
    }

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

    let unsubscribe = SoWService.getSpentCategories(parseSpentCategories);

    onDestroy(unsubscribe);

    // used in view
    newCategory; newPattern; newKind; deleteCategory; edit; create; getDisplayName; showEdit; Modal; Form; editSpentCategoryModel;
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
                                    <td>{getDisplayName(category.kind)}</td>
                                    <td>{category.pattern || ''}</td>
                                    <td>
                                        <button class="btn btn-link btn-anchor" on:click="{() => edit(category)}">
                                            <span class="fe fe-edit"></span>
                                        </button>
                                        <button class="btn btn-link btn-anchor" on:click="{() => deleteCategory(category)}">
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

{#if showEdit}
<Modal bind:show="{showEdit}">
    <div slot="title">
        Редактировать категорию
    </div>
    <Form model="{editSpentCategoryModel}" on:close="{() => showEdit = false}" />
</Modal>
{/if}