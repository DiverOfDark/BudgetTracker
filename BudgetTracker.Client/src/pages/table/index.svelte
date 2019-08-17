<svelte:head>
	<title>BudgetTracker - История</title>
</svelte:head>
{#if !vm}
<div class="container text-center">
    <div class="display-1 text-muted mb-5"><i class="si si-exclamation"></i>Загрузка...</div>
</div>
{:else}
	<div class="{vm && vm.headers.length > 8 ? 'container-fluid' : 'container'}">
		<div class="row row-cards row-deck">
			<div class="col-12">
				<div class="card">
					<div class="card-header">
						<h3 class="card-title">История</h3> 
						<div style="width: 200px; margin-left: 15px;">
							<select class="form-control form-control-sm" id="providerSelector" bind:value={provider} on:change={() => changeProvider()}>
                                {#each providers as i}
                                    {#if i === provider}
                                        <option name="{i}" value="{i}" selected="selected">{i}</option>
                                    {:else}
                                        <option name="{i}" value="{i}">{i}</option>
                                    {/if}
                                {/each}
								<optgroup label="-------------------">
									<option name="BadOption" value="BadOption">Проблемные</option>
								</optgroup>
							</select>
						</div>
						<div class="card-options">
							<Link className="btn btn-secondary btn-sm ml-2" href="/Metadata">
								<span class="small fe fe-edit-2"></span>
							</Link>
							<button class="btn btn-secondary btn-sm ml-2" on:click="{ () => showDelta = !showDelta }">
								{#if showDelta}
									&Delta;
								{:else}
									&Tau;
								{/if}
							</button>
							<button class="btn btn-secondary btn-sm ml-2" on:click="{ () => showControls = !showControls }">
								{#if showControls}
									Скрыть кнопки
								{:else}
									Показать кнопки
								{/if}
							</button>
							<button class="btn btn-secondary btn-sm ml-2" on:click="{ () => exemptTransfers = !exemptTransfers }">
								{#if exemptTransfers}
									Показать переводы
								{:else}
									Скрыть переводы
								{/if}
							</button>
						</div> 
					</div>
					{#if vm.headers && vm.values}
					<div class="table-responsive">
						<table class="table table-sm table-vcenter card-table table-hover table-striped text-nowrap">
							<tr>
								<th scope="col" rowspan="2">Когда</th>
								{#each getGroupedHeaders(vm.headers) as groupedHeader}
									<th scope="col" colspan="{groupedHeader.count}">
										{groupedHeader.name}
									</th>
								{/each}
							</tr>
							<tr>
								{#each vm.headers as p}
									<th title="{p.accountName}">
										{p.userFriendlyName}
										<Link href="/Chart/{encodeURIComponent(p.provider)}/{encodeURIComponent(p.accountName || '')}{encodeURIComponent(p.userFriendlyName || '')}">
											<span class="fe fe-trending-up"></span>
										</Link>
										{#if showControls}
											<Link href="/Metadata/Edit/{p.id}">
												<span class="fe fe-edit-2"></span>
											</Link>
										{/if}
									</th>
								{/each}
							</tr>
							{#each vm.values as item, rowIdx}
								<tr>
									<th class="table-dark" scope="row">
										{formatDate(item.when)}
									</th>
									{#each item.cells as cell, idx}
										<td class="{cell.isOk ? '' : 'table-dark'}">
											{#if canBeCopied(cell, idx)}
												&mdash;
												{#if showControls && hasPreviousCell(vm.values, rowIdx, idx, item.when)}
													<button on:click="{() => copyFromPrevious(vm.headers[idx].id, item.when)}" class="btn btn-sm btn-link btn-anchor">
														<span class="fe fe-copy"></span>
													</button>
													<button on:click="{() => markAsOk(vm.headers[idx].id, item.when)}" class="btn btn-sm btn-link btn-anchor">
														<span class="fe fe-check"></span>
													</button>
												{/if}
											{:else}
												<div use:tooltip="{cell.tooltip}">
													{#if (cell.value === 'NaN')}
														<span class="fe fe-check"></span>
													{:else}
														{#if showDelta}
															{#if cell.diffValue}
																<span class="{cell.diffValue > 0 ? 'text-success' : 'text-danger'}">
																	{formatDiff(cell)}
																</span>
															{:else}
																&mdash;
															{/if}
														{:else}
															{#if getValue(cell, exemptTransfers)}
																<span>{formatPrice(getValue(cell, exemptTransfers))}</span>
															{:else}
																<span>&mdash;</span>
															{/if}
														{/if}

														{#if cell.ccy}
															<span style="font-size: x-small; color: gray;">
																<i>{cell.ccy}</i>
															</span>
														{/if}
														{#if hasPercentage(cell.diffPercentage)}
															<span style="font-size: 0.7em" class="{cell.diffPercentage > 0 ? 'text-success': 'text-danger'}">
																{formatPercentage(cell.diffPercentage)}
															</span>
														{/if}
													{/if}

													{#if showControls && cell.moneyId}
														<button class="btn btn-sm btn-link btn-anchor" style="position: relative; right: 0;" on:click="{() => deleteMoney(cell.moneyId)}">
															<span class="fe fe-x-circle"></span>
														</button>
													{/if}
												</div>
											{/if}
										</td>
									{/each}
								</tr>
							{/each}
						</table>
					</div>
					{/if}

				</div>
			</div>
		</div>
	</div>
{/if}

<script>
	import moment from 'moment'
	import { tooltip } from '../../services/Tooltip'
    import Link from '../../svero/Link.svelte';
	import { TableController } from '../../generated-types.ts'
	import { formatMoney, formatDate } from '../../services/Shared'

	let provider;
	let providers = [];
	let exemptTransfers = false;
	let showDelta = false;
	let showControls = false;
	let vm;

	const fetchData = async (provider, from) => {
	  const data = await TableController.indexJson(provider, from);
	  provider = data.provider;
	  providers = data.providers;
	  vm = data.vm;
	}

	const throttle = (func, limit) => {
	let inThrottle
	return function() {
		const args = arguments
		const context = this
		if (!inThrottle) {
		inThrottle = true
		setTimeout(() => {
			inThrottle = false;
			func.apply(context, args)
		}, limit)
		}
	}
	}

	let deleteMoney = async function(id) {
	  const response = await TableController.deleteMoney(id);
	  fetchData(provider, null);
	};

	let copyFromPrevious = async function(header,date) {
	  const response = await TableController.copyFromPrevious(header, formatDate(date));
	  fetchData(provider, null);
	};

	let markAsOk = async function(header, date) {
	  const response = await TableController.markAsOk(header, formatDate(date));
	  fetchData(provider, null);
	};
	
	let changeProvider = function() {
	  vm.headers = [];
	  vm.values = [];
	  fetchData(provider, null);	
	};

	let getGroupedHeaders = function(headers) {
	  let grouped = headers.reduce((h, x) => {
	    h[x.provider] = (h[x.provider] || 0) + 1;
	    return h;
	  }, []);

	  return Object.keys(grouped).map(function(key){ return { "name": key, "count": grouped[key] }; });;
	};
	
	let hasPreviousCell = function(values, rowIdx, idx, when) {
	  let row = values[rowIdx + 1];
	  if (row && row.cells) {
	    let cell = row.cells[idx];
	    if (cell) {
		  let val = getValue(cell, false);
	      return val != null && val !== 'NaN';
	    }
	  }
	  return false;
	};
	let getValue = function(cell, exemptTransfers) {
	  if (exemptTransfers) {
	    return cell.adjustedValue;
	  }
	  return cell.value;
	};
	let canBeCopied = function(cell, idx) {
		return getValue(cell, false) == null && !vm.headers[idx].isComputed;
	}
	let formatPrice = function(value) {
	  if (!(typeof(value) === 'string' || value instanceof String)) {
	    return formatMoney(value);
	  }
	  return value;
	};
	let formatDiff = function(value) {
	  return value ? value.diffValue : '';
	};
	let hasPercentage = function(percentage) {
	  return percentage && percentage !== 'NaN' && Math.abs(percentage.Value) > 0.0001;
	};
	let formatPercentage = function(percentage) {
	  return (percentage > 0 ? "+" : "") + (percentage * 100).toFixed(2) + '%';
	};

	fetchData("", "");
</script>	