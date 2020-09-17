<script lang="ts">
	import { createEventDispatcher } from 'svelte';
    import SoWService from '../../services/SoWService';
	import { onMount } from 'svelte';

    import spentCategoryProtos from '../../generated/SpentCategories_pb';

    export let model = new spentCategoryProtos.SpentCategory().toObject();

    let kind: number = 0;

    onMount(() => { kind = model.kind });

	const dispatch = createEventDispatcher();

    let submit = async function() {
        model.kind = kind;
        SoWService.updateSpentCategory(model);
        dispatch('close');
    }
</script>

<div class="form-horizontal">
    <div class="form-group">
        <label class="control-label">Категория</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" class="form-control" bind:value="{model.category}" />
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Тип</label>
        <div control-labelstyle="padding-top: 7px;">
            <select bind:value="{kind}" class="form-control form-control-sm">
                <option value="0">Трата</option>
                <option value="1">Доход</option>
                <option value="2">Перевод</option>
                <option value="-1">Неизвестно</option>
            </select>
        </div>
    </div>
    <div class="form-group">
        <label class="control-label">Шаблон</label>
        <div control-labelstyle="padding-top: 7px;">
            <input type="text" class="form-control" bind:value="{model.pattern}" />
        </div>
    </div>
    <div class="form-group text-center">
        <button class="btn btn-default" on:click="{() => submit()}">Обновить</button>
    </div>
</div>