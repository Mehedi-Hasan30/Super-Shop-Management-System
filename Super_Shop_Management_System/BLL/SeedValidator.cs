using System;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.BLL
{
    public static class SeedValidator
    {
        private static bool _alreadyRun;

        public static async Task EnsureInitializedAsync()
        {
            if (_alreadyRun)
            {
                return;
            }

            try
            {
                SeedDataService service = new SeedDataService();
                await service.EnsureSeedDataAsync();
                _alreadyRun = true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SeedValidator.EnsureInitializedAsync", ex);
            }
        }
    }
}
