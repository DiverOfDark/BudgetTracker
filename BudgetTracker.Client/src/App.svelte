<script>
  import '../node_modules/tabler/dist/css/tabler.min.css';
  import '../node_modules/tabler/dist/css/tabler-buttons.min.css';

  import { Router, Route } from 'yrv';
  import AuthService from './services/AuthService';

  import TableIndex from './pages/table/index.svelte';
  import MetadataIndex from './pages/metadata/index.svelte';
  import MetadataEdit from './pages/metadata/edit.svelte';
  import UtilityTasks from './pages/utility/tasks.svelte';
  import UtilityScriptConsole from './pages/utility/scriptconsole.svelte';
  import UtilityScreenshot from './pages/utility/screenshot.svelte';
  import Sms from './pages/sms/index.svelte';
  import SmsRules from './pages/sms/rules.svelte';
  import Payment from './pages/payment/index.svelte';
  import PaymentSplit from './pages/payment/splitPayment.svelte';
  import PaymentEdit from './pages/payment/editPayment.svelte';
  import PaymentCategory from './pages/payment/spentCategories.svelte';
  import PaymentCategoryEdit from './pages/payment/editCategory.svelte';
  import WidgetIndex from './pages/widget/index.svelte'
  import WidgetEdit from './pages/widget/edit.svelte'
  import WidgetChart from './pages/widget/chart.svelte'
  import Debt from './pages/debt/index.svelte';
  import DebtAdd from './pages/debt/add.svelte';
  import Settings from './pages/Settings.svelte';
  import NotFound from './pages/NotFound.svelte';
  import Footer from './components/Footer.svelte'
  import Nav from './components/Nav.svelte';
  import Tooltip from './components/Tooltip.svelte';

  import Auth from './pages/auth.svelte';

  let authorized = AuthService.getStore();

</script>

<div class="page">
  {#if !$authorized}
    <Auth />
  {:else}
    <div class="page-main">
      <Nav />
      <div class="page-content mt-4">
        <Router>
            <Route exact path="/" let:router><WidgetIndex /></Route>
            <Route exact path="/Table" let:router><TableIndex /></Route>
            <Route exact path="/Widget/Edit" let:router><WidgetEdit /></Route>
            <Route exact path="/Widget/Edit/:id" let:router><WidgetEdit /></Route>
            <Route exact path="/Chart/:provider/:account" let:router><WidgetChart /></Route>
            <Route exact path="/Chart/:provider/:account/:exemptTransfers" let:router><WidgetChart /></Route>
            <Route exact path="/Metadata" let:router><MetadataIndex /></Route>
            <Route exact path="/Metadata/Edit" let:router><MetadataEdit /></Route>
            <Route exact path="/Metadata/Edit/:id" let:router><MetadataEdit /></Route>
            <Route exact path="/SmsList" let:router><Sms /></Route>
            <Route exact path="/SmsList/SmsRules" let:router><SmsRules /></Route>
            <Route exact path="/Payment" let:router><Payment /></Route>
            <Route exact path="/Payment/Split/:id" let:router><PaymentSplit /></Route>
            <Route exact path="/Payment/Edit/:id" let:router><PaymentEdit /></Route>
            <Route exact path="/Payment/Category" let:router><PaymentCategory /></Route>
            <Route exact path="/Payment/Category/Edit" let:router><PaymentCategoryEdit /></Route>
            <Route exact path="/Payment/Category/Edit/:id" let:router><PaymentCategoryEdit /></Route>
            <Route exact path="/Debt" let:router><Debt /></Route>
            <Route exact path="/Settings" let:router><Settings /></Route>
            <Route exact path="/Utility/Tasks" let:router><UtilityTasks /></Route>
            <Route exact path="/Utility/ScriptConsole" let:router><UtilityScriptConsole /></Route>
            <Route exact path="/Utility/Screenshot" let:router><UtilityScreenshot /></Route>
            <Route exact fallback let:router><NotFound /></Route>
        </Router>
      </div>
    </div>
    <Footer /> 
  {/if}
  <Tooltip />
</div>
