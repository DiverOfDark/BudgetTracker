<script>
  import '../node_modules/tabler/dist/css/tabler.min.css';
  import '../node_modules/tabler/dist/css/tabler-buttons.min.css';

  import Router from './svero/Router.svelte';
  import Route from './svero/Route.svelte';
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
            <Route path="/"><WidgetIndex /></Route>
            <Route path="/Table"><TableIndex /></Route>
            <Route path="/Widget/Edit"><WidgetEdit /></Route>
            <Route path="/Widget/Edit/:id"><WidgetEdit /></Route>
            <Route path="/Chart/:provider/:account"><WidgetChart /></Route>
            <Route path="/Chart/:provider/:account/:exemptTransfers"><WidgetChart /></Route>
            <Route path="/Metadata"><MetadataIndex /></Route>
            <Route path="/Metadata/Edit"><MetadataEdit /></Route>
            <Route path="/Metadata/Edit/:id"><MetadataEdit /></Route>
            <Route path="/SmsList"><Sms /></Route>
            <Route path="/SmsList/SmsRules"><SmsRules /></Route>
            <Route path="/Payment"><Payment /></Route>
            <Route path="/Payment/Split/:id"><PaymentSplit /></Route>
            <Route path="/Payment/Edit/:id"><PaymentEdit /></Route>
            <Route path="/Payment/Category"><PaymentCategory /></Route>
            <Route path="/Payment/Category/Edit"><PaymentCategoryEdit /></Route>
            <Route path="/Payment/Category/Edit/:id"><PaymentCategoryEdit /></Route>
            <Route path="/Debt"><Debt /></Route>
            <Route path="/Settings"><Settings /></Route>
            <Route path="/Utility/Tasks"><UtilityTasks /></Route>
            <Route path="/Utility/ScriptConsole"><UtilityScriptConsole /></Route>
            <Route path="/Utility/Screenshot"><UtilityScreenshot /></Route>
            <Route path="*"><NotFound /></Route>
        </Router>
      </div>
    </div>
    <Footer /> 
  {/if}
  <Tooltip />
</div>
