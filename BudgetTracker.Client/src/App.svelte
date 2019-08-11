<script>
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
      <div class="page-content">
        <Router>
            <Route path="/" component={WidgetIndex} />
            <Route path="/Table" component={TableIndex} />
            <Route path="/Widget/Edit" component={WidgetEdit} />
            <Route path="/Widget/Edit/:id" component={WidgetEdit} />
            <Route path="/Chart/:provider/:account" component={WidgetChart} />
            <Route path="/Chart/:provider/:account/:exemptTransfers" component={WidgetChart} />
            <Route path="/Metadata" component={MetadataIndex} />
            <Route path="/Metadata/Edit" component={MetadataEdit} />
            <Route path="/Metadata/Edit/:id" component={MetadataEdit} />
            <Route path="/SmsList" component={Sms} />
            <Route path="/SmsList/SmsRules" component={SmsRules} />
            <Route path="/Payment" component={Payment} />
            <Route path="/Payment/Split/:id" component={PaymentSplit} />
            <Route path="/Payment/Edit/:id" component={PaymentEdit} />
            <Route path="/Payment/Category" component={PaymentCategory} />
            <Route path="/Payment/Category/Edit" component={PaymentCategoryEdit} />
            <Route path="/Payment/Category/Edit/:id" component={PaymentCategoryEdit} />
            <Route path="/Debt" component={Debt} />
            <Route path="/Debt/Edit" component={DebtAdd} />
            <Route path="/Debt/Edit/:id" component={DebtAdd} />
            <Route path="/Settings" component={Settings} />
            <Route path="/Utility/Tasks" component={UtilityTasks} />
            <Route path="/Utility/ScriptConsole" component={UtilityScriptConsole} />
            <Route path="/Utility/Screenshot" component={UtilityScreenshot} />
            <Route path="*" component={NotFound} />
        </Router>
      </div>
    </div>
    <Footer /> 
  {/if}
  <Tooltip />
</div>
