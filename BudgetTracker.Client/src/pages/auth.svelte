<script lang="ts">
    import { AuthController } from '../generated-types'
    import AuthService from '../services/AuthService' 

    let error = '';
    let password = '';

    let login = async function() {
        if (await AuthController.login(password)) {
            error = '';
            AuthService.logon();
        } else {
            error = 'Неверный пароль!';
        }
    }

    // used in template
    login; password; error;
</script>

<svelte:head>
    <title>BudgetTracker - Вход</title>
</svelte:head>

<div class="page">
    <div class="page-single">
        <div class="container">
            <div class="row">
                <div class="col col-login mx-auto">
                    {#if error}
                        <div class="alert alert-warning">
                            {error}
                        </div>
                    {/if}
                    <div class="card">
                        <form on:submit|preventDefault="{() => login()}">
                            <div class="card-body p-6">
                                <div class="card-title">Вход в BudgetTracker</div>
                                <div class="form-group">
                                    <label class="form-label">
                                        Пароль
                                    </label>
                                    <input type="password" bind:value="{password}" class="form-control" placeholder="Пароль">
                                </div>
                                <div class="form-footer">
                                    <button type="submit" class="btn btn-primary btn-block">Вход</button>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>