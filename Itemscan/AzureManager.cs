using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Itemscan
{
    public class AzureManager
    {
		private static AzureManager instance;
		private MobileServiceClient client;
        private IMobileServiceTable<ScanItem> scanItemTable;

		private AzureManager()
		{
			this.client = new MobileServiceClient("http://itemscan.azurewebsites.net");
            this.scanItemTable = this.client.GetTable<ScanItem>();
		}

		public MobileServiceClient AzureClient
		{
			get { return client; }
		} 

		public static AzureManager AzureManagerInstance
		{
			get
			{
				if (instance == null)
				{
					instance = new AzureManager();
				}

				return instance;
			}
		}
        public async Task<List<ScanItem>> GetScanItemInformation()
		{
            return await this.scanItemTable.ToListAsync();
		}
		public async Task PostScanItemInformation(ScanItem scanItem)
		{
            await this.scanItemTable.InsertAsync(scanItem);
		}
        public async Task UpdateScanItemInformation(ScanItem scanItem)
        {
            await this.scanItemTable.UpdateAsync(scanItem);
        }
        public async Task DeleteScanItemInformation(ScanItem scanItem)
        {
            await this.scanItemTable.DeleteAsync(scanItem);
        }
    }
}
