<script lang="ts">
    import { SystemController } from '../generated-types'
    import { readable } from 'svelte/store';
    
    //@ts-ignore
    import { Link } from 'svero';

    let info;

    SystemController.siteInfo().then(i => info = i);

    const time = readable(new Date().toLocaleString(), set => {
      const interval = setInterval(() => {
        set(new Date().toLocaleString());
      }, 1000);

      return () => clearInterval(interval);
    });

    // used in template
    Link; info; time;
</script>

<nav>
    <div class="header py-4">
        <div class="container">
            <div class="d-flex">
                <a href="/" class="navbar-brand">
                    BudgetTracker {#if info && !info.isProduction}[ReadOnly]{/if}
                </a>
                <div class="ml-auto d-flex order-lg-2">
                    <div class="nav-item">
                        <div class="btn btn-sm btn-outline-success">
                            {$time}
                        </div>
                    </div>
                    <div class="nav-item">
                        <a href="/Auth/Logout" class="btn btn-sm btn-outline-primary">Выход</a>
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
                        <li class="nav-item"><a class="nav-link" href="/Widget">Отчёт</a></li>
                        <li class="nav-item"><Link className="nav-link" href="/Table">История</Link></li>
                        <li class="nav-item"><a class="nav-link" href="/SmsList">SMS</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Payment">ДДС</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Debt">Долги</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Settings">Настройки</a></li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</nav>