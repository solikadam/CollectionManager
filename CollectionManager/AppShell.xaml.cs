using CollectionManager.Views;
using System.Collections;

namespace CollectionManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CollectionPage), typeof(CollectionPage));
        }
    }
}