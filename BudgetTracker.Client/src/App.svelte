<script>
  import { Router, Route } from 'svero';

  import TableIndex from './pages/table/index.svelte';
  import MetadataIndex from './pages/metadata/index.svelte';
  import MetadataEdit from './pages/metadata/edit.svelte';
  import UtilityTasks from './pages/utility/tasks.svelte';
  import UtilityScriptConsole from './pages/utility/scriptconsole.svelte';
  import UtilityScreenshot from './pages/utility/screenshot.svelte';
  import Sms from './pages/sms/index.svelte';
  import SmsRules from './pages/sms/rules.svelte';
  import Debt from './pages/debt/index.svelte';
  import DebtAdd from './pages/debt/add.svelte';
  import Settings from './pages/Settings.svelte';
  import NotFound from './pages/NotFound.svelte';
  import Footer from './components/Footer.svelte'
  import Nav from './components/Nav.svelte';
  import Tooltip from './components/Tooltip.svelte';
</script>

<div class="page">
	<div class="page-main">
		<Nav />
		<div class="page-content">
            <Router>
                <Route path="*" component={NotFound} />
                <Route path="/Table" component={TableIndex} />
                <Route path="/Metadata" component={MetadataIndex} />
                <Route path="/Metadata/Edit" component={MetadataEdit} />
                <Route path="/Metadata/Edit/:id" component={MetadataEdit} />
                <Route path="/SmsList" component={Sms} />
                <Route path="/SmsList/SmsRules" component={SmsRules} />
                <Route path="/Debt" component={Debt} />
                <Route path="/Debt/Edit" component={DebtAdd} />
                <Route path="/Debt/Edit/:id" component={DebtAdd} />
                <Route path="/Settings" component={Settings} />
                <Route path="/Utility/Tasks" component={UtilityTasks} />
                <Route path="/Utility/ScriptConsole" component={UtilityScriptConsole} />
                <Route path="/Utility/Screenshot" component={UtilityScreenshot} />
            </Router>
		</div>
	</div>
	<Footer /> 
    <Tooltip />
</div>
