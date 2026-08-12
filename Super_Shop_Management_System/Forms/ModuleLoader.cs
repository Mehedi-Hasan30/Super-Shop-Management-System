using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public static class ModuleLoader
    {
        public static void LoadModule(Panel container, string moduleName)
        {
            container.Controls.Clear();

            UserControl module = moduleName switch
            {
                "Dashboard" => new DashboardUserControl(),
                "Products" => new ProductUserControl(),
                "Customers" => new CustomerUserControl(),
                "Sales" => new SalesUserControl(),
                "Categories" => new CategoryUserControl(),
                "Suppliers" => new SupplierUserControl(),
                "Employees" => new EmployeeUserControl(),
                "Attendance" => new AttendanceUserControl(),
                "Reports" => new ReportsUserControl(),
                "Settings" => new SettingsUserControl(),
                _ => null
            };

            if (module != null)
            {
                module.Dock = DockStyle.Fill;
                container.Controls.Add(module);
            }
        }
    }
}