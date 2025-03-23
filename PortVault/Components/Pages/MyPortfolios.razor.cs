using Microsoft.AspNetCore.Components;
using PortVault.Models;
using PortVault.Services.MutualFund;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Components.Pages
{
    public partial class MyPortfolios : ComponentBase
    {
        [Inject]
        private IMutualFundService MutualFundService { get; set; }

        private string searchText = "";
        private List<MutualFundModel> searchResults = new();
        private List<MutualFundModel> selectedFunds = new();

        protected override async Task OnInitializedAsync()
        {
            await MutualFundService.EnsureFundsExistAsync();
        }

        // Search function triggered when input text changes
        private async Task OnSearchChanged(ChangeEventArgs e)
        {
            searchText = e.Value?.ToString() ?? "";
            if (searchText.Length >= 4)
            {
                searchResults = await MutualFundService.SearchFundsAsync(searchText);
            }
            else
            {
                searchResults.Clear();
            }
            await InvokeAsync(StateHasChanged);
        }

        private void AddFundToPortfolio(MutualFundModel fund)
        {
            if (!selectedFunds.Any(f => f.SchemeCode == fund.SchemeCode))
            {
                selectedFunds.Add(fund);
            }
            searchResults.Clear();
        }

        private void RemoveFund(MutualFundModel fund)
        {
            selectedFunds.Remove(fund);
        }
    }   
}
