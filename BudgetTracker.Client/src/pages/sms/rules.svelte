<svelte:head>
    <title>BudgetTracker - Правила обработки SMS</title>
</svelte:head>

<script>
    import { SmsListController } from '../../generated-types';
    import { XCircleIcon } from 'svelte-feather-icons';
    import {compare} from '../../services/Shared';

    let load = async function() {
        rules = await SmsListController.smsRules();
        rules = rules.sort((a,b) => {
            if (a.ruleType != b.ruleType)
                return compare(a.ruleType, b.ruleType);

            if (a.sender != b.sender)
                return compare(a.sender, b.sender);

            return compare(a.text, b.text);
        });
    }

    let deleteRule = async function(id) {
        await SmsListController.deleteRule(id);
        await load();
    }

    let rules = [];

    let newRuleType = "Ignore";
    let newSender = "";
    let newText = "";

    let create = async function() {
        await SmsListController.createRule(newRuleType, newSender, newText);
        await load();
    }

    load();
</script>

<div class="container">
    <div class="row row-cards row-deck">
        <div class="col-12">
            <div class="card">
                    <div class="card-header">
                        <h3 class="card-title">
                            Правила обработки SMS
                        </h3>
                    </div>
                    <div class="table-responsive">
                        <table class="table card-table table-sm table-hover table-striped">
                            <thead>
                            <tr>
                                <th>Правило</th>
                                <th>Шаблон отправителя</th>
                                <th>Шаблон сообщения</th>
                                <th>Удалить</th>
                            </tr>
                            </thead>
                            <tbody>
                            {#each rules as rule, idx}
                                <tr>
                                    <td>{rule.ruleType}</td>
                                    <td>{rule.sender || ""}</td>
                                    <td>{rule.text || ""}</td>
                                    <td>
                                        <btn class="btn btn-link btn-anchor" on:click="{() => deleteRule(rule.id)}">
                                            <XCircleIcon size="16" />
                                        </btn>
                                    </td>
                                </tr>
                            {/each}
    
                            <tr>
                                <td>
                                    <select bind:value="{newRuleType}" class="form-control form-control-sm">
                                        <option value="Ignore">Скрывать</option>
                                        <option value="Money">Считать как трату</option>
                                    </select>
                                </td>
                                <td>
                                    <input type="text" bind:value="{newSender}" class="form-control form-control-sm"/>
                                </td>
                                <td>
                                    <input type="text" bind:value="{newText}" class="form-control form-control-sm"/>
                                </td>
                                <td>
                                    <button class="btn btn-sm btn-azure" on:click="{() => create(newRuleType, newSender, newText)}">Добавить</button>
                                </td>
                            </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
        </div>
    </div>
</div>