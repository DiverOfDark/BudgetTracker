<script lang="ts">
    import { AuthController } from '../generated-types'
    import AuthService from '../services/AuthService'
    import { readable } from 'svelte/store';
    import NavLink from './NavLink.svelte';
    import SoWService from '../services/SoWService';

    let info = SoWService.SystemInfo;

    const time = readable(new Date().toLocaleString(), set => {
      const interval = setInterval(() => {
        set(new Date().toLocaleString());
      }, 1000);

      return () => clearInterval(interval);
    });

    let logout = async function() {
        await AuthController.logout();
        AuthService.logoff();
    }

    // used in template
    NavLink; info; time; logout;
</script>

<nav>
    <div class="header py-4">
        <div class="container">
            <div class="d-flex row">
                <NavLink title="{"BudgetTracker" + ($info && !$info.isProduction ? "[ReadOnly]" : "")}" class="navbar-brand" />
                <div class="ml-auto d-flex order-lg-2">
                    <div class="nav-item">
                        <div class="btn btn-sm btn-outline-success">
                            {$time}
                        </div>
                    </div>
                    <div class="nav-item">
                        <button on:click="{() => logout()}" class="btn btn-sm btn-outline-primary">Выход</button>
                    </div>
                </div>
                <a href="/" onclick="return false;" class="header-toggler d-lg-none ml-3 ml-lg-0" data-toggle="collapse" data-target="#headerMenuCollapse">
                    <span class="header-toggler-icon"></span>
                </a>
            </div>
        </div>
    </div>
    <div class="header d-lg-flex p-0 collapse" id="headerMenuCollapse">
        <div class="container">
            <div class="row align-items-center">
                <div class="col">
                    <ul class="nav nav-tabs border-0 flex-column flex-lg-row">
                        <li class="nav-item"><NavLink title="Отчёт" href="/"/></li>
                        <li class="nav-item"><NavLink title="История" href="/Table"/></li>
                        <li class="nav-item"><NavLink title="SMS" href="/SmsList"/></li>
                        <li class="nav-item"><NavLink title="ДДС" href="/Payment"/></li>
                        <li class="nav-item"><NavLink title="Долги" href="/Debt"/></li>
                        <li class="nav-item"><NavLink title="Настройки" href="/Settings"/></li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</nav>